using System;
using System.Linq;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class UpdatedAndDidNotDoOrdersFixture : AdmIntegrationFixture
	{
		private User user;
		private Client client;
		private UpdatedAndDidNotDoOrdersFilter filter;

		[SetUp]
		public void SetUp()
		{
			client = DataMother.TestClient(x => {
				x.AddUser(new User(x) {
					Name = "test"
				});
				x.AddUser(new User(x) {
					Name = "test"
				});
				var address = x.AddAddress("Тестовый адрес");
				x.Users.Each(y => y.AvaliableAddresses.Add(address));
			});
			session.Save(client);
			user = client.Users.First();
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			filter = new UpdatedAndDidNotDoOrdersFilter {
				Session = session
			};
		}

		[TearDown]
		public void Down()
		{
			session.DeleteEach(client.Users);
			session.Delete(client);
		}

		[Test]
		public void No_result_find_test()
		{
			filter.UpdatePeriod.End = DateTime.Now.AddDays(-3);
			session.Save(user);
			Flush();
			var result = filter.Find();
			Assert.AreEqual(result.Count, 0);
		}

		[Test]
		public void Find_test_if_correct_condition()
		{
			user.AssignDefaultPermission(session);
			var address = new Address(client) { Value = "123" };
			session.Save(address);
			user.AvaliableAddresses.Add(address);
			var region = session.Query<Region>().First();
			var price = session.Query<Price>().First();
			filter.UpdatePeriod.End = DateTime.Now;
			filter.Regions = new [] { region.Id };
			var order = new ClientOrder(user, price) { WriteTime = DateTime.Now.AddDays(-2) };
			session.Save(order);
			session.Save(user);
			Flush();
			var result = filter.Find();
			Assert.AreEqual(result.Count, 1);
		}

		[Test]
		public void No_order_for_supplier()
		{
			user.AssignDefaultPermission(session);
			var supplier1 = DataMother.CreateSupplier(x => x.Name = Guid.NewGuid().ToString());
			session.Save(supplier1);
			var supplier2 = DataMother.CreateSupplier(x => x.Name = Guid.NewGuid().ToString());
			session.Save(supplier2);
			var order = new ClientOrder(user, supplier1.Prices[0]) { WriteTime = DateTime.Now.AddDays(-2) };
			session.Save(order);
			filter.Suppliers = new []{ supplier1.Id, supplier2.Id };
			session.Flush();
			var result = filter.Find();
			Assert.That(result.Count, Is.GreaterThan(0));
			var item = result.FirstOrDefault(x => x.InnerUserId == user.Id.ToString());
			Assert.IsNotNull(item, $"не найдена запись для пользователя {user.Id}");
			Assert.AreEqual(item.NoOrderSuppliers, supplier2.Name);
		}
	}
}
