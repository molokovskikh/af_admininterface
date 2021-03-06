﻿using System.Linq;
using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class WorkingPricesInheritanceFixture : AdmIntegrationFixture
	{
		[Test]
		public void Inherit_working_prices()
		{
			var client = DataMother.TestClient();
			var parent = new User(client);
			var child = new User(client);
			client.AddUser(parent);
			client.AddUser(child);
			parent.Setup(session);
			child.Setup(session);
			Flush();

			var prices = session.CreateSQLQuery(@"
select PriceId
from Customers.UserPrices
where UserId = :id")
				.SetParameter("id", parent.Id)
				.List();

			Assert.That(prices, Is.Not.Empty);
			parent.DisablePrice(session, prices.Cast<uint>().First());

			child.InheritPricesFrom = parent;
			session.SaveOrUpdate(parent);

			var pricesForParent = session.CreateSQLQuery(@"
call Customers.GetPrices(:id);
select PriceCode from Usersettings.Prices;")
					.SetParameter("id", parent.Id)
					.List();

			var pricesForChild = session.CreateSQLQuery(@"
drop temporary table Usersettings.Prices;
call Customers.GetPrices(:id);
select PriceCode from Usersettings.Prices")
					.SetParameter("id", child.Id)
					.List();

			Assert.That(pricesForParent.Count, Is.EqualTo(pricesForChild.Count));
			Assert.That(pricesForParent.Cast<uint>().ToArray(), Is.EquivalentTo(pricesForChild.Cast<uint>().ToArray()));
		}
	}
}