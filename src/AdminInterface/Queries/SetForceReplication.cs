using System;
using AdminInterface.Models;
using AdminInterface.Models.Listeners;
using AdminInterface.Models.Suppliers;
using NHibernate;

namespace AdminInterface.Queries
{
	public class SetForceReplication : IAppQuery
	{
		private Supplier _supplier;
		private User _user;
		private Client _client;

		public SetForceReplication(object entity)
		{
			if (entity is Price)
				_supplier = ((Price)entity).Supplier;
			else if (entity is User)
				_user = (User)entity;
			else if (entity is Client)
				_client = (Client)entity;
			else if (entity is DrugstoreSettings)
				_client = ((DrugstoreSettings)entity).Client;
			else
				_supplier = (Supplier)entity;
		}

		public void ForUser(ISession session, uint id)
		{
			session.CreateSQLQuery(
				@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where UserId = :userId")
				.SetParameter("userId", id)
				.ExecuteUpdate();
		}

		public void ForClient(ISession session, uint id)
		{
			session.CreateSQLQuery(@"
update Usersettings.AnalitFReplicationInfo r
join Customers.Users u on u.Id = r.UserId
set ForceReplication = 1
where u.ClientId = :ClientId")
				.SetParameter("ClientId", id)
				.ExecuteUpdate();
		}

		public void ForSupplier(ISession session, uint id)
		{
			session.CreateSQLQuery(
				@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where FirmCode = :supplierId")
				.SetParameter("supplierId",id)
				.ExecuteUpdate();
		}

		public void Execute(ISession session)
		{
			if (_client != null)
				ForClient(session, _client.Id);
			if (_supplier != null)
				ForSupplier(session, _supplier.Id);
			if (_user != null)
				ForUser(session, _user.Id);
		}
	}
}