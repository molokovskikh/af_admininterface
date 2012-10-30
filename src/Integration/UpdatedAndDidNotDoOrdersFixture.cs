using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class UpdatedAndDidNotDoOrdersFixture : IntegrationFixture
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
			Flush();
		}

		[TearDown]
		public void Down()
		{
			foreach (var user1 in client.Users) {
				session.Delete(user1);
			}
			session.Delete(client);
			Flush();
		}

		[Test]
		public void No_result_find_test()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			filter.UpdatePeriod.End = DateTime.Now.AddDays(-3);
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
			filter.UpdatePeriod.End = DateTime.Now;
			filter.OrderDate = DateTime.Now.AddDays(-1);
			filter.Session = session;
			var region = session.Query<Region>().First();
			var order = new ClientOrder { Client = client, User = user, WriteTime = DateTime.Now.AddDays(-2), Region = region };
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
