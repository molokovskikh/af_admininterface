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
using AdminInterface.Queries;

namespace Integration
{
	[TestFixture]
	public class WhoWasNotUpdatedFilterFixture : AdmIntegrationFixture
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
			session.CreateSQLQuery(@"
update Customers.Users set InheritPricesFrom = null;
delete from customers.Users;
delete from Billing.Accounts where Type = 0;")
				.ExecuteUpdate();

			client = DataMother.TestClient();
			session.Save(client);

			user = new User(client) { Login = "user", Name = "user" };
			user1 = new User(client) { Login = "user1", Name = "user1" };
			user2 = new User(client) { Login = "user2", Name = "user2" };
			user3 = new User(client) { Login = "user3", Name = "user3" };
			user.AssignDefaultPermission(session);
			user1.AssignDefaultPermission(session);
			user2.AssignDefaultPermission(session);
			user3.AssignDefaultPermission(session);
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
			client.Delete(session);
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

			var filter = new WhoWasNotUpdatedFilter {Session = session, BeginDate = DateTime.Now.AddDays(-1), Regions = new ulong[] { client.HomeRegion.Id } };

			var data = filter.Find();
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

			var filter = new WhoWasNotUpdatedFilter { Session = session, BeginDate = DateTime.Now.AddDays(-1) };

			var data = filter.Find();
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

			var filter = new WhoWasNotUpdatedFilter { Session = session, BeginDate = DateTime.Now.AddDays(-1) };

			var data = filter.Find().Select(d => d.UserName).ToList();
			Assert.AreEqual(data.Count, 3);
			Assert.IsTrue(data.Contains("user"));
			Assert.IsTrue(data.Contains("user1"));
			Assert.IsTrue(data.Contains("user2"));
		}

		[Test]
		public void Analysis_Of_Work_Drugstores_Filter_Test()
		{
			var filter = new AnalysisOfWorkDrugstoresFilter
			{
				Session = session,
				ObjectId = client.Id,
				AutoOrder = (int)AutoOrderStatus.NotUsed
			};

			// настроен ли у клиента автозаказ
			var price = client.Settings.SmartOrderRules.AssortimentPriceCode;
			if (price == null || price.Id == 4662)
				filter.AutoOrder = (int)AutoOrderStatus.NotTuned;

			// при установке фильтра результат находится
			var data = filter.Find();
			var row = (AnalysisOfWorkFiled)data[0];
			Assert.IsTrue(row.UserCount.Contains(">4<"));
			Assert.AreEqual(row.AddressCount, "2");

			// отключенные пользователи и адреса не попадают в отчет
			user3.Enabled = false;
			session.Save(user3);
			address2.Enabled = false;
			session.Save(address2);

			Flush();

			data = filter.Find();
			row = (AnalysisOfWorkFiled)data[0];
			Assert.IsTrue(row.UserCount.Contains(">3<"));
			Assert.AreEqual(row.AddressCount, "1");

			// при изменении фильтра результат не находится
			filter.AutoOrder = (int)AutoOrderStatus.Used;
			data = filter.Find();
			Assert.AreEqual(data.Count, 0);
		}

		[Test]
		public void Exclude_users()
		{
			//исключаем из отчета пользователя
			user1.ExcludeFromManagerReports = true;

			session.Save(user1);
			Flush();

			var filter = new WhoWasNotUpdatedFilter { Session = session, BeginDate = DateTime.Now.AddDays(-3) };

			var data = filter.Find();
			Assert.AreEqual(data.Count, 3);
		}

		[Test]
		public void LastVersionUpdate()
		{
			user.Logs.AFTime = DateTime.Now.AddDays(-2);

			session.Save(user);
			Flush();

			var filter = new WhoWasNotUpdatedFilter { Session = session, BeginDate = DateTime.Now.AddDays(-1) };
			var data = filter.Find().Where(d => d.UserName == "user").ToList();

			Assert.AreEqual(data.First().LastUpdateDate, user.Logs.AFTime.Value.ToString("dd.MM.yyyy HH:mm"));

			user.Logs.AFTime = null;
			user.Logs.AFNetTime = DateTime.Now.AddDays(-2).AddMinutes(10);

			session.Save(user);
			Flush();

			data = filter.Find().Where(d => d.UserName == "user").ToList();
			Assert.AreEqual(data.First().LastUpdateDate, user.Logs.AFNetTime.Value.ToString("dd.MM.yyyy HH:mm"));

			user.Logs.AFTime = DateTime.Now.AddDays(-2);

			session.Save(user);
			Flush();

			data = filter.Find().Where(d => d.UserName == "user").ToList();

			Assert.AreEqual(data.First().LastUpdateDate, user.Logs.AFNetTime.Value.ToString("dd.MM.yyyy HH:mm"));
		}
	}
}