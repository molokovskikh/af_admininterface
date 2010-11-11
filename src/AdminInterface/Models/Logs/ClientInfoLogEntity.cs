using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Helpers;
using System.Linq;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "clientsinfo", Schema = "logs")]
	public class ClientInfoLogEntity : ActiveRecordLinqBase<ClientInfoLogEntity>
	{
		public ClientInfoLogEntity()
		{}

		public ClientInfoLogEntity(string message)
		{
			Message = message;
			UserName = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
		}

		public ClientInfoLogEntity(string message, Client client) : this(message)
		{
			ClientCode = client.Id;
		}

		public ClientInfoLogEntity(string message, User user) : this(message)
		{
			ClientCode = user.Client.Id;
			User = user;
		}

		public ClientInfoLogEntity(string message, object entity) : this(message)
		{
			if (entity is Client)
			{
				ClientCode = ((Client)entity).Id;
			}
			else if (entity is User)
			{
				User = (User)entity;
				ClientCode = User.Id;
			}
			else if (entity is Address)
			{
				ClientCode = ((Address)entity).Client.Id;
			}
			else if (entity is DrugstoreSettings)
			{
				ClientCode = ((DrugstoreSettings)entity).Id;
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

		[Property]
		public string Message { get; set; }

		[Property]
		public uint ClientCode { get; set; }

		[Property]
		public DateTime WriteTime { get; set; }

		[BelongsTo(Column = "UserId")]
		public virtual User User { get; set; }

		public string Login
		{
			get { return User.Login; }
		}

		public string Operator
		{
			get { return GetHumanReadableOperatorName(); }
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

		public string GetHumanReadableOperatorName()
		{
			return ViewHelper.GetHumanReadableOperatorName(UserName);
		}

		public static ClientInfoLogEntity PasswordChange(User user, bool isFree, string reason)
		{
			return new ClientInfoLogEntity("", user).SetProblem(isFree, user.Login, reason);
		}

		public static ClientInfoLogEntity StatusChange(ClientStatus status, Client client)
		{
			return new ClientInfoLogEntity(String.Format("$$$Клиент {0}", BindingHelper.GetDescription(status).ToLower()), client);
		}

		public static ClientInfoLogEntity ReseteUin(Client client, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason), client);
		}

		public static ClientInfoLogEntity ReseteUin(User user, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason + ". $$$ Пользователь: " + user.Login), user);
		}

		public static IList<ClientInfoLogEntity> MessagesForClient(Client client)
		{
			return Queryable
				.Where(l => l.ClientCode == client.Id)
				.OrderByDescending(l => l.WriteTime)
				.ToList();
		}

		public static IList<ClientInfoLogEntity> MessagesForUser(User user)
		{
			return Queryable
				.Where(l => l.User == user || (l.ClientCode == user.Client.Id && l.User == null))
				.OrderByDescending(l => l.WriteTime)
				.ToList();
		}
	}
}
