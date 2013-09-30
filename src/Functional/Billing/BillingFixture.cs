using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using Functional.ForTesting;
using Integration.ForTesting;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;
using WatiN.CssSelectorExtensions;
using Document = WatiN.Core.Document;

namespace Functional.Billing
{
	public class BillingFixture : FunctionalFixture
	{
		private Client client;
		private Supplier _supplier;
		private Payer payer;
		private Address address;
		private User user;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			session.SaveOrUpdate(payer);

			client.AddAddress("test address for billing");
			user = client.Users[0];
			address = client.Addresses[0];
			session.SaveOrUpdate(client);

			_supplier = DataMother.CreateSupplier();
			_supplier.Payer = payer;
			session.Save(_supplier);

			Open(payer);
			browser.WaitUntilContainsText("Плательщик", 2);
			AssertText("Плательщик");
		}

		[Test]
		public void View_all_addresses()
		{
			client.Addresses.Each(a => a.Enabled = false);
			Refresh();

			AssertText("Адреса доставки");
			foreach (var address in client.Addresses) {
				var row = browser.TableRow("AddressRow" + address.Id);
				Assert.That(row.ClassName, Is.StringContaining("disabled"));
				var checkBox = row.Css("input[name=status]");
				Assert.That(checkBox.Checked, Is.False);
			}
		}

