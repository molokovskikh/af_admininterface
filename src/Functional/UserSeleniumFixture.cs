using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using Test.Support.Selenium;

namespace Functional
{
	public class UserSeleniumFixture : SeleniumFixture
	{
		private Client _client;
		private User _user;

		[SetUp]
		public void Setup()
		{
			_client = DataMother.CreateTestClientWithUser();
			_user = _client.Users.First();
			session.Save(_client);
			session.Save(_user);
		}

		[Test]
		public void AssortimentTest()
		{
			Assert.IsNull(_user.Client.Settings.BuyingMatrix);

			Open(_user, "Edit");
			ClickLink("Настройка");

			browser.FindElementByName("user.AssignedPermissions[3].Id").Click();

			ClickButton("Сохранить");
			AssertText("Сохранено");

			Close();

			Assert.IsNotNull(session.Get<Client>(_client.Id).Settings.BuyingMatrix);
		}
	}
}
