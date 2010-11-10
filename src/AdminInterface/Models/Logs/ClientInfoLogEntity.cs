using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using System.Linq;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "clientsinfo", Schema = "logs")]
	public class ClientInfoLogEntity : ActiveRecordLinqBase<ClientInfoLogEntity>
	{
		public ClientInfoLogEntity()
		{}

		public ClientInfoLogEntity(string message, Client client)
		{
			UserName = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			Message = message;
			ClientCode = client.Id;
		}

		public ClientInfoLogEntity(string message, User user)
		{
			UserName = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			Message = message;
			ClientCode = user.Client.Id;
			User = user;
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

		public static IList<ClientInfoLogEntity> MessagesForUserAndClient(User user)
		{
			var messages = (List<ClientInfoLogEntity>)MessagesForUser(user);
			messages.AddRange(FindAll(DetachedCriteria
				.For<ClientInfoLogEntity>()
				.Add(Expression.Eq("ClientCode", user.Client.Id))
				.Add(Expression.IsNull("User"))));
			return messages.OrderByDescending(item => item.WriteTime).ToList();
		}
	}

	public static class ClientInfoLogEntityExtension
	{
		public static IList<ClientInfoLogEntity> OrderBy(this IList<ClientInfoLogEntity> list, string columnName, bool descending)
		{
			if (columnName.Equals("WriteTime", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return list.OrderByDescending(item => item.WriteTime).ToList();
				return list.OrderBy(item => item.WriteTime).ToList();
			}
			if (columnName.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return list.OrderByDescending(item => item.User != null ? item.User.GetLoginOrName() : String.Empty).ToList();
				return list.OrderBy(item => item.User != null ? item.User.GetLoginOrName() : String.Empty).ToList();
			}
			if (columnName.Equals("Operator", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return list.OrderByDescending(item => ViewHelper.GetHumanReadableOperatorName(item.UserName)).ToList();
				return list.OrderBy(item => ViewHelper.GetHumanReadableOperatorName(item.UserName)).ToList();
			}
			return list.OrderByDescending(item => item.WriteTime).ToList();
		}
	}
}
