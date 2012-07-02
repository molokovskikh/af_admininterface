using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class OrderFilterFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Find_not_sended_orders()
		{
			new OrderFilter {
				NotSent = true
			}.Find();
		}

		[Test]
		public void Find_order_for_user()
		{
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			var user = client.Users.First();
			new OrderFilter {
				User = user,
				Client = client
			}.Find();
		}

		[Test]
		public void Select_result_for_max_id()
		{
			var supplier = DataMother.CreateSupplier();
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			user.AvaliableAddresses.Add(client.Addresses[0]);

			var order = new ClientOrder(user, supplier.Prices[0]);

			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			var line = new OrderLine(order, product, 100, 1);

			Save(supplier, order, product, line);
			session.CreateSQLQuery(@"
insert into Logs.Orders(OrderId, TransportType, ResultCode) values(:orderId, 0, 5);
insert into Logs.Orders(OrderId, TransportType, ResultCode) values(:orderId, 1, 24989)")
				.SetParameter("orderId", order.Id)
				.ExecuteUpdate();

			var orders = new OrderFilter{User = user}.Find();
			Assert.That(orders.Count, Is.EqualTo(1));
			Assert.That(orders[0].GetResult(), Is.EqualTo("24989"));
		}
	}
}