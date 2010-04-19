﻿using System.Collections.Generic;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;

using WatiN.Core;
using Functional.ForTesting;
using AdminInterface.Models;
using System;
using System.Threading;
using Common.MySql;
using Common.Web.Ui.Helpers;
using Castle.ActiveRecord;
using System.Linq;

namespace Functional
{
	public class BillingFixture : WatinFixture
	{
		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			using (var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				Assert.That(browser.Text, Text.Contains("Фильтр плательщиков"));
				browser.RadioButton(Find.ById("SearchByBillingId")).Click();
				browser.TextField(Find.ById("SearchText")).TypeText("921");
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.Text, Text.Contains("Офис123"));
				browser.Link(Find.ByText("921")).Click();
				Assert.That(browser.Text, Text.Contains("Плательщик Офис123"));
			}
		}

		[Test]
		public void View_billing_page_for_client()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.BillingInstance.ShortName += client.BillingInstance.PayerID;
			client.BillingInstance.UpdateAndFlush();
			using (var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				browser.RadioButton(Find.ById("SearchByBillingId")).Click();
				browser.TextField(Find.ById("SearchText")).TypeText(client.BillingInstance.PayerID.ToString());
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.Text, Text.Contains(client.BillingInstance.ShortName));
				browser.Link(Find.ByText(client.BillingInstance.PayerID.ToString())).Click();
				Assert.That(browser.Text, Text.Contains("Плательщик " + client.BillingInstance.ShortName));				
			}
		}

		[Test]
		public void View_all_addresses()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.BillingInstance.ShortName += client.BillingInstance.PayerID;
			client.BillingInstance.UpdateAndFlush();
			var addr = new Address { Client = client, Value = "test address for billing",};
			addr.SaveAndFlush();
			client.Addresses.Add(addr);
			client.UpdateAndFlush();
			foreach (var address in client.Addresses)
			{
				address.Enabled = false;
				address.UpdateAndFlush();
			}

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				Assert.That(browser.Text, Text.Contains(String.Format("Адреса плательщика \"{0}\"", client.BillingInstance.ShortName)));
				foreach (var address in client.Addresses)
				{
					var row = browser.TableRow("AddressRow" + address.Id);
					Assert.That(row.ClassName, Text.Contains("Disabled"));
					var checkBox = browser.CheckBox("AddressStatus" + address.Id);
					Assert.That(checkBox.Checked, Is.False);
				}
			}
		}

		[Test]
		public void View_all_users()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.BillingInstance.ShortName += client.BillingInstance.PayerID;
			client.BillingInstance.UpdateAndFlush();
			var user = new User {Client = client, Name = "test user for billing",};
			user.Setup(client);
			user.SaveAndFlush();
			client.Users.Add(user);
			client.UpdateAndFlush();
			foreach (var item in client.Users)
			{
				item.Enabled = false;
				item.UpdateAndFlush();
			}
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				Assert.That(browser.Text, Text.Contains(String.Format("Пользователи плательщика \"{0}\"", client.BillingInstance.ShortName)));
				foreach (var item in client.Users)
				{
					var row = browser.TableRow("UserRow" + item.Id);
					Assert.That(row.ClassName, Text.Contains("Disabled"));
					var checkBox = browser.CheckBox("UserStatusCheckBox" + item.Id);
					Assert.That(checkBox.Checked, Is.False);
				}
			}
		}

		[Test]
		public void View_additional_address_info()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var address = client.Addresses[0];
				browser.Link(Find.ByText(address.Value)).Click();
				Thread.Sleep(500);
				Assert.That(browser.Text, Text.Contains("Пользователи"));
				Assert.That(browser.Text, Text.Contains("Подключить пользователя"));
				// Щелкаем по адресу. Должна быть показана дополнительная информация
				var additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + address.Id));
				Assert.That(additionalInfoDiv.Exists, Is.True);
				Assert.That(additionalInfoDiv.Enabled, Is.True);
				// Щелкаем второй раз. Дополнительная информация должна быть скрыта
				browser.Link(Find.ByText(address.Value)).Click();
				additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + address.Id));
				Assert.That(additionalInfoDiv.Exists, Is.False);
			}
		}

		[Test]
		public void View_additional_user_info()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var user = client.Users[0];
				browser.Link(Find.ByText(user.Login)).Click();
				Thread.Sleep(500);
				Assert.That(browser.Text, Text.Contains("Адреса"));
				Assert.That(browser.Text, Text.Contains("Подключить адрес"));
				// Щелкаем по адресу. Должна быть показана дополнительная информация
				var additionalInfoDiv = browser.Div(Find.ById("additionalUserInfo" + user.Id));
				Assert.That(additionalInfoDiv.Exists, Is.True);
				Assert.That(additionalInfoDiv.Enabled, Is.True);
				// Щелкаем второй раз. Дополнительная информация должна быть скрыта
				browser.Link(Find.ByText(user.Login)).Click();
				additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + user.Id));
				Assert.That(additionalInfoDiv.Exists, Is.False);
			}			
		}

		[Test]
		public void Adding_users_to_address()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var address = client.Addresses[0];
				var user = client.Users[0];
				Assert.That(client.Addresses[0].AvaliableForUsers, Is.Null);
				address.AvaliableForUsers = new List<User>();
				browser.Link(Find.ByText(address.Value)).Click();
				Thread.Sleep(500);
				var connectUserLink = browser.Link(Find.ByText("Подключить пользователя"));
				connectUserLink.Click();
				Assert.That(connectUserLink.Style.ToString().ToLower().Contains("display: none"), Is.True);
				browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
				Thread.Sleep(500);
				var comboBox = browser.SelectList(Find.ById("UsersComboBox" + address.Id));
				Assert.That(comboBox.Options.Length, Is.GreaterThan(0));
				Assert.That(comboBox.HasSelectedItems, Is.True);
				Assert.That(comboBox.SelectedOption.Text.Contains(user.GetLoginOrName()));
				browser.Button(Find.ById("ConnectAddressToUserButton" + address.Id)).Click();
				Thread.Sleep(2000);
				Assert.That(connectUserLink.Style.ToString().ToLower().Contains("display: block"), Is.True);
				var connectingDiv = browser.Div(Find.ById("ConnectingUserDiv" + address.Id));
				Assert.That(connectingDiv.Style.ToString().ToLower().Contains("display: none"), Is.True);
				
				using (var scope = new SessionScope())
				{
					var newClient = Client.Find(client.Id);
					Assert.That(newClient.Addresses[0].AvaliableFor(newClient.Users[0]), Is.True);
				}
			}
		}

		[Test]
		public void Adding_addresses_to_users()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var address = client.Addresses[0];
				var user = client.Users[0];
				Assert.That(client.Addresses[0].AvaliableForUsers, Is.Null);
				address.AvaliableForUsers = new List<User>();
				browser.Link(Find.ByText(user.Login)).Click();
				Thread.Sleep(500);
				var connectAddressLink = browser.Link(Find.ByText("Подключить адрес"));
				connectAddressLink.Click();
				Assert.That(connectAddressLink.Style.ToString().ToLower().Contains("display: none"), Is.True);
				browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
				Thread.Sleep(500);
				var comboBox = browser.SelectList(Find.ById("AddressesComboBox" + user.Id));
				Assert.That(comboBox.Options.Length, Is.GreaterThan(0));
				Assert.That(comboBox.HasSelectedItems, Is.True);
				Assert.That(comboBox.SelectedOption.Text.Contains(address.Value));

				browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
				Thread.Sleep(2000);
				Assert.That(connectAddressLink.Style.ToString().ToLower().Contains("display: block"), Is.True);
				var connectingDiv = browser.Div(Find.ById("ConnectingAddressDiv" + user.Id));
				Assert.That(connectingDiv.Style.ToString().ToLower().Contains("display: none"), Is.True);

				using (var scope = new SessionScope())
				{
					var newClient = Client.Find(client.Id);
					Assert.That(newClient.Addresses[0].AvaliableFor(newClient.Users[0]), Is.True);
				}
			}
		}

		[Test]
		public void DisconnectUserFromAddress()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var address = client.Addresses[0];
				var user = client.Users[0];
				browser.Link(Find.ByText(address.Value)).Click();
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
				using (var scope = new SessionScope())
				{
					var newClient = Client.Find(client.Id);
					Assert.That(newClient.Addresses[0].AvaliableFor(newClient.Users[0]), Is.False);
				}
			}
		}

		[Test]
		public void DisconnectAddressFromUser()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))			
			{
				var address = client.Addresses[0];
				var user = client.Users[0];
				browser.Link(Find.ByText(user.Login)).Click();
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
				using (var scope = new SessionScope())
				{
					var newClient = Client.Find(client.Id);
					Assert.That(newClient.Addresses[0].AvaliableFor(newClient.Users[0]), Is.False);
				}
			}			
		}

		[Test]
		public void Change_user_status()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			user.Enabled = true;
			user.UpdateAndFlush();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var checkBoxStatus = browser.CheckBox(Find.ById("UserStatusCheckBox" + user.Id));
				var row = browser.TableRow(Find.ById("UserRow" + user.Id));
				Assert.IsTrue(checkBoxStatus.Checked);
				Assert.That(row.ClassName, Text.DoesNotContain("Disabled"));
				checkBoxStatus.Click();
				Thread.Sleep(500);
				Assert.That(row.ClassName, Text.Contains("Disabled"));
			}
		}

		[Test]
		public void Change_address_status()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var address = client.Addresses[0];
			address.Enabled = true;
			address.UpdateAndFlush();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{				
				var checkBoxStatus = browser.CheckBox(Find.ById("AddressStatus" + address.Id));
				var row = browser.TableRow(Find.ById("AddressRow" + address.Id));
				Assert.IsTrue(checkBoxStatus.Checked);
				Assert.That(row.ClassName, Text.DoesNotContain("Disabled"));				
				Assert.That(row.ClassName, Text.Contains("HasNoConnectedUsers"));
				checkBoxStatus.Click();
				Thread.Sleep(500);
				Assert.That(row.ClassName, Text.Contains("Disabled"));
			}
		}

		[Test]
		public void Change_client_status()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Status = ClientStatus.On;
			client.UpdateAndFlush();
			var address = client.Addresses[0];
			var user = client.Users[0];
			address.Enabled = true;
			address.UpdateAndFlush();
			user.Enabled = true;
			user.UpdateAndFlush();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var userStatus = browser.CheckBox(Find.ById("UserStatusCheckBox" + user.Id));
				var addressStatus = browser.CheckBox(Find.ById("AddressStatus" + address.Id));
				var clientStatus = browser.CheckBox(Find.ById("ClientStatus" + client.Id));
				var userRow = browser.TableRow(Find.ById("UserRow" + user.Id));
				var addressRow = browser.TableRow(Find.ById("AddressRow" + address.Id));
				var clientRow = browser.TableRow(Find.ById("ClientRow" + client.Id));
				Assert.IsTrue(userStatus.Checked);
				Assert.IsTrue(addressStatus.Checked);
				Assert.IsTrue(clientStatus.Checked);
				Assert.That(userRow.ClassName, Text.DoesNotContain("Disabled"));
				Assert.That(addressRow.ClassName, Text.DoesNotContain("Disabled"));
				Assert.That(clientRow.ClassName, Text.DoesNotContain("Disabled"));
				clientStatus.Click();
				Thread.Sleep(2000);
				Assert.IsFalse(userStatus.Checked);
				Assert.IsFalse(addressStatus.Checked);
				Assert.IsFalse(clientStatus.Checked);
				Assert.That(userRow.ClassName, Text.Contains("Disabled"));
				Assert.That(addressRow.ClassName, Text.Contains("Disabled"));
				Assert.That(clientRow.ClassName, Text.Contains("Disabled"));
			}
		}

		[Test]
		public void Test_view_connecting_user_to_address()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var address = client.Addresses[0];
				browser.Link(Find.ByText(address.Value)).Click();
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
				Assert.That(browser.Div(Find.ById("SearchUserMessage" + address.Id)).Text, Text.Contains("Ничего не найдено"));
			}
		}

		[Test]
		public void Test_view_connecting_address_to_user()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{				
				var user = client.Users[0];
				browser.Link(Find.ByText(user.Login)).Click();
				browser.Link(Find.ByText("Подключить адрес")).Click();
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
				Assert.That(browser.Div(Find.ById("SearchAddressMessage" + user.Id)).Text, Text.Contains("Ничего не найдено"));
			}			
		}

		[Test]
		public void If_clients_count_more_than_5_they_must_be_collapsed()
		{
		}

		[Test]
		public void If_addresses_count_more_than_5_they_must_be_collapsed()
		{

		}

		[Test(Description = "Тест сворачивания/разворачивания заголовков списков клиентов, пользователей и адресов")]
		public void Test_collapse_and_spread_headers()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var count = 10;
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				for (var i = 0; i < count; i++)
				{
					var user = new User { Client = client, Name = "user", };
					user.Setup(client);
					var address = new Address { Client = client, Value = "address", };
					address.Save();
				}
				scope.VoteCommit();
			}
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
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
		}

		[Test]
		public void Test_refresh_total_sum()
		{
			// Создаем 2 пользователя и 3 адреса. 2 пользователя и 2 адреса включены
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var user = new User { Client = client, Name = "test user", Enabled = true, };
				user.Setup(client);
				var address = new Address { Client = client, Value = "address", Enabled = true, };
				address.Save();
				address = new Address { Client = client,  Value = "address", Enabled = true, };
				address.Save();
				client.Users[0].Enabled = true;
				client.Users[0].Save();
				client.Addresses[0].Enabled = false;
				client.Addresses[0].Save();
				scope.VoteCommit();
				client = Client.Find(client.Id);
				foreach (var addr in client.Addresses)
				{
					addr.AvaliableForUsers = new List<User> { client.Users[0], client.Users[1] };
					addr.Save();
				}
			}
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var user = client.Users[0];
				var address = client.Addresses.Where(addr => !addr.Enabled).First();
				var totalSumDiv = browser.Div(Find.ById("TotalSum"));
				var sum = Convert.ToInt64(totalSumDiv.Text);
				
				// Включаем адрес. Сумма должна увеличиться
				browser.CheckBox(Find.ById("AddressStatus" + address.Id)).Click();
				Thread.Sleep(500);
				Assert.That(Convert.ToInt64(totalSumDiv.Text), Is.GreaterThan(sum));
				sum = Convert.ToInt64(totalSumDiv.Text);

				// Выключаем пользователя. Сумма должна уменьшиться
				browser.CheckBox(Find.ById("UserStatusCheckBox" + user.Id)).Click();
				Thread.Sleep(500);
				Assert.That(Convert.ToInt64(totalSumDiv.Text), Is.LessThan(sum));

				// Выключаем клиента. Сумма должна стать равной нулю
				browser.CheckBox(Find.ById("ClientStatus" + client.Id)).Click();
				Thread.Sleep(500);
				Assert.That(Convert.ToInt64(totalSumDiv.Text), Is.EqualTo(0));
			}
		}

		[Test]
		public void Show_message_if_server_error_occured()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				// Подключаем пользователю адрес
				browser.Link(Find.ByText(user.Login)).Click();
				Thread.Sleep(500);
				browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
				Thread.Sleep(500);
				browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
				Thread.Sleep(2000);
				browser.Link(Find.ByText(user.Login)).Click();

				// Удаляем адрес и пользователя, чтобы произошла ошибка на сервере
				client.Addresses[0].DeleteAndFlush();
				client.Users[0].DeleteAndFlush();

				var errorMessageDiv = browser.Div(Find.ById("ErrorMessageDiv"));
				Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("none"));

				// Пытаемся посмотреть адреса, доступные пользователю
				browser.Link(Find.ByText(user.Login)).Click();
				Thread.Sleep(500);

				Assert.IsTrue(errorMessageDiv.Exists);
				Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("block"));
				Assert.IsTrue(errorMessageDiv.Text.Equals("При выполнении операции возникла ошибка. Попробуйте повторить позднее"));
			}
		}

		[Test]
		public void Send_message_to_user()
		{
			using (new SessionScope())
			{
				var client = DataMother.CreateTestClientWithAddressAndUser();
				using (var browser = Open(string.Format("Client/{0}", client.Id)))
				{
					var username = client.Users[0].GetLoginOrName();
					browser.Link(Find.ByText("Биллинг")).Click();
					browser.SelectList(Find.ByName("NewClientMessage.ClientCode")).Select(username);
					var message = "test message for user " + username;
					browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(message);
					browser.Button(Find.ByValue("Отправить сообщение")).Click();
					Assert.That(browser.Text, Text.Contains("Сообщение сохранено"));
					Assert.That(browser.Text, Text.Contains(String.Format("Остались не показанные сообщения для пользователя {0}", username)));
					browser.Link(Find.ByText("Просмотреть сообщение")).Click();
					Thread.Sleep(500);
					Assert.That(browser.Text, Text.Contains(message));
				}
			}			
		}

		[Test]
		public void Send_message_to_all_users()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var usr = new User {Client = client, Name = "test user", Enabled = true,};
				usr.Setup(client);
				scope.VoteCommit();
			}
			using (new SessionScope())
			{
				using (var browser = Open(string.Format("Client/{0}", client.Id)))
				{
					client = Client.Find(client.Id);
					browser.Link(Find.ByText("Биллинг")).Click();
					browser.SelectList(Find.ByName("NewClientMessage.ClientCode")).Select("Для всех пользователей");
					var message = "test message for all users";
					browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(message);
					browser.Button(Find.ByValue("Отправить сообщение")).Click();
					Assert.That(browser.Text, Text.Contains("Сообщение сохранено"));
					foreach (var user in client.Users)
					{
						Assert.That(browser.Text, Text.Contains(String.Format("Остались не показанные сообщения для пользователя {0}",
						                                                      user.GetLoginOrName())));
						var div = browser.Div(Find.ById("CurrentMessageForUser" + user.Id));
						Assert.IsTrue(div.Exists);
						browser.Link(Find.ById("ViewMessageForUser" + user.Id)).Click();
						Thread.Sleep(500);
						var messageBody = browser.Table(Find.ById("MessageForUser" + user.Id));
						Assert.IsTrue(messageBody.Exists);
						browser.Button(Find.ById("CancelViewMessage" + user.Id)).Click();
						Thread.Sleep(500);
						Assert.IsFalse(messageBody.Exists);
						Assert.That(browser.Text, Text.Contains("Сообщение удалено"));
					}
				}
			}
		}

		[Test]
		public void Check_columns_by_billing_search()
		{
			using (var browser = Open("Billing/Search.rails"))
			{
				browser.Button(Find.ByValue("Найти")).Click();
				Assert.That(browser.Text, Text.Contains("Отключенных копий"));
				Assert.That(browser.Text, Text.Contains("Работающих копий"));
				Assert.That(browser.Text, Text.Contains("Отключенных адресов"));
				Assert.That(browser.Text, Text.Contains("Работающих адресов"));
			}
		}

		[Test]
		public void Check_user_for_accounting()
		{
			Client client = null;
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].Name = String.Format("Test username for Accounting [{0}]", client.Users[0].Id);
				client.Users[0].UpdateAndFlush();
				scope.VoteCommit();
			}
			using (var browser = Open("Billing/Accounting"))
			{
				Assert.That(browser.Text, Is.StringContaining("Учет адресов и пользователей"));
				Assert.That(browser.Text, Is.StringContaining(client.Users[0].Name));
			}
		}

		[Test, Description("1 пользователь, 2 адреса. 1-й адрес не должне быть в списке неучтенных, 2-й должен быть")]
		public void Check_address_for_accounting()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var user = new User { Client = client, Name = "test user", Enabled = true, };
				user.Setup(client);
				var address = new Address { Client = client, Value = "address", Enabled = true, };
				address.Save();
				client = Client.Find(client.Id);

				client.Users[0].Enabled = true;
				client.Users[0].Save();

				client.Addresses[0].Enabled = true;
				client.Addresses[0].BeAccounted = false;
				client.Addresses[0].Value = String.Format("Test address for accounting [{0}]", client.Addresses[0].Id);
				client.Addresses[0].Save();

				client.Addresses[1].Enabled = true;
				client.Addresses[1].BeAccounted = true;
				client.Addresses[1].Value = String.Format("Test address for accounting [{0}]", client.Addresses[1].Id);
				client.Addresses[1].Save();
				scope.VoteCommit();
				
				foreach (var addr in client.Addresses)
				{
					addr.AvaliableForUsers = new List<User> { client.Users[0] };
					addr.Save();
				}
				scope.VoteCommit();
			}
			using (var browser = Open("Billing/Accounting"))
			{
				Assert.That(browser.Text, Is.Not.StringContaining(client.Addresses[0].Value));
				Assert.That(browser.Text, Is.StringContaining(client.Addresses[1].Value));
			}
		}
	}
}
