﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Common.Tools;
using log4net.Config;
using NUnit.Framework;
using Functional.ForTesting;
using Integration.ForTesting;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using WatiN.Core;
using WatiN.CssSelectorExtensions;
using DescriptionAttribute = NUnit.Framework.DescriptionAttribute;
using Document = WatiN.Core.Document;

namespace Functional
{
	public class BillingFixture : WatinFixture2
	{
		private Client client;
		private Payer payer;
		private Address address;

		public BillingFixture()
		{
			UseTestScope = true;
		}

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			payer.UpdateAndFlush();

			client.AddAddress("test address for billing");
			client.UpdateAndFlush();
			address = client.Addresses[0];
			Open(payer);
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));
		}

		[Test]
		public void View_all_addresses()
		{
			client.Addresses.Each(a => a.Enabled = false);
			Refresh();

			Assert.That(browser.Text, Is.StringContaining("Адреса доставки"));
			foreach (var address in client.Addresses)
			{
				var row = browser.TableRow("AddressRow" + address.Id);
				Assert.That(row.ClassName, Is.StringContaining("Disabled"));
				var checkBox = row.Css("input[name=status]");
				Assert.That(checkBox.Checked, Is.False);
			}
		}

		[Test]
		public void View_all_users()
		{
			var user = new User(client) {Name = "test user for billing",};
			user.Setup();
			user.Save();
			client.Users.Each(u => u.Enabled = false);
			Refresh();

			Assert.That(browser.Text, Is.StringContaining("Пользователи"));
			foreach (var item in client.Users)
			{
				var row = browser.TableRow("UserRow" + item.Id);
				Assert.That(row.ClassName, Is.StringContaining("Disabled"));
				var checkBox = row.Css("input[name=status]");
				Assert.That(checkBox.Checked, Is.False);
			}
		}

		[Test]
		public void View_additional_address_info()
		{
			var addressId = "usersForAddress" + address.Id;
			browser.Link(Find.ById(addressId)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Text, Is.StringContaining("Пользователи"));
			Assert.That(browser.Text, Is.StringContaining("Подключить пользователя"));
			// Щелкаем по адресу. Должна быть показана дополнительная информация
			var additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + address.Id));
			Assert.That(additionalInfoDiv.Exists, Is.True);
			Assert.That(additionalInfoDiv.Enabled, Is.True);
			// Щелкаем второй раз. Дополнительная информация должна быть скрыта
			browser.Link(Find.ById(addressId)).Click();
			additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + address.Id));
			Assert.That(additionalInfoDiv.Exists, Is.False);
		}

		[Test]
		public void View_additional_user_info()
		{
			var user = client.Users[0];
			var userId = "addressesForUser" + user.Id;
			browser.Link(Find.ById(userId)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Text, Is.StringContaining("Адреса"));
			Assert.That(browser.Text, Is.StringContaining("Подключить адрес"));
			// Щелкаем по адресу. Должна быть показана дополнительная информация
			var additionalInfoDiv = browser.Div(Find.ById("additionalUserInfo" + user.Id));
			Assert.That(additionalInfoDiv.Exists, Is.True);
			Assert.That(additionalInfoDiv.Enabled, Is.True);
			// Щелкаем второй раз. Дополнительная информация должна быть скрыта
			browser.Link(Find.ById(userId)).Click();
			additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + user.Id));
			Assert.That(additionalInfoDiv.Exists, Is.False);
		}

		[Test]
		public void Adding_users_to_address()
		{
			var user = client.Users[0];
			Assert.That(address.AvaliableForUsers.Count, Is.EqualTo(0));
			address.AvaliableForUsers = new List<User>();
			browser.Link(Find.ById("usersForAddress" + address.Id)).Click();
			Thread.Sleep(500);
			var connectUserLink = browser.Link(Find.ByText("Подключить пользователя"));
			connectUserLink.Click();
			Assert.That(connectUserLink.Style.Display, Is.EqualTo("none"));
			browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
			Thread.Sleep(500);
			var comboBox = browser.SelectList(Find.ById("UsersComboBox" + address.Id));
			Assert.That(comboBox.Options.Count, Is.GreaterThan(0));
			Assert.That(comboBox.HasSelectedItems, Is.True);
			Assert.That(comboBox.SelectedOption.Text.Contains(user.GetLoginOrName()));
			browser.Button(Find.ById("ConnectAddressToUserButton" + address.Id)).Click();
			Thread.Sleep(2000);
			Assert.That(connectUserLink.Style.Display, Is.EqualTo("block"));
			var connectingDiv = browser.Div(Find.ById("ConnectingUserDiv" + address.Id));
			Assert.That(connectingDiv.Style.Display, Is.EqualTo("none"));

			client.Refresh();
			scope.Evict(client);

			client = Client.Find(client.Id);
			Assert.That(address.AvaliableFor(client.Users[0]), Is.True);
		}

		[Test]
		public void Adding_addresses_to_users()
		{
			var user = client.Users[0];
			Assert.That(address.AvaliableForUsers.Count, Is.EqualTo(0));
			address.AvaliableForUsers = new List<User>();
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
			Thread.Sleep(500);
			var connectAddressLink = browser.Link(Find.ByText("Подключить адрес"));
			connectAddressLink.Click();
			Assert.That(connectAddressLink.Style.Display, Is.EqualTo("none"));
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			var comboBox = browser.SelectList(Find.ById("AddressesComboBox" + user.Id));
			Assert.That(comboBox.Options.Count, Is.GreaterThan(0));
			Assert.That(comboBox.HasSelectedItems, Is.True);
			Assert.That(comboBox.SelectedOption.Text.Contains(address.Value));

			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			Assert.That(connectAddressLink.Style.Display, Is.EqualTo("block"));
			var connectingDiv = browser.Div(Find.ById("ConnectingAddressDiv" + user.Id));
			Assert.That(connectingDiv.Style.Display, Is.EqualTo("none"));

			client.Refresh();
			Assert.That(address.AvaliableFor(client.Users[0]), Is.True);
		}

		[Test]
		public void DisconnectUserFromAddress()
		{
			var user = client.Users[0];
			browser.Link(Find.ById("usersForAddress" + address.Id)).Click();
			Thread.Sleep(500);
			var connectUserLink = browser.Link(Find.ByText("Подключить пользователя"));
			connectUserLink.Click();
			browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("ConnectAddressToUserButton" + address.Id)).Click();
			Thread.Sleep(2000);
			var row = browser.TableRow(Find.ById("RowAddress" + address.Id + "User" + user.Id));
			Assert.That(row.Exists, Is.True);
			browser.CheckBox(Find.ById(String.Format("CheckBoxAddress{0}User{1}", address.Id, user.Id))).Click();
			Thread.Sleep(500);
			var checkBox = browser.CheckBox(Find.ById(String.Format("CheckBoxAddress{0}User{1}", address.Id, user.Id)));
			Assert.That(checkBox.Exists, Is.False);

			client.Refresh();
			Assert.That(address.AvaliableFor(client.Users[0]), Is.False);
		}

		[Test]
		public void DisconnectAddressFromUser()
		{
			var user = client.Users[0];
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Link(Find.ByText("Подключить адрес")).Click();
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			var row = browser.TableRow(Find.ById("RowUser" + user.Id + "Address" + address.Id));
			Assert.That(row.Exists, Is.True);
			browser.CheckBox(Find.ById(String.Format("CheckBoxUser{0}Address{1}", user.Id, address.Id))).Click();
			Thread.Sleep(500);
			var checkBox = browser.CheckBox(Find.ById(String.Format("CheckBoxUser{0}Address{1}", user.Id, address.Id)));
			Assert.That(checkBox.Exists, Is.False);

			client.Refresh();
			Assert.That(address.AvaliableFor(client.Users[0]), Is.False);
		}

		[Test]
		public void Change_user_status()
		{
			var user = client.Users[0];

			var selector = String.Format("tr#UserRow{0}", user.Id);
			var row = (TableRow)browser.CssSelect(selector);
			var checkbox = (CheckBox)browser.CssSelect("input[name=status]");

			Assert.IsTrue(checkbox.Checked);
			Assert.That(row.ClassName, Is.Not.StringContaining("Disabled"));
			SimulateClick(browser, selector, checkbox);
			Assert.That(row.ClassName, Is.StringContaining("Disabled"));
		}

		[Test]
		public void Change_address_status()
		{
			var selector = String.Format("tr#AddressRow{0}", address.Id);
			var row = (TableRow)browser.Css(selector);
			var checkbox = (CheckBox)browser.Css(String.Format("tr#AddressRow{0} input[name=status]", address.Id));
				
			Assert.IsTrue(checkbox.Checked);
			Assert.That(row.ClassName, Is.Not.StringContaining("Disabled"));
			Assert.That(row.ClassName, Is.StringContaining("HasNoConnectedUsers"));

			SimulateClick(browser, selector, checkbox);

			Assert.That(row.ClassName, Is.StringContaining("Disabled"));
		}

		//я обрабатываю change но почему то click не вызывает change, по этому симулирую его
		private void SimulateClick(Document browser, string selector, CheckBox checkbox)
		{
			checkbox.Click();
			browser.Eval(String.Format("$('{0}').change()", String.Format("{0} input[name=status]", selector)));
		}

		[Test]
		public void Change_client_status()
		{
			var user = client.Users[0];
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));

			var userRow = browser.TableRow(Find.ById("UserRow" + user.Id));
			var userStatus = userRow.Css("input[name='status']");
			var addressRow = browser.TableRow(Find.ById("AddressRow" + address.Id));
			var addressStatus = addressRow.Css("input[name='status']");
			var clientRow = browser.TableRow(Find.ById("ClientRow" + client.Id));
			var clientStatus = clientRow.Css(String.Format("#ClientStatus{0}", client.Id));
			Assert.IsTrue(userStatus.Checked);
			Assert.IsTrue(addressStatus.Checked);
			Assert.IsTrue(clientStatus.Checked);
			Assert.That(userRow.ClassName, Is.Not.StringContaining("DisabledByBilling"));
			Assert.That(addressRow.ClassName, Is.Not.StringContaining("DisabledByBilling"));
			Assert.That(clientRow.ClassName, Is.Not.StringContaining("DisabledByBilling"));
			clientStatus.Click();
			Thread.Sleep(2000);
			Assert.IsTrue(userStatus.Checked);
			Assert.IsFalse(userStatus.Enabled);
			Assert.IsTrue(addressStatus.Checked);
			Assert.IsFalse(addressStatus.Enabled);
			Assert.IsFalse(clientStatus.Checked);
			Assert.That(userRow.ClassName, Is.Not.StringContaining("DisabledByBilling"));
			Assert.That(addressRow.ClassName, Is.Not.StringContaining("DisabledByBilling"));
			Assert.That(clientRow.ClassName, Is.StringContaining("DisabledByBilling"));
		}

		[Test]
		public void Test_view_connecting_user_to_address()
		{
			browser.Link(Find.ById("usersForAddress" + address.Id)).Click();
			browser.Link(Find.ByText("Подключить пользователя")).Click();
			var searchDiv = browser.Div(Find.ById("SearchUserDiv" + address.Id));
			var selectDiv = browser.Div(Find.ById("SelectUserDiv" + address.Id));
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("none"));
			// Жмем "Найти"
			browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
			Thread.Sleep(500);
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("none"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("block"));
			// Жмем "Сброс" и возвращаемся к полю ввода текста для поиска
			browser.Button(Find.ById("ResetUserSearchButton" + address.Id)).Click();
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("none"));
			// Жмем "Отмена". Должна появиться ссылка "Подключить пользователя", а поле ввода и кнопки - исчезнуть
			browser.Button(Find.ById("CancelUserSearchButton" + address.Id)).Click();
			Assert.IsTrue(browser.Link(Find.ByText("Подключить пользователя")).Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(browser.Div(Find.ById("ConnectingUserDiv" + address.Id)).Style.Display.ToLower().Equals("none"));
			// Жмем "Подключить пользователя" и вводим то, что не сможем найти. Должно быть сообщение что ничего не нашли
			browser.Link(Find.ByText("Подключить пользователя")).Click();
			browser.TextField(Find.ById("UserSearchText" + address.Id)).TypeText("1234567890");
			browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Div(Find.ById("SearchUserMessage" + address.Id)).Text, Is.StringContaining("Ничего не найдено"));
		}

		[Test]
		public void Test_view_connecting_address_to_user()
		{
			var user = client.Users[0];
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Link(Find.ByText("Подключить адрес")).Click();
			Thread.Sleep(500);
			var searchDiv = browser.Div(Find.ById("SearchAddressDiv" + user.Id));
			var selectDiv = browser.Div(Find.ById("SelectAddressDiv" + user.Id));
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("none"));
			// Жмем "Найти"
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("none"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("block"));
			// Жмем "Сброс" и возвращаемся к полю ввода текста для поиска
			browser.Button(Find.ById("ResetSearchButton" + user.Id)).Click();
			Assert.IsTrue(searchDiv.Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(selectDiv.Style.Display.ToLower().Equals("none"));
			// Жмем "Отмена". Должна появиться ссылка "Подключить адрес", а поле ввода и кнопки - исчезнуть
			browser.Button(Find.ById("CancelSearchButton" + user.Id)).Click();
			Assert.IsTrue(browser.Link(Find.ByText("Подключить адрес")).Style.Display.ToLower().Equals("block"));
			Assert.IsTrue(browser.Div(Find.ById("ConnectingAddressDiv" + user.Id)).Style.Display.ToLower().Equals("none"));
			// Жмем "Подключить адрес" и вводим то, что не сможем найти. Должно быть сообщение что ничего не нашли
			browser.Link(Find.ByText("Подключить адрес")).Click();
			browser.TextField(Find.ById("AddressSearchText" + user.Id)).TypeText("1234567890");
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Div(Find.ById("SearchAddressMessage" + user.Id)).Text, Is.StringContaining("Ничего не найдено"));
		}

		private void AddUsers(Client client, int countUsers)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				for (var i = 0; i < countUsers; i++)
				{
					var user = new User(client) { Name = "user", };
					user.Setup();
					var address = new Address { Client = client, Value = "address", };
					client.AddAddress(address);
					address.Save();
				}
				scope.VoteCommit();
			}
		}

		[Test(Description = "Тест сворачивания/разворачивания заголовков списков клиентов, пользователей и адресов")]
		public void Test_collapse_and_spread_headers()
		{
			AddUsers(client, 10);
			Refresh();

			// Список клиентов должен быть в свернутом виде
			var clientsHeaderDiv = browser.Div(Find.ById("ClientListHeader"));
			var clientsBodyDiv = browser.Div(Find.ById("ClientListBody"));
			Assert.IsTrue(clientsHeaderDiv.ClassName.Trim().ToLower().Equals("showhiden"));
			Assert.IsTrue(clientsBodyDiv.ClassName.Trim().ToLower().Equals("hidden"));
			// Щелкаем по заголовку. Список должен свернуться
			clientsHeaderDiv.Click();
			Assert.IsTrue(clientsHeaderDiv.ClassName.Trim().ToLower().Equals("hidevisible"));
			Assert.IsTrue(clientsBodyDiv.ClassName.Trim().ToLower().Equals("visiblefolder"));

			// Списки адресов и пользователей должны быть в свернутом виде
			var addressesHeaderDiv = browser.Div(Find.ById("AddressListHeader"));
			var addressesBodyDiv = browser.Div(Find.ById("AddressListBody"));
			Assert.IsTrue(addressesHeaderDiv.ClassName.Trim().ToLower().Equals("showhiden"));
			Assert.IsTrue(addressesBodyDiv.ClassName.Trim().ToLower().Equals("hidden"));
			// Открываем адреса
			addressesHeaderDiv.Click();
			Assert.IsTrue(addressesHeaderDiv.ClassName.Trim().ToLower().Equals("hidevisible"));
			Assert.IsTrue(addressesBodyDiv.ClassName.Trim().ToLower().Equals("visiblefolder"));

			var usersHeaderDiv = browser.Div(Find.ById("UserListHeader"));
			var usersBodyDiv = browser.Div(Find.ById("UserListBody"));
			Assert.IsTrue(usersHeaderDiv.ClassName.Trim().ToLower().Equals("showhiden"));
			Assert.IsTrue(usersBodyDiv.ClassName.Trim().ToLower().Equals("hidden"));
			// Открываем пользователей
			usersHeaderDiv.Click();
			Assert.IsTrue(usersHeaderDiv.ClassName.Trim().ToLower().Equals("hidevisible"));
			Assert.IsTrue(usersBodyDiv.ClassName.Trim().ToLower().Equals("visiblefolder"));
		}

		[Test]
		public void Test_refresh_total_sum()
		{
			// Создаем 2 пользователя и 3 адреса. 2 пользователя и 2 адреса включены
			var user = new User(client) { Name = "test user", };
			user.Accounting.ReadyForAcounting = true;
			user.Setup();
			client.AddAddress("address");
			client.AddAddress("address");
			client.Users[0].Enabled = true;
			address.Enabled = false;
			client.Save();
			//BasicConfigurator.Configure();
			foreach (var a in client.Addresses)
			{
				a.Accounting.ReadyForAcounting = true;
				a.AvaliableForUsers = new List<User> { client.Users[0], client.Users[1] };
				a.Save();
			}

			Refresh();
			var disabledAddress = client.Addresses.Where(a => !a.Enabled).First();
			var sum = GetTotalSum();

			// Включаем адрес. Сумма должна увеличиться
			Assert.That(Css(String.Format("#AddressRow{0} input[name=status]", disabledAddress.Id)).Checked, Is.False);
			Css(String.Format("#AddressRow{0} input[name=status]", disabledAddress.Id)).Click();
			Thread.Sleep(500);
			var currentSum = GetTotalSum();
			Assert.That(currentSum, Is.GreaterThan(sum));
			sum = currentSum;

			// Выключаем пользователя. Сумма должна уменьшиться
			Assert.That(Css(String.Format("#UserRow{0} input[name=status]", user.Id)).Checked, Is.True);
			Css(String.Format("#UserRow{0} input[name=status]", user.Id)).Click();
			Thread.Sleep(500);
			currentSum = GetTotalSum();
			Assert.That(currentSum, Is.LessThan(sum));
			sum = currentSum;

			// Выключаем клиента. Сумма должна стать равной нулю
			Css(String.Format("#ClientStatus{0}", client.Id)).Click();
			Thread.Sleep(500);
			currentSum = GetTotalSum();
			Assert.That(currentSum, Is.EqualTo(0));
		}

		private object GetTotalSum()
		{
			var value = (string)Css("#TotalSum").Text.ToString().Trim();
			return Convert.ToDecimal(value.Substring(0, value.Length - 2), CultureInfo.GetCultureInfo("ru-RU"));
		}

		[Test]
		public void Show_message_if_server_error_occured()
		{
			var user = client.Users[0];
			// Подключаем пользователю адрес
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();

			// Удаляем адрес и пользователя, чтобы произошла ошибка на сервере
			user.Delete();
			address.Delete();
			scope.Flush();

			var errorMessageDiv = browser.Div(Find.ById("ErrorMessageDiv"));
			Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("none"));

			// Пытаемся посмотреть адреса, доступные пользователю
			browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
			Thread.Sleep(500);

			Assert.IsTrue(errorMessageDiv.Exists);
			Assert.That(errorMessageDiv.Text, Is.EqualTo("При выполнении операции возникла ошибка. Попробуйте повторить позднее"));
			Assert.That(errorMessageDiv.Style.Display, Is.EqualTo("block"));
		}

		[Test]
		public void Send_message_to_user()
		{
			var username = client.Users[0].GetLoginOrName();

			browser.SelectList(Find.ByName("NewClientMessage.Id")).Select(username);
			var messageText = "test message for user " + username;
			browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(messageText);
			browser.Button(Find.ByValue("Отправить сообщение")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сообщение сохранено"));
			Assert.That(browser.Text, Is.StringContaining(String.Format("Остались не показанные сообщения для пользователя {0}", username)));
			browser.Link(Find.ByText("Просмотреть сообщение")).Click();
			Thread.Sleep(500);
			Assert.That(browser.Text, Is.StringContaining(messageText));
			var messages = Client.Find(client.Id).Users.Select(u => UserMessage.Find(u.Id)).ToList();
			messages[0].Refresh();
			Assert.That(messages[0].Message, Is.EqualTo(messageText));
			Assert.That(messages[0].ShowMessageCount, Is.EqualTo(1));
		}

		[Test]
		public void Cancel_message_for_user()
		{
			client.Refresh();
			var username = client.Users[0].GetLoginOrName();
			browser.SelectList(Find.ByName("NewClientMessage.Id")).Select(username);
			var messageText = "test message for user " + username;
			browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(messageText);
			browser.Button(Find.ByValue("Отправить сообщение")).Click();
			browser.Link(Find.ByText("Просмотреть сообщение")).Click();
			Thread.Sleep(500);
			browser.Button(String.Format("CancelViewMessage{0}", client.Users[0].Id)).Click();
			var messages = Client.Find(client.Id).Users.Select(u => UserMessage.Find(u.Id)).ToList();
			var message = messages[0];

			message.Refresh();
			Assert.That(message.Message, Is.EqualTo(messageText));
			Assert.That(message.ShowMessageCount, Is.EqualTo(0));
		}

		[Test]
		public void Send_message_to_all_users()
		{
			browser.SelectList(Find.ByName("NewClientMessage.Id")).Select("Для всех пользователей");
			var message = "test message for all users";
			browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(message);
			browser.Button(Find.ByValue("Отправить сообщение")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сообщение сохранено"));
			foreach (var user in client.Users)
			{
				Assert.That(browser.Text,
					Is.StringContaining(String.Format("Остались не показанные сообщения для пользователя {0}", user.GetLoginOrName())));

				var div = browser.Div(Find.ById("CurrentMessageForUser" + user.Id));
				Assert.IsTrue(div.Exists);
				browser.Link(Find.ById("ViewMessageForUser" + user.Id)).Click();
				Thread.Sleep(500);
				var messageBody = browser.Table(Find.ById("MessageForUser" + user.Id));
				Assert.IsTrue(messageBody.Exists);
				browser.Button(Find.ById("CancelViewMessage" + user.Id)).Click();
				Thread.Sleep(500);
				Assert.IsFalse(messageBody.Exists);
				Assert.That(browser.Text, Is.StringContaining("Сообщение удалено"));
			}
		}

		[Test, Description("Проверка фильтрации записей в статистике вкл./откл. по пользователю")]
		public void FilterLogMessagesByUser()
		{
			AddUsers(client, 3);
			client.Refresh();
			Refresh();

			// Проверяем, что логины пользователей - это ссылки
			foreach (var usr in client.Users)
				Assert.IsTrue(browser.Link(Find.ByText(usr.Login)).Exists, usr.Login);

			// Кликаем по логину одного из пользователей
			var user = client.Users[2];
			browser.Link(Find.ByText(user.Login)).Click();
			// В таблице статистики вкл./откл. должны остаться видимыми только строки, относящиеся к выбранному пользователю
			// остальные строки должны быть невидимы
			var logRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("logRow"));
			foreach (TableRow row in logRows)
			{
				if (row.Id.Equals("logRowUserLog" + user.Id))
					Assert.That(row.Style.Display, Is.Not.StringContaining("none"));
				else
					Assert.That(row.Style.Display, Is.StringContaining("none"));
			}
			//Логин-ссылка должна должна получить класс который сделает ее жирной
			Assert.That(browser.Link("UserLink" + user.Id).ClassName, Is.StringContaining("selected-filter"));
		}

		[Test, Description("Проверка, что по клику на логин, этот пользователь выбирается в выпадающем списке 'Сообщение для пользователя:'")]
		public void SelectUserForSendMessage()
		{
			AddUsers(client, 3);
			Refresh();
			// Кликаем по логину одного из пользователей
			var user = client.Users[2];
			browser.Link(Find.ByText(user.Login)).Click();
			// Этот пользователь должен стать выделенным в списке "Сообщение для пользователя"
			Assert.That(browser.SelectList(Find.ByName("NewClientMessage.Id")).SelectedOption.Text, Is.EqualTo(user.GetLoginOrName()));
		}

		[Test, Description("Проверка, что при выделении клиента, отображаются адреса и пользователи только для выбранного клиента")]
		public void ShowUsersOnlyForSelectedClient()
		{
			var client2 = DataMother.CreateTestClientWithAddressAndUser();

			client.Name += client.Id;
			client.UpdateAndFlush();
			client2.Name += client2.Id;
			client2.Payers.Add(client.Payers.First());
			client2.UpdateAndFlush();
			client.Refresh();
			client2.Refresh();
			var testUserId = client2.Users[0].Id;
			Refresh();

			var clientRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("ClientRow")).ToList();
			Assert.That(clientRows.Count, Is.EqualTo(2));
			Click("Клиенты");
			Assert.That(browser.Links.Where(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)).Count(), Is.EqualTo(1));
			// Кликаем на другого клиента
			browser.Links.Where(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)).First().Click();

			// В таблице, которая содержит всех пользователей плательщика должны быть видимыми только те строки,
			// которые соответствуют пользователям, принадлежащим выделенному клиенту
			var userRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("UserRow")).ToList();
			foreach (var row in userRows)
			{
				if (row.Id.Contains("UserRowHidden"))
					continue;
				if (row.Id.Equals("UserRow" + testUserId))
					Assert.That(row.Style.Display, Is.Null);
				else
					Assert.That(row.Style.Display, Is.EqualTo("none"));
			}
		}

		[Test]
		public void ShowAddressesOnlyForSelectedClient()
		{
			var client2 = DataMother.CreateTestClientWithAddressAndUser();
			client.Name += client.Id;
			client.UpdateAndFlush();
			client2.Name += client2.Id;
			client2.Payers.Add(client.Payers.First());
			client2.UpdateAndFlush();
			client.Refresh();
			client2.Refresh();
			var testAddressId = client2.Addresses[0].Id;
			Refresh();

			// Кликаем на другого клиента
			browser.Links.Where(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)).First().Click();

			// В таблице, которая содержит все адреса доставки плательщика должны быть видимыми только те строки,
			// которые соответствуют адресам, принадлежащим выделенному клиенту
			var addressRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("AddressRow")).ToList();
			foreach (var row in addressRows)
			{
				if (row.Id.Contains("AddressRowHidden"))
					continue;
				if (row.Id.Equals("AddressRow" + testAddressId))
					Assert.That(row.Style.Display, Is.Null);
				else
					Assert.That(row.Style.Display, Is.EqualTo("none"));
			}
		}

		[Test]
		public void Show_all_users_for_payer()
		{
			var client2 = DataMother.CreateTestClientWithAddressAndUser();
			client.Name += client.Id;
			client2.Name += client2.Id;
			client2.Payers.Add(client.Payers.First());
			client2.Users[0].Payer = payer;
			client.Save();
			client2.Save();

			Refresh();

			Click(client2.Name);
			var usersTable = browser.Table("users");
			var countVisibleRows = usersTable.TableRows.Count(row => (row != null) && (row.Id != null) 
				&& !row.Id.Contains("UserRowHidden") 
				&& (row.Style.Display != "none"));

			browser.Link(Find.ById("ShowOrHideUsers")).Click();
			Thread.Sleep(1000);

			var countVisibleRows2 = usersTable.TableRows.Count(row => (row != null) 
				&& (row.Id != null) 
				&& !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"));

			Assert.That(countVisibleRows, Is.LessThan(countVisibleRows2));
			browser.Link(Find.ById("ShowOrHideUsers")).Click();
			Thread.Sleep(1000);

			var countVisibleRows3 = usersTable.TableRows.Count(row => (row != null) 
				&& (row.Id != null) 
				&& !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"));
			Assert.That(countVisibleRows, Is.EqualTo(countVisibleRows3));
			browser.Link(Find.ById("ShowOrHideUsers")).Click();
			Thread.Sleep(1000);
		}

		[Test]
		public void Show_payer_with_no_addresses()
		{
			client = DataMother.CreateTestClientWithAddress();
			payer = client.Payers.First();

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining(String.Format("Плательщик {0}", payer.Name)));
		}

		[Test]
		public void Show_payer_with_no_users()
		{
			client = DataMother.CreateTestClientWithAddress();
			payer = client.Payers.First();

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining(String.Format("Плательщик {0}", payer)));
		}

		[Test]
		public void Change_recipient_for_payer()
		{
			var recipient = Recipient.Queryable.First();

			browser.Link(Find.ByText("Отправка кореспонденции")).Click();

			Click("Отправка кореспонденции");
			var select = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			Assert.That(select.SelectedItem, Is.Null);
			select.Select(recipient.Name);
			var mailTab = (Div)Css("#mail-tab");
			mailTab.Button(Find.ByValue("Сохранить")).Click();

			Assert.That(browser.SelectList(Find.ByName("Instance.Recipient.Id")).SelectedItem, Is.EqualTo(recipient.Name));
		}

		[Test, Description("Создание нового юридического лица")]
		public void Create_new_juridical_organization()
		{
			browser.Link(Find.ByText("Юридические лица")).Click();
			Assert.IsTrue(browser.Div(Find.ById("FormForAdding")).Exists);
			Assert.That(browser.Div(Find.ById("FormForAdding")).Style.Display, Is.EqualTo("none"));
			Assert.That(browser.Button(Find.ById("AddJuridicalOrganizationButton")).Style.Display, Is.Not.EqualTo("none"));
			browser.Button(Find.ById("AddJuridicalOrganizationButton")).Click();
			Thread.Sleep(500);
			Assert.That(browser.Div(Find.ById("FormForAdding")).Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Button(Find.ById("AddJuridicalOrganizationButton")).Style.Display, Is.EqualTo("none"));
			browser.TextField(Find.ByName("JuridicalOrganization.Name")).TypeText(String.Format("TestJuridicalNameForClient{0}", client.Id));
			browser.Button(Find.ById("SubmitFormButton")).Click();
			Assert.That(browser.Text, Is.StringContaining("Юридическое лицо создано"));

			var count = LegalEntity.Queryable.Count(u => u.Name == String.Format("TestJuridicalNameForClient{0}", client.Id));
			Assert.That(count, Is.EqualTo(1));
		}

		[Test]
		public void Update_juridical_organization_info()
		{
			var juridicalOrganization = new LegalEntity {
				Name = "Test" + client.Id,
				Payer = client.Payers.First(),
			};
			juridicalOrganization.Save();

			Refresh();

			browser.Link(Find.ByText("Юридические лица")).Click();
			browser.Link(Find.ById(String.Format("JuridicalOrganization{0}Link", juridicalOrganization.Id))).Click();

			var newName = juridicalOrganization.Name + "_" + client.Id;
			browser.TextField(Find.ByValue(juridicalOrganization.Name)).TypeText(newName);
			browser.Button(Find.ById(String.Format("SubmitChanges{0}", juridicalOrganization.Id))).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			juridicalOrganization.Refresh();
			Assert.That(juridicalOrganization.Name, Is.EqualTo(newName));
		}
	}
}
