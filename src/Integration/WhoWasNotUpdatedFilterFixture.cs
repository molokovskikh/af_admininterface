﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class WhoWasNotUpdatedFilterFixture : IntegrationFixture
	{
		private User user;
		private User user1;
		private User user2;
		private User user3;

		private Address address;
		private Address address2;

		private Client client;

		[SetUp]
		public void SetUp()
		{
			client = DataMother.TestClient();
			session.Save(client);

			user = new User(client) { Login = "user", Name = "user" };
			user1 = new User(client) { Login = "user1", Name = "user1" };
			user2 = new User(client) { Login = "user2", Name = "user2" };
			user3 = new User(client) { Login = "user3", Name = "user3" };
			client.AddUser(user);
			client.AddUser(user1);
			client.AddUser(user2);
			client.AddUser(user3);

			address = new Address { Value = "123", Client = client };
			address2 = new Address { Value = "123", Client = client };
			client.AddAddress(address);
			client.AddAddress(address2);

			address.AvaliableForUsers.Add(user);
			address.AvaliableForUsers.Add(user1);
			address.AvaliableForUsers.Add(user2);
			address.AvaliableForUsers.Add(user3);
			address2.AvaliableForUsers.Add(user3);

			user.UserUpdateInfo = new UserUpdateInfo { User = user, AFCopyId = User.GetTempLogin() };
			user1.UserUpdateInfo = new UserUpdateInfo { User = user1, AFCopyId = User.GetTempLogin() };
			user2.UserUpdateInfo = new UserUpdateInfo { User = user2, AFCopyId = User.GetTempLogin() };
			user3.UserUpdateInfo = new UserUpdateInfo { User = user3, AFCopyId = User.GetTempLogin() };

			session.Save(user);
			session.Save(user1);
			session.Save(user2);
			session.Save(user3);

			session.Save(client);

			Flush();

			session.Save(address);
			session.Save(address2);
		}

		[TearDown]
		public void Down()
		{
			session.Delete(user);
			session.Delete(user1);
			session.Delete(user2);
			session.Delete(user3);

			session.Delete(address);
			session.Delete(address2);

			session.Delete(client);

			Flush();
		}

		[Test]
		public void All_in_result()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user1.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user2.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user3.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);

			session.Save(user);
			session.Save(user1);
			session.Save(user2);
			session.Save(user3);

			Flush();

			var filter = new WhoWasNotUpdatedFilter { Period = new DatePeriod(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)) };

			var data = filter.SqlQuery2(session);
			Assert.AreEqual(data.Count, 4);
		}

		[Test]
		public void Only_multi_user_in_result()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user1.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user2.UserUpdateInfo.UpdateDate = DateTime.Now;
			user3.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);

			session.Save(user);
			session.Save(user1);
			session.Save(user2);
			session.Save(user3);

			Flush();

			var filter = new WhoWasNotUpdatedFilter { Period = new DatePeriod(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)) };

			var data = filter.SqlQuery2(session);
			Assert.AreEqual(data.Count, 1);
			Assert.That(data[0].UserName, Is.EqualTo("user3"));
		}

		[Test]
		public void Only_ones_users_in_result()
		{
			user.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user1.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user2.UserUpdateInfo.UpdateDate = DateTime.Now.AddDays(-2);
			user3.UserUpdateInfo.UpdateDate = DateTime.Now;

			session.Save(user);
			session.Save(user1);
			session.Save(user2);
			session.Save(user3);

			Flush();

			var filter = new WhoWasNotUpdatedFilter { Period = new DatePeriod(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)) };

			var data = filter.SqlQuery2(session).Select(d => d.UserName).ToList();
			Assert.AreEqual(data.Count, 3);
			Assert.IsTrue(data.Contains("user"));
			Assert.IsTrue(data.Contains("user1"));
			Assert.IsTrue(data.Contains("user2"));
		}
	}
}