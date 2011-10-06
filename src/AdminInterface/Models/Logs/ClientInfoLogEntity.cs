using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using System.Linq;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Logs
{
	public enum LogObjectType
	{
		[Description("Поставщик")] Supplier,
		[Description("Клиент")] Client,
		[Description("Пользователь")] User,
		[Description("Адрес")] Address,
		[Description("Отчет")] Report,
	}

	[ActiveRecord(Table = "clientsinfo", Schema = "logs")]
	public class ClientInfoLogEntity : ActiveRecordLinqBase<ClientInfoLogEntity>, IUrlContributor, IAuditRecord
	{
		public ClientInfoLogEntity()
		{}

		public ClientInfoLogEntity(object entity)
		{
			var admin = SecurityContext.Administrator;
			Administrator = admin;
			UserName = admin.UserName;
			WriteTime = DateTime.Now;
			SetObjectInfo(entity);
		}

		public ClientInfoLogEntity(string message, object entity) : this(entity)
		{
			Message = message;
		}

		private void SetObjectInfo(object entity)
		{
			if (entity is DrugstoreSettings)
			{
				entity = ((DrugstoreSettings) entity).Client;
			}

			if (entity is Service)
			{
				var service = ((Service) entity);
				Service = service;
				ObjectId = Service.Id;
				Name = service.Name;
				if (service is Client)
					Type = LogObjectType.Client;
				else
					Type = LogObjectType.Supplier;
			}
			else if (entity is User)
			{
				var user = (User) entity;
				ObjectId = user.Id;
				Service = user.RootService;
				Type = LogObjectType.User;
				Name = user.GetLoginOrName();
			}
			else if (entity is Address)
			{
				var address = ((Address) entity);
				ObjectId = address.Id;
				Service = address.Client;
				Type = LogObjectType.Address;
				Name = address.Value;
			}
			else
			{
				throw new Exception(String.Format("Не знаю как делать сообщения для {0}", entity));
			}
		}

		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public string UserName { get; set; }

		[BelongsTo]
		public Administrator Administrator { get; set;}

		[Property]
		public DateTime WriteTime { get; set; }

		[BelongsTo("ServiceId", NotFoundBehaviour = NotFoundBehaviour.Ignore)]
		public Service Service { get; set; }

		[Property]
		public uint ObjectId { get; set; }

		[Property]
		public LogObjectType Type { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public string Message { get; set; }

		public string Operator
		{
			get
			{
				if (Administrator != null)
					return Administrator.ManagerName;
				return UserName;
			}
		}

		public string HtmlMessage
		{
			get
			{
				return AppealHelper.TnasformRedmineToLink(ViewHelper.FormatMessage(HttpUtility.HtmlEncode(Message)));
			}
		}

		public static LogObjectType GetLogObjectType(object entity)
		{
			var type = NHibernateUtil.GetClass(entity);
			if (type == typeof(Client))
				return LogObjectType.Client;
			if (type == typeof(Supplier))
				return LogObjectType.Supplier;
			if (type == typeof(Address))
				return LogObjectType.Address;
			if (type == typeof(User))
				return LogObjectType.User;

			throw new Exception(String.Format("Не могу определить тип объекта для {0}", entity));
		}

		public ClientInfoLogEntity SetProblem(bool isFree, string username, string problem)
		{
			if (isFree)
				Message = String.Format("$$$Пользователь {0}. Бесплатное изменение пароля: {1}", username, problem);
			else
				Message = String.Format("$$$Пользователь {0}. Платное изменение пароля: {1}", username,problem);
			return this;
		}

		public bool IsStatusChange()
		{
			return Message.Contains("$$$Клиент ");
		}

		public static ClientInfoLogEntity PasswordChange(User user, bool isFree, string reason)
		{
			return new ClientInfoLogEntity("", user).SetProblem(isFree, user.Login, reason);
		}

		public static ClientInfoLogEntity StatusChange(Service service)
		{
			string status;
			if (service.Disabled)
				status = "отключен";
			else
				status = "включен";
			return new ClientInfoLogEntity(String.Format("$$$Клиент {0}", status), service);
		}

		public static ClientInfoLogEntity ReseteUin(Client client, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason), client);
		}

		public static ClientInfoLogEntity ReseteUin(User user, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason + ". $$$ Пользователь: " + user.Login), user);
		}

		public static IList<ClientInfoLogEntity> MessagesForClient(Service service)
		{
			return Queryable
				.Where(l => l.Service == service)
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}

		public static IList<ClientInfoLogEntity> MessagesForUser(User user)
		{
			var objectType = GetLogObjectType(user);
			var serviceType = GetLogObjectType(user.RootService);
			return Queryable
				.Where(l => (l.ObjectId == user.Id && l.Type == objectType) || (l.ObjectId == user.RootService.Id && l.Type == serviceType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}

		public override string ToString()
		{
			return Message;
		}

		public static void UpdateLogs(uint serviceId, uint objectId)
		{
			ArHelper.WithSession(s => s.CreateSQLQuery(@"
update logs.clientsinfo set ServiceId = :serviceId
where ObjectId = :objectId")
				.SetParameter("serviceId", serviceId)
				.SetParameter("objectId", objectId)
				.ExecuteUpdate());
		}

		public IDictionary GetQueryString()
		{
			return new Dictionary<string, string> {
				{"controller", Type.ToString()},
				{"action", "show"},
				{"id", ObjectId.ToString()},
			};
		}
	}
}
