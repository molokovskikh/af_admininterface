using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using Common.Tools;

namespace AdminInterface.Queries
{
	public class UpdateOrders
	{
		public Client Client;
		public User User;
		public Address Address;

		public UpdateOrders(Client client, User user, Address address)
		{
			Client = client;
			User = user;
			Address = address;
		}

		public void Execute(ISession session)
		{
			var query = new DetachedSqlQuery();
			var sql = new List<string>();
			var head = "update {0}.OrdersHead"
				+ " set ClientCode = :clientId";
			if (User != null) {
				sql.Add(head + " where UserId = :userId");
				query.SetParameter("userId", User.Id);
			}
			if (Address != null) {
				sql.Add(head + " where AddressId = :addressId");
				query.SetParameter("addressId", Address.Id);
			}

			sql = sql.Select(s => String.Format(s, "Orders")).ToList();
#if !DEBUG
			sql.AddRange(sql.Select(s => String.Format(s, "OrdersOld")).ToList());
#endif
			query.Sql = sql.Implode(";");
			query.SetParameter("clientId", Client.Id);

			query.GetExecutableQuery(session)
				.ExecuteUpdate();
		}
	}
}