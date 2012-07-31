using AdminInterface.Models;
using NHibernate;

namespace AdminInterface.Queries
{
	public class ResetReclameDate
	{
		private Client _client;

		public ResetReclameDate(Client client)
		{
			_client = client;
		}

		public void Execute(ISession session)
		{
			session.CreateSQLQuery(@"update Usersettings.UserUpdateInfo i
join Customers.Users u on u.Id = i.UserId
set ReclameDate = null
where u.ClientId = :clientId")
				.SetParameter("clientId", _client.Id)
				.ExecuteUpdate();
		}
	}
}