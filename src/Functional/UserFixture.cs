using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class UserFixture : FunctionalFixture
	{
		private Client client;
		private User user;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			session.SaveOrUpdate(client);
			user = client.Users[0];
		}

		[Test]
		public void SaveSettingsWithNullShowUserTest()
		{
			Open(user);
			Click("Настройка");
			Click("Добавить");
			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void ShowUsersTest()
		{
			var user1 = new User(client) {
				Name = "Пробный", Login = User.GetTempLogin()
			};
			client.AddUser(user1);
			Save(user1);
			Open(user);
			Click("Настройка");
			AssertText("Логины в видимости пользователя");
			Click("Добавить");
			Css(".search input.term").AppendText("Про");
			Click("Найти");
			var selectList = (SelectList)Css(".search select");
			var val = selectList.Options.First(o => o.Text.Contains("Про")).Value;
			selectList.SelectByValue(val);
			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");
			AssertText(val);
		}

		[Test]
		public void AvailibleAddressLogTest()
		{
			Open(user);
			var addressesCount = user.Client.Addresses.Count;
			for (int i = 0; i < addressesCount; i++) {
				browser.CheckBox(Find.ByName(string.Format("user.AvaliableAddresses[{0}].Id", i))).Checked = true;
			}
			Click("Сохранить");
			AssertText("$$$Изменено 'список адресов доставки пользователя'");
			for (int i = 0; i < addressesCount; i++) {
				AssertText(user.Client.Addresses[i].Name);
			}
			Open(user.Client);
			Click("Новый пользователь");
			for (int i = 0; i < addressesCount; i++) {
				browser.CheckBox(Find.ByName(string.Format("user.AvaliableAddresses[{0}].Id", i))).Checked = true;
			}
			Css("#user_Name").AppendText("TestUser");
			browser.CheckBox("sendClientCard").Checked = true;
			browser.TextField(Find.ByName("mails")).AppendText("kvasovtest@analit.net");
			Click("Создать");
			AssertText("подключены следующие адреса доставки:");

			var address = user.Client.Addresses.First();
			Click(address.Name);
			AssertText("Адрес доставки");
			var userCount = address.AvaliableForUsers.Count;
			for (int i = 0; i < userCount; i++) {
				browser.CheckBox(Find.ByName(string.Format("address.AvaliableForUsers[{0}].Id", i))).Checked = false;
			}
			Click("Сохранить");
			AssertText("отключен у всех пользователей");
		}

		[Test]
		public void Disabled_address_test_for_user()
		{
			var address = user.GetAvaliableAddresses().First();
			user.AvaliableAddresses.Add(address);
			address.Enabled = false;
			session.SaveOrUpdate(address);
			Flush();
			Open(user);
			browser.CheckBox(Find.ByName("user.AvaliableAddresses[0].Id")).Checked = false;
			Click("Сохранить");
			Assert.IsFalse(browser.CheckBox(Find.ByName("user.AvaliableAddresses[0].Id")).Enabled);
		}
	}
}