		[Test]
		public void View_all_users()
		{
			var user = client.AddUser("test user for billing");
			Save(user);

			client.Users.Each(u => u.Enabled = false);
			Refresh();

			AssertText("Пользователи");
			foreach (var item in client.Users) {
				var row = browser.TableRow("UserRow" + item.Id);
				Assert.That(row.ClassName, Is.StringContaining("disabled"));
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
			AssertText("Пользователи");
			AssertText("Подключить пользователя");
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
			var userId = "addressesForUser" + user.Id;
			browser.Link(Find.ById(userId)).Click();
			Thread.Sleep(500);
			AssertText("Адреса");
			AssertText("Подключить адрес");
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
		public void ViewAdditionalSupplierInfo()
		{
			var supplierId = "pricesRegions" + _supplier.Id;
			browser.Link(Find.ById(supplierId)).Click();
			Thread.Sleep(500);
			AssertText("Регионы размещения прайс-листов");

			var additionalInfoDiv = browser.Div(Find.ById("additionalSupplierInfo" + _supplier.Id));
			Assert.That(additionalInfoDiv.Exists, Is.True);
			Assert.That(additionalInfoDiv.Enabled, Is.True);

			browser.Link(Find.ById(supplierId)).Click();
			additionalInfoDiv = browser.Div(Find.ById("additionalSupplierInfo" + _supplier.Id));
			Assert.That(additionalInfoDiv.Exists, Is.False);
		}

		[Test]
		public void Adding_users_to_address()
		{
			Assert.That(address.AvaliableForUsers.Count, Is.EqualTo(0));
			address.AvaliableForUsers = new List<User>();
			Css("#usersForAddress" + address.Id).Click();
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

			session.Refresh(client);
			session.Evict(client);

			client = session.Load<Client>(client.Id);
			Assert.That(address.AvaliableFor(user), Is.True);
		}

		[Test]
		public void Adding_addresses_to_users()
		{
			Assert.That(address.AvaliableForUsers.Count, Is.EqualTo(0));

			Css("#addressesForUser" + user.Id).Click();
			Thread.Sleep(500);
			var connectAddressLink = browser.Link(Find.ByText("Подключить адрес"));
			connectAddressLink.Click();
			Assert.That(connectAddressLink.Style.Display, Is.EqualTo("none"));
			SearchAndSelectAddress(address.Value);

			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			Assert.That(connectAddressLink.Style.Display, Is.EqualTo("block"));
			var connectingDiv = browser.Div(Find.ById("ConnectingAddressDiv" + user.Id));
			Assert.That(connectingDiv.Style.Display, Is.EqualTo("none"));

			session.Refresh(client);
			Assert.That(address.AvaliableFor(user), Is.True);
		}

		[Test]
		public void DisconnectUserFromAddress()
		{
			Css("#usersForAddress" + address.Id).Click();
			WaitForText("Подключить пользователя");
			Click("Подключить пользователя");
			Css("#SearchUserButton" + address.Id).Click();
			var select = (SelectList)Css("#UsersComboBox" + address.Id);
			Wait(() => select.Options.Count > 0, "не удалось найти ни одного пользователя для подключения");
			Css("#ConnectAddressToUserButton" + address.Id).Click();

			WaitForCss("#RowAddress" + address.Id + "User" + user.Id);
			var row = browser.TableRow(Find.ById("RowAddress" + address.Id + "User" + user.Id));
			Assert.That(row.Exists, Is.True);
			browser.CheckBox(Find.ById(String.Format("CheckBoxAddress{0}User{1}", address.Id, user.Id))).Click();
			Thread.Sleep(1000);
			var checkBox = browser.CheckBox(Find.ById(String.Format("CheckBoxAddress{0}User{1}", address.Id, user.Id)));
			Assert.That(checkBox.Exists, Is.False);

			session.Refresh(client);
			Assert.That(address.AvaliableFor(user), Is.False);
		}

		[Test]
		public void DisconnectAddressFromUser()
		{
			Css("#addressesForUser" + user.Id).Click();
			WaitForText("Подключить адрес");
			ClickLink("Подключить адрес");
			SearchAndSelectAddress(address.Value);
			WaitForCss("#ConnectAddressToUserButton" + user.Id);
			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			var row = browser.TableRow(Find.ById("RowUser" + user.Id + "Address" + address.Id));
			Assert.That(row.Exists, Is.True);
			browser.CheckBox(Find.ById(String.Format("CheckBoxUser{0}Address{1}", user.Id, address.Id))).Click();
			Thread.Sleep(1000);
			var checkBox = browser.CheckBox(Find.ById(String.Format("CheckBoxUser{0}Address{1}", user.Id, address.Id)));
			Assert.That(checkBox.Exists, Is.False);

			session.Refresh(client);
			Assert.That(address.AvaliableFor(user), Is.False);
		}

		[Test]
		public void Change_user_status()
		{
			var selector = String.Format("tr#UserRow{0}", user.Id);
			var row = (TableRow)browser.CssSelect(selector);
			var checkbox = (CheckBox)row.CssSelect("input[name=status]");

			Assert.IsTrue(checkbox.Checked);
			Assert.That(row.ClassName, Is.Not.StringContaining("disabled"));
			SimulateClick(browser, selector, checkbox);
			Assert.That(row.ClassName, Is.StringContaining("disabled"));
		}

		[Test]
		public void Change_address_status()
		{
			var selector = String.Format("tr#AddressRow{0}", address.Id);
			var row = (TableRow)browser.Css(selector);
			var checkbox = (CheckBox)browser.Css(String.Format("tr#AddressRow{0} input[name=status]", address.Id));

			Assert.IsTrue(checkbox.Checked);
			Assert.That(row.ClassName, Is.Not.StringContaining("disabled"));
			Assert.That(row.ClassName, Is.StringContaining("has-no-connected-users"));

			SimulateClick(browser, selector, checkbox);

			Assert.That(row.ClassName, Is.StringContaining("disabled"));
		}

		//я обрабатываю change но почему то click не вызывает change, по этому симулирую его

		private void SimulateClick(Document browser, string selector, CheckBox checkbox)
		{
			checkbox.Click();
			browser.Eval(String.Format("$('{0}').change()", String.Format("{0} input[name=status]", selector)));
		}

		[Test]
		public void NotSubmitInCommentDialogTest()
		{
			var clientStatus = browser.Css(String.Format("#ClientStatus{0}", client.Id));
			Assert.IsTrue(clientStatus.Checked);
			clientStatus.Click();
			((TextField)Css(".ui-dialog-content #AddCommentField")).TypeText("TestComment");
			browser.Eval("$('input#AddCommentField').trigger($.Event( 'keydown', {which:$.ui.keyCode.ENTER, keyCode:$.ui.keyCode.ENTER}));");
			ConfirmDialog();
			Assert.IsFalse(clientStatus.Checked);
			Thread.Sleep(1000);
			browser.Refresh();
			AssertText("TestComment");
		}

		[Test]
		public void Change_client_status()
		{
			AssertText("Плательщик");

			var userRow = browser.TableRow(Find.ById("UserRow" + user.Id));
			var userStatus = userRow.Css("input[name='status']");
			var addressRow = browser.TableRow(Find.ById("AddressRow" + address.Id));
			var addressStatus = addressRow.Css("input[name='status']");
			var clientRow = browser.TableRow(Find.ById("ClientRow" + client.Id));
			var clientStatus = clientRow.Css(String.Format("#ClientStatus{0}", client.Id));
			Assert.IsTrue(userStatus.Checked);
			Assert.IsTrue(addressStatus.Checked);
			Assert.IsTrue(clientStatus.Checked);
			Assert.That(userRow.ClassName, Is.Not.StringContaining("disabled"));
			Assert.That(addressRow.ClassName, Is.Not.StringContaining("disabled"));
			Assert.That(clientRow.ClassName, Is.Not.StringContaining("disabled"));
			clientStatus.Click();
			AddCommentInDisableDialig();
			Wait(() => !userStatus.Enabled, "не дождались");
			Assert.IsTrue(userStatus.Checked);
			Assert.IsFalse(userStatus.Enabled);
			Assert.IsTrue(addressStatus.Checked);
			Assert.IsFalse(addressStatus.Enabled);
			Assert.IsFalse(clientStatus.Checked);
			Assert.That(userRow.ClassName, Is.StringContaining("disabled-by-parent"));
			Assert.That(addressRow.ClassName, Is.StringContaining("disabled-by-parent"));
			Assert.That(clientRow.ClassName, Is.StringContaining("disabled"));
		}

		[Test]
		public void Test_view_connecting_user_to_address()
		{
			Css("#usersForAddress" + address.Id).Click();
			ClickLink("Подключить пользователя");
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
			ClickLink("Подключить пользователя");
			browser.TextField(Find.ById("UserSearchText" + address.Id)).TypeText("1234567890");
			browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Div(Find.ById("SearchUserMessage" + address.Id)).Text, Is.StringContaining("Ничего не найдено"));
		}

		[Test]
		public void Test_view_connecting_address_to_user()
		{
			Css("#addressesForUser" + user.Id).Click();
			Thread.Sleep(500);
			ClickLink("Подключить адрес");
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
			ClickLink("Подключить адрес");
			browser.TextField(Find.ById("AddressSearchText" + user.Id)).TypeText("1234567890");
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			Assert.That(browser.Div(Find.ById("SearchAddressMessage" + user.Id)).Text, Is.StringContaining("Ничего не найдено"));
		}

		private void AddUsersAdnAddresses(Client client, int countUsers)
		{
			for (var i = 0; i < countUsers; i++) {
				client.AddUser("user");
				var address = client.AddAddress("address");
				session.Save(address);
			}
		}

		[Test(Description = "Тест сворачивания/разворачивания заголовков списков клиентов, пользователей и адресов")]
		public void Test_collapse_and_spread_headers()
		{
			AddUsersAdnAddresses(client, 10);
			Refresh();

			// Список клиентов должен быть в развернутом виде
			var collapsible = GetCollapsible("#clients");
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("hidevisible"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.Not.StringContaining("hidden"));
			collapsible.header.Click();
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("showhiden"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.StringContaining("hidden"));

			// Списки адресов и пользователей должны быть в свернутом виде
			collapsible = GetCollapsible("#addresses");
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("showhiden"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.StringContaining("hidden"));
			collapsible.header.Click();
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("hidevisible"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.Not.StringContaining("hidden"));

			// Списки адресов и пользователей должны быть в свернутом виде
			collapsible = GetCollapsible("#users");
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("showhiden"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.StringContaining("hidden"));
			collapsible.header.Click();
			Assert.That(collapsible.header.ClassName.Trim().ToLower(), Is.StringContaining("hidevisible"));
			Assert.That(collapsible.body.ClassName.Trim().ToLower(), Is.Not.StringContaining("hidden"));
		}

		[Test]
		public void Test_refresh_total_sum()
		{
			// Создаем 2 пользователя и 3 адреса. 2 пользователя и 2 адреса включены
			var user = client.AddUser("test user");
			user.Accounting.ReadyForAccounting = true;
			client.AddAddress("address");
			client.AddAddress("address");
			user.Enabled = true;
			address.Enabled = false;
			session.SaveOrUpdate(client);
			foreach (var a in client.Addresses) {
				a.Accounting.ReadyForAccounting = true;
				a.AvaliableForUsers = new List<User> { client.Users[0], client.Users[1] };
				session.Save(a);
			}

			Refresh();
			var disabledAddress = client.Addresses.Where(a => !a.Enabled).First();
			var sum = GetTotalSum();
			var currentSum = sum;

			// Включаем адрес. Сумма должна увеличиться
			Assert.That(Css(String.Format("#AddressRow{0} input[name=status]", disabledAddress.Id)).Checked, Is.False);
			Css(String.Format("#AddressRow{0} input[name=status]", disabledAddress.Id)).Click();
			Wait(() => {
				currentSum = GetTotalSum();
				return currentSum > sum;
			}, String.Format("Не дождался что бы сумма увеличилась {0}", browser.Url));

			Assert.That(currentSum, Is.GreaterThan(sum));
			sum = currentSum;

			// Выключаем пользователя. Сумма должна уменьшиться
			Assert.That(Css(String.Format("#UserRow{0} input[name=status]", user.Id)).Checked, Is.True);
			Css(String.Format("#UserRow{0} input[name=status]", user.Id)).Click();
			AddCommentInDisableDialig();
			Wait(() => {
				currentSum = GetTotalSum();
				return currentSum < sum;
			}, String.Format("Не дождался что бы сумма уменьшилась {0}", browser.Url));
			Assert.That(currentSum, Is.LessThan(sum));

			// Выключаем клиента. Сумма должна стать равной нулю
			Css(String.Format("#ClientStatus{0}", client.Id)).Click();
			AddCommentInDisableDialig();

			Wait(() => {
				currentSum = GetTotalSum();
				return currentSum == 0m;
			}, String.Format("Не дождался что бы сумма стала 0 {0}", browser.Url));
			Assert.That(currentSum, Is.EqualTo(0));
		}

		private decimal GetTotalSum()
		{
			var value = (string)Css("#TotalSum").Text.ToString().Trim();
			return Convert.ToDecimal(value.Substring(0, value.Length - 2), CultureInfo.GetCultureInfo("ru-RU"));
		}

		[Test]
		public void Show_message_if_server_error_occured()
		{
			// Подключаем пользователю адрес
			Css("#addressesForUser" + user.Id).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
			Thread.Sleep(500);
			browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
			Thread.Sleep(2000);
			Css("#addressesForUser" + user.Id).Click();

			// Удаляем адрес и пользователя, чтобы произошла ошибка на сервере
			client.Users.Remove(user);
			payer.Users.Remove(user);
			session.Delete(user);
			client.Addresses.Remove(address);
			payer.Addresses.Remove(address);
			session.Delete(address);
			Flush();

			var errorMessageDiv = browser.Div(Find.ById("ErrorMessageDiv"));
			Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("none"));

			// Пытаемся посмотреть адреса, доступные пользователю
			Css("#addressesForUser" + user.Id).Click();
			Thread.Sleep(500);

			Assert.IsTrue(errorMessageDiv.Exists);
			Assert.That(errorMessageDiv.Text, Is.EqualTo("При выполнении операции возникла ошибка. Попробуйте повторить позднее."));
			Assert.That(errorMessageDiv.Style.Display, Is.EqualTo("block"));
		}

