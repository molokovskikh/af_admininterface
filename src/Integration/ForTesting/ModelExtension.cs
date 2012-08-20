using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;

namespace Integration.ForTesting
{
	public static class ModelExtension
	{
		public static int GetUserPriceCount(this User user)
		{
			var result = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select * from Customers.UserPrices where UserId = :userId")
					.SetParameter("userId", user.Id)
					.List());

			return result.Count;
		}

		public static int GetIntersectionCount(this Client client)
		{
			return Convert.ToInt32(ArHelper.WithSession(s =>
				s.CreateSQLQuery("select count(*) from Customers.intersection where ClientId = :ClientId")
					.SetParameter("ClientId", client.Id)
					.UniqueResult()));
		}

		public static int GetAddressIntersectionCount(this Address address)
		{
			return ArHelper.WithSession(s =>
				s.CreateSQLQuery("select * from Customers.AddressIntersection where AddressId = :id")
					.SetParameter("id", address.Id)
					.List())
				.Count;
		}

		public static Payer MakeNameUniq(this Payer payer)
		{
			payer.Save();
			payer.Name += " " + payer.Id;
			payer.Save();
			return payer;
		}
	}
}