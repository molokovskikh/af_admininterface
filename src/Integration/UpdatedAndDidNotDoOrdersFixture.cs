using System;
using System.Linq;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

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
			client = DataMother.CreateClientAndUsers();
			session.Save(client);
			user = client.Users.First();
			filter = new UpdatedAndDidNotDoOrdersFilter();
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
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			filter.UpdatePeriod.End = DateTime.Now.AddDays(-3);
			filter.NoOrders = true;
			filter.Session = session;
			session.Save(user);
			Flush();
			var result = filter.Find();
			Assert.AreEqual(result.Count, 0);
		}

		[Test]
		public void Find_test_if_correct_condition()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user.AssignDefaultPermission(session);
			var address = new Address(client) { Value = "123" };
			session.Save(address);
			user.AvaliableAddresses.Add(address);
			var region = session.Query<Region>().First();
			var price = session.Query<Price>().First();
			filter.UpdatePeriod.End = DateTime.Now;
			filter.Regions = new [] { region.Id };
			filter.Session = session;
			var order = new ClientOrder { Client = client, User = user, WriteTime = DateTime.Now.AddDays(-2), Region = region, Price = price };
			session.Save(order);
			session.Save(user);
			Flush();
			var result = filter.Find();
			Assert.AreEqual(result.Count, 1);
		}

		[Test]
		public void No_result_if_correct_condition_and_order()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			filter.UpdatePeriod.End = DateTime.Now;
			filter.Session = session;
			session.Save(user);
			var region = session.Query<Region>().First();
			var order = new ClientOrder { Client = client, User = user, WriteTime = DateTime.Now.AddDays(-2), Region = region };
			session.Save(order);
			Flush();
			var result = filter.Find();
			Assert.AreEqual(result.Count, 0);
		}
	}
}
