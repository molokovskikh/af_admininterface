using System;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;

namespace Functional.ForTesting
{
	public static class ModelExtension
	{
		public static int GetUserPriceCount(this User user)
		{
			var result = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select * from future.UserPrices where UserId = :userId")
				.SetParameter("userId", user.Id)
				.List());

			return result.Count;
		}

		public static int GetIntersectionCount(this Client client)
		{
			return Convert.ToInt32(ArHelper.WithSession(s =>
					s.CreateSQLQuery("select count(*) from future.intersection where ClientId = :ClientId")
					.SetParameter("ClientId", client.Id)
					.UniqueResult()));
		}

		public static int GetAddressIntersectionCount(this Address address)
		{
			return ArHelper.WithSession(s =>
					s.CreateSQLQuery("select * from future.AddressIntersection where AddressId = :id")
					.SetParameter("id", address.Id)
					.List()).Count;
		}
	}
}