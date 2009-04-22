using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
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

		public ClientInfoLogEntity SetProblem(bool isFree, string problem)
		{
			if (isFree)
				Message = "$$$Бесплатное изменение пароля: " + problem;
			else
				Message = "$$$Платное изменение пароля: " + problem;
			return this;
		}

		public static IList<ClientInfoLogEntity> MessagesForClient(Client client)
		{
			return new List<ClientInfoLogEntity>(FindAll(DetachedCriteria
			                                             	.For<ClientInfoLogEntity>()
			                                             	.Add(Expression.Eq("ClientCode", client.Id))
			                                             	.AddOrder(Order.Desc("WriteTime"))));
		}

		public static ClientInfoLogEntity PasswordChange(uint clientCode, bool isFree, string reason)
		{
			return new ClientInfoLogEntity("", clientCode).SetProblem(isFree, reason);
		}

		public static ClientInfoLogEntity ReseteUin(uint clientCode, string reason)
		{
			return new ClientInfoLogEntity(String.Format("$$$Изменение УИН: " + reason), clientCode);
		}
	}
}
