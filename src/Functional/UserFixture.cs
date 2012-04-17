using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class UserFixture : WatinFixture2
	{
		private Client client;
		private User user;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			client.SaveAndFlush();
			user = client.Users[0];
		}

		[Test]
		public void ShowUsersTest()
		{
			new User(client) {
				Name = "Пробный",
				Login = User.GetTempLogin()
			}.Save();
			Open(user);
			Click("Настройка");
			Assert.That(browser.Text, Is.StringContaining("Логины в видимости пользователя"));
			Click("Добавить");
			browser.Css(".search input.term").AppendText("Про");
			Click("Найти");
			var selectList = (SelectList)browser.Css(".search select");
			var val = selectList.Options.First(o => o.Text.Contains("Про")).Value;
			selectList.SelectByValue(val);
			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");
			AssertText(val);
		}
	}
}
