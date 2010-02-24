using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "logs.clientsinfo")]
	public class ClientInfoLogEntity : ActiveRecordBase<ClientInfoLogEntity>
	{
		public ClientInfoLogEntity()
		{}

		public ClientInfoLogEntity(string message, uint clientCode)
		{
			UserName = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			Message = message;
			ClientCode = clientCode;
		}

		public ClientInfoLogEntity(string message, uint clientCode, uint userId)
		{
			UserName = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			Message = message;
			ClientCode = clientCode;
			User = User.Find(userId);
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
			return new ClientInfoLogEntity("", user.Client.Id, user.Id).SetProblem(isFree, user.Login, reason);
		}

		public static ClientInfoLogEntity StatusChange(ClientStatus status, uint clientCode)
		{
			return new ClientInfoLogEntity(String.Format("$$$Клиент {0}", BindingHelper.GetDescription(status).ToLower()), clientCode);
		}

		public static ClientInfoLogEntity ReseteUin(uint clientCode, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason), clientCode);
		}

		public static ClientInfoLogEntity ReseteUin(User user, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason + ". $$$ Пользователь: " + user.Login), user.Client.Id, user.Id);
		}

		public static IList<ClientInfoLogEntity> MessagesForClient(Client client)
		{
			return new List<ClientInfoLogEntity>(FindAll(DetachedCriteria
															.For<ClientInfoLogEntity>()
															.Add(Expression.Eq("ClientCode", client.Id))
															.AddOrder(Order.Desc("WriteTime"))));
		}

		public static IList<ClientInfoLogEntity> MessagesForUser(User user)
		{
			return new List<ClientInfoLogEntity>(FindAll(DetachedCriteria
															.For<ClientInfoLogEntity>()
															.Add(Expression.Eq("User", user))
															.AddOrder(Order.Desc("WriteTime"))));
		}
	}
}
