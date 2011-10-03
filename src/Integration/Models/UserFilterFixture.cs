using System;
using System.Linq;
using AdminInterface.Models;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class UserFilterFixture : IntegrationFixture
	{
		[Test]
		public void Search_by_address_mail()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			var address = client.Addresses.First();
			address.AvaliableForUsers.Add(client.Users[0]);
			Flush();

			var filter = new UserFilter();
			filter.SearchBy = SearchUserBy.AddressMail;
			filter.SearchText = String.Format("{0}@waybills.analit.net", address.Id);
			var result = filter.Find();

			Assert.That(result.Count, Is.EqualTo(1), result.Implode(r => r.UserId.ToString()));
			Assert.That(result[0].UserId, Is.EqualTo(user.Id));
		}
	}
}