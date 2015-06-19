using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Queries;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class OrderFilterFixture : AdmIntegrationFixture
	{
		private User _user;
		private ClientOrder _order;
		[SetUp]
		public void SetUp()
		{
			var supplier = DataMother.CreateSupplier();
			var client = DataMother.CreateTestClientWithAddressAndUser();
			_user = client.Users[0];
			_user.AvaliableAddresses.Add(client.Addresses[0]);

			_order = new ClientOrder(_user, supplier.Prices[0]);

			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			var line = new OrderLine(_order, product, 100, 1);

			Save(supplier, _order, product, line);
			Flush();
		}

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
			session.CreateSQLQuery(@"
insert into Logs.Orders(OrderId, TransportType, ResultCode) values(:orderId, 0, 5);
insert into Logs.Orders(OrderId, TransportType, ResultCode) values(:orderId, 1, 24989)")
				.SetParameter("orderId", _order.Id)
				.ExecuteUpdate();

			var orders = new OrderFilter { User = _user }.Find();
			Assert.That(orders.Count, Is.EqualTo(1));
			Assert.That(orders[0].GetResult(), Is.EqualTo("24989"));
		}

		[Test]
		public void GetStatusForDeletedOrder()
		{
			_order.Deleted = true;
			Save(_order);
			Flush();
			var orders = new OrderFilter { User = _user }.Find();
			Assert.That(orders.Count, Is.EqualTo(1));
			Assert.That(orders.First().GetResult(), Is.EqualTo("Удален"));
		}

		[Test]
		public void GetStatusForNotSubmitedOrder()
		{
			_order.Submited = false;
			Save(_order);
			Flush();
			var orders = new OrderFilter { User = _user }.Find();
			Assert.That(orders.Count, Is.EqualTo(1));
			Assert.That(orders.First().GetResult(), Is.EqualTo("Не подтвержден"));
		}
	}
}