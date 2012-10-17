using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;

namespace AdminInterface.Models.Logs
{
	public enum LogObjectType
	{
		[Description("Поставщик")] Supplier,
		[Description("Клиент")] Client,
		[Description("Пользователь")] User,
		[Description("Адрес")] Address,
		[Description("Отчет")] Report,
		[Description("Плательщик")] Payer
	}

	public enum LogMessageType
	{
		[Description("Пользовательское")] User,
		[Description("Системное")] System,
		[Description("Статистическое")] Stat,
		[Description("Плательщика")] Payer
	}

	[ActiveRecord(Table = "clientsinfo", Schema = "logs")]
	public class AuditRecord : ActiveRecordLinqBase<AuditRecord>, IUrlContributor, IAuditRecord
	{
		public AuditRecord()
		{
		}

		public AuditRecord(object entity)
		{
			var admin = SecurityContext.Administrator;
			Administrator = admin;
			UserName = admin.UserName;
			WriteTime = DateTime.Now;
			SetObjectInfo(entity);
		}

		public AuditRecord(string message, object entity) : this(entity)
		{
			Message = message;
		}

		private void SetObjectInfo(object entity)
		{
			if (entity is DrugstoreSettings) {
				entity = ((DrugstoreSettings)entity).Client;
			}

			if (entity is Service) {
				var service = ((Service)entity);
				Service = service;
				ObjectId = Service.Id;
				Name = service.Name;
				if (service.Type == ServiceType.Drugstore)
					Type = LogObjectType.Client;
				if (service.Type == ServiceType.Supplier)
					Type = LogObjectType.Supplier;
			}
			else if (entity is User) {
				var user = (User)entity;
				ObjectId = user.Id;
				Service = user.RootService;
				Type = LogObjectType.User;
				Name = user.GetLoginOrName();
			}
			else if (entity is Address) {
				var address = ((Address)entity);
				ObjectId = address.Id;
				Service = address.Client;
				Type = LogObjectType.Address;
				Name = address.Value;
			}
			else {
				throw new Exception(String.Format("Не знаю как делать сообщения для {0}", entity));
			}
		}

		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public string UserName { get; set; }

		[BelongsTo]
		public Administrator Administrator { get; set; }

		[Property]
		public DateTime WriteTime { get; set; }

		[BelongsTo("ServiceId", NotFoundBehaviour = NotFoundBehaviour.Ignore)]
		public Service Service { get; set; }

		[Property]
		public uint ObjectId { get; set; }

		[Property]
		public LogObjectType Type { get; set; }

		[Property]
		public LogMessageType MessageType { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public string Message { get; set; }

		[Property]
		public bool IsHtml { get; set; }

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
				if (IsHtml)
					return Message;
				return AppealHelper.TnasformRedmineToLink(ViewHelper.FormatMessage(Message));
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

		public AuditRecord SetProblem(bool isFree, string username, string problem)
		{
			if (isFree)
				Message = String.Format("$$$Пользователь {0}. Бесплатное изменение пароля: {1}", username, problem);
			else
				Message = String.Format("$$$Пользователь {0}. Платное изменение пароля: {1}", username, problem);
			return this;
		}

		[Style]
		public bool IsDisabled
		{
			get
			{
				return Message.Contains("$$$Клиент включен")
					|| Message.Contains("$$$Клиент отключен")
					|| Message.Contains("$$$Изменено 'Включен'");
			}
		}

		public static AuditRecord PasswordChange(User user, bool isFree, string reason)
		{
			return new AuditRecord("", user).SetProblem(isFree, user.Login, reason);
		}

		public static AuditRecord StatusChange(Service service)
		{
			string status;
			if (service.Disabled)
				status = "отключен";
			else
				status = "включен";
			return new AuditRecord(String.Format("$$$Клиент {0}", status), service) { MessageType = LogMessageType.System };
		}

		public static AuditRecord ReseteUin(Client client, string reason)
		{
			return new AuditRecord(String.Format("$$$Изменение УИН: " + reason), client);
		}

		public static AuditRecord ReseteUin(User user, string reason)
		{
			return new AuditRecord(String.Format("$$$Изменение УИН: " + reason + ". $$$ Пользователь: " + user.Login), user);
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
				{ "controller", Type.ToString() },
				{ "action", "show" },
				{ "id", ObjectId.ToString() },
			};
		}

		public static void DeleteAuditRecords(object entity)
		{
			var auditRecord = new AuditRecord(entity);
			ArHelper.WithSession(s => {
				s.CreateSQLQuery("delete from Logs.ClientsInfo where ObjectId = :Id and Type = :Type")
					.SetParameter("Id", auditRecord.ObjectId)
					.SetParameter("Type", auditRecord.Type)
					.ExecuteUpdate();
			});
		}
	}
}