		[Test]
		public void Send_message_to_user()
		{
			var username = user.GetLoginOrName();
			var messageText = "test message for user " + username;

			browser.SelectList(Find.ByName("userMessage.Id")).Select(username);
			browser.TextField(Find.ByName("userMessage.Message")).TypeText(messageText);
			Click("Отправить сообщение");

			AssertText("Сообщение сохранено");
			AssertText(String.Format("Остались не показанные сообщения для пользователя {0}", username));
			Click("Просмотреть сообщение");
			browser.WaitUntilContainsText("test message for user", 2);

			AssertText(messageText);
			var messages = session.Load<Client>(client.Id).Users.Select(u => session.Load<UserMessage>(u.Id)).ToList();
			session.Refresh(messages[0]);
			Assert.That(messages[0].Message, Is.EqualTo(messageText));
			Assert.That(messages[0].ShowMessageCount, Is.EqualTo(1));
		}

		[Test]
		public void Cancel_message_for_user()
		{
			session.Refresh(client);
			var username = user.GetLoginOrName();
			browser.SelectList(Find.ByName("userMessage.Id")).Select(username);
			var messageText = "test message for user " + username;
			browser.TextField(Find.ByName("userMessage.Message")).TypeText(messageText);
			ClickButton("Отправить сообщение");
			ClickLink("Просмотреть сообщение");
			Thread.Sleep(500);
			browser.Button(String.Format("CancelViewMessage{0}", user.Id)).Click();
			var messages = session.Load<Client>(client.Id).Users.Select(u => session.Load<UserMessage>(u.Id)).ToList();
			var message = messages[0];

			session.Refresh(message);
			Assert.That(message.Message, Is.EqualTo(messageText));
			Assert.That(message.ShowMessageCount, Is.EqualTo(0));
		}

