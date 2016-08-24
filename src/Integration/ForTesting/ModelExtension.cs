using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using NHibernate;

namespace Integration.ForTesting
{
	public static class ModelExtension
	{
		public static int GetUserPriceCount(this User user, ISession session)
		{
			return session
				.CreateSQLQuery("select * from Customers.UserPrices where UserId = :userId")
				.SetParameter("userId", user.Id)
				.List()
				.Count;
		}

		public static int GetIntersectionCount(this Client client, ISession session)
		{
			return Convert.ToInt32(
				session.CreateSQLQuery("select count(*) from Customers.intersection where ClientId = :ClientId")
					.SetParameter("ClientId", client.Id)
					.UniqueResult());
		}

		public static int GetAddressIntersectionCount(this Address address, ISession session)
		{
			return session.CreateSQLQuery("select * from Customers.AddressIntersection where AddressId = :id")
				.SetParameter("id", address.Id)
				.List().Count;
		}

		public static Payer MakeNameUniq(this Payer payer)
		{
			payer.Save();
			payer.Name += " " + payer.Id;
			payer.Save();
			return payer;
		}

		public static void DisablePrice(this User parent, ISession session, uint priceId)
		{
			session.CreateSQLQuery(@"
delete from Customers.UserPrices
where UserId = :userId and priceId = :priceId")
				.SetParameter("userId", parent.Id)
				.SetParameter("priceId", priceId)
				.ExecuteUpdate();
		}
	}
}