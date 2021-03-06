﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using Test.Support.Selenium;

namespace Functional
{
	public class UserSeleniumFixture : AdmSeleniumFixture
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

			var el = browser.FindElementByCssSelector("#UserSettingsForm input[value='55']");
			el.Click();

			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Clear();
			Assert.IsNotNull(session.Get<Client>(_client.Id).Settings.BuyingMatrix);
		}
	}
}
