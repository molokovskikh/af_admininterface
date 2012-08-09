﻿using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class WorkingPricesInheritanceFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Inherit_working_prices()
		{
			var client = DataMother.TestClient();
			var parent = new User(client);
			var child = new User(client);
			client.AddUser(parent);
			client.AddUser(child);
			parent.Setup();
			child.Setup();
			Flush();

			ArHelper.WithSession(s => {
				var prices = s.CreateSQLQuery(@"
select PriceId
from Customers.UserPrices
where UserId = :id")
				.SetParameter("id", parent.Id)
				.List();

				Assert.That(prices, Is.Not.Empty);
				s.CreateSQLQuery(@"
delete from Customers.UserPrices
where UserId = :userId and priceId = :priceId")
					.SetParameter("userId", parent.Id)
					.SetParameter("priceId", prices.Cast<uint>().First())
					.ExecuteUpdate();

				child.InheritPricesFrom = parent;
				session.SaveOrUpdate(parent);
			});

			var pricesForParent = ArHelper.WithSession(s =>
				s.CreateSQLQuery(@"
call Customers.GetPrices(:id);
select PriceCode from Usersettings.Prices;")
				.SetParameter("id", parent.Id)
				.List());

			var pricesForChild = ArHelper.WithSession(s =>
				s.CreateSQLQuery(@"
drop temporary table Usersettings.Prices;
call Customers.GetPrices(:id);
select PriceCode from Usersettings.Prices")
				.SetParameter("id", child.Id)
				.List());

			Assert.That(pricesForParent.Count, Is.EqualTo(pricesForChild.Count));
			Assert.That(pricesForParent.Cast<uint>().ToArray(), Is.EquivalentTo(pricesForChild.Cast<uint>().ToArray()));
		}
	}
}
