using AdminInterface.Models;
using AdminInterface.Models.Listeners;
using NHibernate;

namespace AdminInterface.Queries
{
	public class ResetReclameDate : ITriggerQuery
	{
		private Client _client;

		public ResetReclameDate(object entity)
		{
			if (entity is Client)
				_client = (Client)entity;
			else
				_client = ((DrugstoreSettings)entity).Client;
		}

		public void Execute(ISession session)
		{
			session.CreateSQLQuery(@"
update Usersettings.UserUpdateInfo i
join Customers.Users u on u.Id = i.UserId
set ReclameDate = null
where u.ClientId = :clientId;

update Customers.AnalitFNetDatas d
join Customers.Users u on u.Id = d.UserId
set LastReclameUpdateAt = null,
	LastPendingReclameUpdateAt = null
where u.ClientId = :clientId;")
				.SetParameter("clientId", _client.Id)
				.ExecuteUpdate();
		}
	}
}