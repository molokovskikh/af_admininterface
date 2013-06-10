using System;
using AdminInterface.Models;
using AdminInterface.Queries;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration.Queries
{
	[TestFixture]
	public class UpdateOrdersFixture : IntegrationFixture
	{
		[Test]
		public void Update_orders()
		{
			var supplier = DataMother.CreateSupplier();

			var newClient = DataMother.TestClient();
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			var address = client.Addresses[0];
			user.AvaliableAddresses.Add(address);
			var query = new UpdateOrders(newClient, user, address);

			var order = new ClientOrder(user, supplier.Prices[0]);

			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			var line = new OrderLine(order, product, 100, 1);
			Save(supplier, product, line, order);

			query.Execute(session);

			session.Refresh(order);
			Assert.That(order.Client, Is.EqualTo(newClient));
		}
	}
}