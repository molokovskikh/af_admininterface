using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Test;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class BaseFiltersFixture : IntegrationFixture
	{
		[Test]
		public void Base_filter_test()
		{
			var filter = new SwitchOffClientsFilter { SortBy = "ClientName" };
			QueryCatcher.Catch();
			filter.Find(session);
		}

		[Test]
		public void Who_was_not_updated_filter_test()
		{
			var client = DataMother.TestClient();
			session.Save(client);

			var user = new User(client) { Login = "user", Name = "user" };
			var user1 = new User(client) { Login = "user1", Name = "user1" };
			var user2 = new User(client) { Login = "user2", Name = "user2" };
			var user3 = new User(client) { Login = "user3", Name = "user3" };
			client.AddUser(user);
			client.AddUser(user1);
			client.AddUser(user2);
			client.AddUser(user3);

			var address = new Address { Value = "123", Client = client };
			var address2 = new Address { Value = "123", Client = client };
			client.AddAddress(address);
			client.AddAddress(address2);

			address.AvaliableForUsers.Add(user);
			address.AvaliableForUsers.Add(user1);
			address.AvaliableForUsers.Add(user2);
			address.AvaliableForUsers.Add(user3);
			address2.AvaliableForUsers.Add(user3);

			session.Save(address);
			session.Save(address2);

			user.UserUpdateInfo = new UserUpdateInfo { UpdateDate = DateTime.Now.AddDays(-2), User = user, AFCopyId = User.GetTempLogin() };
			user1.UserUpdateInfo = new UserUpdateInfo { UpdateDate = DateTime.Now.AddDays(-2), User = user1, AFCopyId = User.GetTempLogin() };
			user2.UserUpdateInfo = new UserUpdateInfo { UpdateDate = DateTime.Now.AddDays(-2), User = user2, AFCopyId = User.GetTempLogin() };
			user3.UserUpdateInfo = new UserUpdateInfo { UpdateDate = DateTime.Now.AddDays(-2), User = user3, AFCopyId = User.GetTempLogin() };

			session.Save(user);
			session.Save(user1);
			session.Save(user2);
			session.Save(user3);
			session.Save(client);

			Flush();

			var filter = new WhoWasNotUpdatedFilter { Period = new DatePeriod(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)) };
			QueryCatcher.Catch();
			var data = filter.SqlQuery2(session);
		}
	}
}
