using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;
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

		[Test]
		public void AvailibleAddressLogTest()
		{
			Open(user);
			var addressesCount = user.Client.Addresses.Count;
			Console.WriteLine(addressesCount);
			for (int i = 0; i < addressesCount; i++) {
				browser.CheckBox(Find.ByName(string.Format("user.AvaliableAddresses[{0}].Id", i))).Checked = true;
			}
			Click("Сохранить");
			AssertText("$$$Изменен список адресов доставки пользовалеля");
			for (int i = 0; i < addressesCount; i++) {
				AssertText(user.Client.Addresses[i].Name);
			}
			Open(user.Client);
			Click("Новый пользователь");
			for (int i = 0; i < addressesCount; i++) {
				browser.CheckBox(Find.ByName(string.Format("user.AvaliableAddresses[{0}].Id", i))).Checked = true;
			}
			browser.TextField("user_Name").AppendText("TestUser");
			browser.CheckBox("sendClientCard").Checked = true;
			browser.TextField(Find.ByName("mails")).AppendText("kvasovtest@analit.net");
			Click("Создать");
			AssertText("подключены слудующие адреса доставки:");

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
	}
}