		[Test]
		public void Send_message_to_all_users()
		{
			browser.SelectList(Find.ByName("userMessage.Id")).Select("Для всех пользователей");
			var message = "test message for all users";
			browser.TextField(Find.ByName("userMessage.Message")).TypeText(message);
			ClickButton("Отправить сообщение");
			AssertText("Сообщение сохранено");
			foreach (var user in client.Users) {
				Assert.That(browser.Text,
					Is.StringContaining(String.Format("Остались не показанные сообщения для пользователя {0}", user.GetLoginOrName())));

				var div = browser.Div(Find.ById("CurrentMessageForUser" + user.Id));
				Assert.IsTrue(div.Exists);
				Css("#ViewMessageForUser" + user.Id).Click();
				Thread.Sleep(500);
				var messageBody = browser.Table(Find.ById("MessageForUser" + user.Id));
				Assert.IsTrue(messageBody.Exists);
				browser.Button(Find.ById("CancelViewMessage" + user.Id)).Click();
				Thread.Sleep(500);
				Assert.IsFalse(messageBody.Exists);
				AssertText("Сообщение удалено");
			}
		}

		[Test, NUnit.Framework.Description("Проверка фильтрации записей в статистике вкл./откл. по пользователю")]
		public void FilterLogMessagesByUser()
		{
			AddUsersAdnAddresses(client, 3);
			Refresh();

			// Проверяем, что логины пользователей - это ссылки
			foreach (var usr in client.Users)
				Assert.IsTrue(browser.Link(Find.ByText(usr.Login)).Exists, usr.Login);

			// Кликаем по логину одного из пользователей
			var user = client.Users[2];
			ClickLink(user.Login);
			// В таблице статистики вкл./откл. должны остаться видимыми только строки, относящиеся к выбранному пользователю
			// остальные строки должны быть невидимы
			var logRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("logRow"));
			foreach (TableRow row in logRows) {
				if (row.Id.Equals("logRowUserLog" + user.Id))
					Assert.That(row.Style.Display, Is.Not.StringContaining("none"));
				else
					Assert.That(row.Style.Display, Is.StringContaining("none"));
			}
			//Логин-ссылка должна должна получить класс который сделает ее жирной
			Assert.That(browser.Link("UserLink" + user.Id).ClassName, Is.StringContaining("current-filter"));
		}

		[Test, NUnit.Framework.Description("Проверка, что по клику на логин, этот пользователь выбирается в выпадающем списке 'Сообщение для пользователя:'")]
		public void SelectUserForSendMessage()
		{
			AddUsersAdnAddresses(client, 3);
			Refresh();
			// Кликаем по логину одного из пользователей
			var user = client.Users[2];
			ClickLink(user.Login);
			// Этот пользователь должен стать выделенным в списке "Сообщение для пользователя"
			Assert.That(browser.SelectList(Find.ByName("userMessage.Id")).SelectedOption.Text, Is.EqualTo(user.GetLoginOrName()));
		}

		[Test, NUnit.Framework.Description("Проверка, что при выделении клиента, отображаются адреса и пользователи только для выбранного клиента")]
		public void ShowUsersOnlyForSelectedClient()
		{
			var client2 = DataMother.CreateTestClientWithAddressAndUser();
			client.Name += client.Id;
			client2.Name += client2.Id;
			client2.Payers.Add(client.Payers.First());
			var testUserId = client2.Users[0].Id;

			session.SaveOrUpdate(client);
			session.SaveOrUpdate(client2);
			Refresh();

			var clientRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("ClientRow")).ToList();
			Assert.That(clientRows.Count, Is.EqualTo(2));
			Click("Клиенты");
			Assert.That(browser.Links.Count(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)), Is.EqualTo(1));
			// Кликаем на другого клиента
			browser.Links.First(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)).Click();

			// В таблице, которая содержит всех пользователей плательщика должны быть видимыми только те строки,
			// которые соответствуют пользователям, принадлежащим выделенному клиенту
			var userRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("UserRow")).ToList();
			foreach (var row in userRows) {
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
			client2.Name += client2.Id;
			client2.Payers.Add(client.Payers.First());
			var testAddressId = client2.Addresses[0].Id;
			session.SaveOrUpdate(client);
			session.SaveOrUpdate(client2);
			Refresh();

			// Кликаем на другого клиента
			browser.Links.Where(link => (link != null) && (link.Text != null) && link.Text.Contains(client2.Name)).First().Click();

			// В таблице, которая содержит все адреса доставки плательщика должны быть видимыми только те строки,
			// которые соответствуют адресам, принадлежащим выделенному клиенту
			var addressRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("AddressRow")).ToList();
			foreach (var row in addressRows) {
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
			session.SaveOrUpdate(client);
			session.SaveOrUpdate(client2);

			Refresh();

			Click(client2.Name);
			var usersTable = browser.Table("users");
			var countVisibleRows = usersTable.TableRows.Count(row => (row != null) && (row.Id != null)
				&& !row.Id.Contains("UserRowHidden")
				&& (row.Style.Display != "none"));

			Css("#ShowOrHideUsers").Click();
			Thread.Sleep(1000);

			var countVisibleRows2 = usersTable.TableRows.Count(row => (row != null)
				&& (row.Id != null)
				&& !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"));

			Assert.That(countVisibleRows, Is.LessThan(countVisibleRows2));
			Css("#ShowOrHideUsers").Click();
			Thread.Sleep(1000);

			var countVisibleRows3 = usersTable.TableRows.Count(row => (row != null)
				&& (row.Id != null)
				&& !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"));
			Assert.That(countVisibleRows, Is.EqualTo(countVisibleRows3));
			Css("#ShowOrHideUsers").Click();
			Thread.Sleep(1000);
		}

		[Test]
		public void Show_payer_with_no_addresses()
		{
			client = DataMother.CreateTestClientWithAddress();
			payer = client.Payers.First();

			Open(payer);
			AssertText(String.Format("Плательщик {0}", payer.Name));
		}

		[Test]
		public void Show_payer_with_no_users()
		{
			client = DataMother.CreateTestClientWithAddress();
			payer = client.Payers.First();

			Open(payer);
			AssertText(String.Format("Плательщик {0}", payer));
		}

		[Test]
		public void Change_recipient_for_payer()
		{
			var recipient = session.Query<Recipient>().First();

			ClickLink("Отправка кореспонденции");

			Click("Отправка кореспонденции");
			var select = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			Assert.That(select.SelectedItem, Is.Null);
			select.Select(recipient.Name);
			var mailTab = (Div)Css("#mail-tab");
			mailTab.Button(Find.ByValue("Сохранить")).Click();

			Assert.That(browser.SelectList(Find.ByName("Instance.Recipient.Id")).SelectedItem, Is.EqualTo(recipient.Name));
		}

		[Test]
		public void Create_balance_operation()
		{
			Click(String.Format("Платежи/Счета {0}", DateTime.Now.Year));
			browser.WaitUntilContainsText("Списание", 1000);
			Css("#operation_Description").TypeText("тестовый возврат");
			Css("#operation_Sum").TypeText("500");
			Css("#operation_Date").TypeText(DateTime.Now.ToShortDateString());
			Click(String.Format("#new-operation-{0}", DateTime.Now.Year), "Добавить");
			AssertText("Сохранено");
		}

		[Test]
		public void If_free_flag_turnoff_on_user_than_free_flag_should_by_thurnoff_on_all_related_addresses()
		{
			user.Accounting.Payment = 100;
			user.Accounting.IsFree = true;
			address.Accounting.IsFree = true;
			address.Accounting.Payment = 100;
			user.AvaliableAddresses.Add(address);
			session.SaveOrUpdate(client);
			Refresh();

			Css(String.Format("#UserRow{0} input[name=free]", user.Id)).Click();
			browser.WaitUntilContainsText("Следующие адреса доставки стали платными", 1);
			AssertText(String.Format("Следующие адреса доставки стали платными: {0}", address.Value));

			Assert.That(Css(String.Format("#UserRow{0} input[name=free]", user.Id)).Checked, Is.False);
			Assert.That(Css(String.Format("#UserRow{0}", user.Id)).ClassName, Is.Not.StringContaining("consolidate-free"));

			Assert.That(Css(String.Format("#AddressRow{0} input[name=free]", address.Id)).Checked, Is.False);
			Assert.That(Css(String.Format("#AddressRow{0}", address.Id)).ClassName, Is.Not.StringContaining("consolidate-free"));

			session.Refresh(user.Accounting);
			session.Refresh(address.Accounting);
			Assert.That(user.Accounting.IsFree, Is.False);
			Assert.That(address.Accounting.IsFree, Is.False);
		}

		[Test]
		public void Show_payer_message_history()
		{
			Css("textarea[name='userMessage.Message']").TypeText("Тестовое сообщение");
			Click("Отправить сообщение");
			AssertText("Сообщение сохранено");
			Click("История сообщений");
			AssertText("История сообщения");
			AssertText("Тестовое сообщение");
		}

		[Test]
		public void Accounted_not_selected_if_unchecked_status()
		{
			user.Accounting.BeAccounted = true;
			session.SaveOrUpdate(client);
			Refresh();

			Assert.That(Css(String.Format("#UserRow{0} input[name=accounted]", user.Id)).Checked, Is.True);

			Css(String.Format("#UserRow{0} input[name=status]", user.Id)).Checked = false;
			Css("#AddCommentField").AppendText("testComment");
			ConfirmDialog();
			Thread.Sleep(3000);

			Refresh();

			Assert.That(Css(String.Format("#UserRow{0} input[name=accounted]", user.Id)).Checked, Is.False);
		}

		private void SearchAndSelectAddress(string value)
		{
			Css("#SearchAddressButton" + user.Id).Click();
			var id = "AddressesComboBox" + user.Id;
			var comboBox = browser.SelectList(Find.ById(id));
			Wait(() => comboBox.Options.Count > 0, "не удалось дождаться списка адресов");

			Assert.That(comboBox.Options.Count, Is.GreaterThan(0));
			Assert.That(comboBox.HasSelectedItems, Is.True);
			comboBox.Select(value + " ");
		}

		public class Collapsible
		{
			public Collapsible(Element header, Element body)
			{
				this.header = header;
				this.body = body;
			}

			public Element header;
			public Element body;
		}

		private Collapsible GetCollapsible(string selector)
		{
			var collapsible = ((Table)Css(selector)).Parents().First(p => p.ClassName != null && p.ClassName.ToLower().Contains("collapsible"));
			var header = collapsible.CssSelect(".trigger");
			var body = collapsible.CssSelect(".VisibleFolder");
			return new Collapsible(header, body);
		}

		private void AddCommentInDisableDialig()
		{
			Css(".ui-dialog-content #AddCommentField").AppendText("TestComment");
			ConfirmDialog();
		}
	}
}