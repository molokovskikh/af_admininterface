﻿using System.Collections.Generic;
using AdminInterface.Models.Billing;
using NUnit.Framework;
using WatiN.Core;
using Functional.ForTesting;
using AdminInterface.Models;
using System;
using System.Threading;
using Common.Web.Ui.Helpers;
using Castle.ActiveRecord;
using System.Linq;

namespace Functional
{
	public class BillingFixture : WatinFixture
	{
		[SetUp]
		public void Setup()
		{
			UseTestScope = true;
		}

		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			using (var scope = new TransactionScope())
			{
				var payer = Payer.Find(921u);
				payer.ShortName = "Офис123";
				payer.UpdateAndFlush();
				scope.VoteCommit();
			}

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
			client.Refresh();
			client.BillingInstance.ShortName += client.BillingInstance.PayerID;
			client.BillingInstance.UpdateAndFlush();
			client.AddAddress(new Address {Client = client, Value = "test address for billing",});
			client.UpdateAndFlush();
			foreach (var address in client.Addresses)
			{
				address.Enabled = false;
				address.UpdateAndFlush();
			}

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				Assert.That(browser.Text, Text.Contains(String.Format("Адреса клиента \"{0}\"", client.Name)));
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

			client.Refresh();
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
				Assert.That(browser.Text, Text.Contains(String.Format("Пользователи клиента \"{0}\"", client.Name)));
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
				client.Refresh();
				var address = client.Addresses[0];
				var addressId = "usersForAddress" + address.Id;
				browser.Link(Find.ById(addressId)).Click();
				Thread.Sleep(500);
				Assert.That(browser.Text, Text.Contains("Пользователи"));
				Assert.That(browser.Text, Text.Contains("Подключить пользователя"));
				// Щелкаем по адресу. Должна быть показана дополнительная информация
				var additionalInfoDiv = browser.Div(Find.ById("additionalAddressInfo" + address.Id));
				Assert.That(additionalInfoDiv.Exists, Is.True);
				Assert.That(additionalInfoDiv.Enabled, Is.True);
				// Щелкаем второй раз. Дополнительная информация должна быть скрыта
				browser.Link(Find.ById(addressId)).Click();
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
				client.Refresh();
				var user = client.Users[0];
				var userId = "addressesForUser" + user.Id;
				browser.Link(Find.ById(userId)).Click();
				Thread.Sleep(500);
				Assert.That(browser.Text, Text.Contains("Адреса"));
				Assert.That(browser.Text, Text.Contains("Подключить адрес"));
				// Щелкаем по адресу. Должна быть показана дополнительная информация
				var additionalInfoDiv = browser.Div(Find.ById("additionalUserInfo" + user.Id));
				Assert.That(additionalInfoDiv.Exists, Is.True);
				Assert.That(additionalInfoDiv.Enabled, Is.True);
				// Щелкаем второй раз. Дополнительная информация должна быть скрыта
				browser.Link(Find.ById(userId)).Click();
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
				client.Refresh();
				var address = client.Addresses[0];
				var user = client.Users[0];
				Assert.That(client.Addresses[0].AvaliableForUsers.Count, Is.EqualTo(0));
				address.AvaliableForUsers = new List<User>();
				browser.Link(Find.ById("usersForAddress" + address.Id)).Click();
				Thread.Sleep(500);
				var connectUserLink = browser.Link(Find.ByText("Подключить пользователя"));
				connectUserLink.Click();
				Assert.That(connectUserLink.Style.Display, Is.EqualTo("none"));
				browser.Button(Find.ById("SearchUserButton" + address.Id)).Click();
				Thread.Sleep(500);
				var comboBox = browser.SelectList(Find.ById("UsersComboBox" + address.Id));
				Assert.That(comboBox.Options.Length, Is.GreaterThan(0));
				Assert.That(comboBox.HasSelectedItems, Is.True);
				Assert.That(comboBox.SelectedOption.Text.Contains(user.GetLoginOrName()));
				browser.Button(Find.ById("ConnectAddressToUserButton" + address.Id)).Click();
				Thread.Sleep(2000);
				Assert.That(connectUserLink.Style.Display, Is.EqualTo("block"));
				var connectingDiv = browser.Div(Find.ById("ConnectingUserDiv" + address.Id));
				Assert.That(connectingDiv.Style.Display, Is.EqualTo("none"));
			}

			client.Refresh();
			scope.Evict(client);

			client = Client.Find(client.Id);
			Assert.That(client.Addresses[0].AvaliableFor(client.Users[0]), Is.True);
		}

		[Test]
		public void Adding_addresses_to_users()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
				var address = client.Addresses[0];
				var user = client.Users[0];
				Assert.That(client.Addresses[0].AvaliableForUsers.Count, Is.EqualTo(0));
				address.AvaliableForUsers = new List<User>();
				browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
				Thread.Sleep(500);
				var connectAddressLink = browser.Link(Find.ByText("Подключить адрес"));
				connectAddressLink.Click();
				Assert.That(connectAddressLink.Style.Display, Is.EqualTo("none"));
				browser.Button(Find.ById("SearchAddressButton" + user.Id)).Click();
				Thread.Sleep(500);
				var comboBox = browser.SelectList(Find.ById("AddressesComboBox" + user.Id));
				Assert.That(comboBox.Options.Length, Is.GreaterThan(0));
				Assert.That(comboBox.HasSelectedItems, Is.True);
				Assert.That(comboBox.SelectedOption.Text.Contains(address.Value));

				browser.Button(Find.ById("ConnectAddressToUserButton" + user.Id)).Click();
				Thread.Sleep(2000);
				Assert.That(connectAddressLink.Style.Display, Is.EqualTo("block"));
				var connectingDiv = browser.Div(Find.ById("ConnectingAddressDiv" + user.Id));
				Assert.That(connectingDiv.Style.Display, Is.EqualTo("none"));
			}

			client.Refresh();
			Assert.That(client.Addresses[0].AvaliableFor(client.Users[0]), Is.True);
		}

		[Test]
		public void DisconnectUserFromAddress()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
				var address = client.Addresses[0];
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
			}

			client.Refresh();
			Assert.That(client.Addresses[0].AvaliableFor(client.Users[0]), Is.False);
		}

		[Test]
		public void DisconnectAddressFromUser()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))			
			{
				client.Refresh();
				var address = client.Addresses[0];
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
			}

			client.Refresh();
			Assert.That(client.Addresses[0].AvaliableFor(client.Users[0]), Is.False);
		}

		[Test]
		public void Change_user_status()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Refresh();
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
			client.Refresh();
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
			client.Refresh();
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
				Assert.That(userRow.ClassName, Text.DoesNotContain("DisabledByBilling"));
				Assert.That(addressRow.ClassName, Text.DoesNotContain("DisabledByBilling"));
				Assert.That(clientRow.ClassName, Text.DoesNotContain("DisabledByBilling"));
				clientStatus.Click();
				Thread.Sleep(2000);
				Assert.IsTrue(userStatus.Checked);
				Assert.IsFalse(userStatus.Enabled);
				Assert.IsTrue(addressStatus.Checked);
				Assert.IsFalse(addressStatus.Enabled);
				Assert.IsFalse(clientStatus.Checked);
				Assert.That(userRow.ClassName, Text.DoesNotContain("DisabledByBilling"));
				Assert.That(addressRow.ClassName, Text.DoesNotContain("DisabledByBilling"));
				Assert.That(clientRow.ClassName, Text.Contains("DisabledByBilling"));
			}
		}

		[Test]
		public void Test_view_connecting_user_to_address()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
				var address = client.Addresses[0];
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
				Assert.That(browser.Div(Find.ById("SearchUserMessage" + address.Id)).Text, Text.Contains("Ничего не найдено"));
			}
		}

		[Test]
		public void Test_view_connecting_address_to_user()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
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
				Assert.That(browser.Div(Find.ById("SearchAddressMessage" + user.Id)).Text, Text.Contains("Ничего не найдено"));
			}
		}

		private void AddUsers(Client client, int countUsers)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				for (var i = 0; i < countUsers; i++)
				{
					var user = new User { Client = client, Name = "user", };
					user.Setup(client);
					var address = new Address { Client = client, Value = "address", };
					client.AddAddress(address);
					address.Save();
				}
				scope.VoteCommit();
			}
		}

		private void AddClientWithSamePayer(Payer payer)
		{
			
		}

		[Test(Description = "Тест сворачивания/разворачивания заголовков списков клиентов, пользователей и адресов")]
		public void Test_collapse_and_spread_headers()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			AddUsers(client, 10);

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
				client.Refresh();
				var user = new User { Client = client, Name = "test user", Enabled = true, };
				user.Setup(client);
				var address = new Address { Client = client, Value = "address", Enabled = true, };
				client.AddAddress(address);
				address.Save();
				address = new Address { Client = client,  Value = "address", Enabled = true, };
				client.AddAddress(address);
				address.Save();
				client.Users[0].Enabled = true;
				client.Users[0].Save();
				client.Addresses[0].Enabled = false;
				client.Addresses[0].Save();
				client = Client.Find(client.Id);
				foreach (var addr in client.Addresses)
				{
					addr.AvaliableForUsers = new List<User> { client.Users[0], client.Users[1] };
					addr.Save();
				}
				scope.VoteCommit();
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
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
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
				ArHelper.WithSession(session => session.CreateSQLQuery(@"
DELETE FROM future.Addresses WHERE Id = :AddressId
;
DELETE FROM future.Users WHERE Id = :UserId
;")
				.SetParameter("AddressId", client.Addresses[0].Id)
				.SetParameter("UserId", client.Users[0].Id)
				.ExecuteUpdate());

				var errorMessageDiv = browser.Div(Find.ById("ErrorMessageDiv"));
				Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("none"));

				// Пытаемся посмотреть адреса, доступные пользователю
				browser.Link(Find.ById("addressesForUser" + user.Id)).Click();
				Thread.Sleep(500);

				Assert.IsTrue(errorMessageDiv.Exists);
				Assert.IsTrue(errorMessageDiv.Style.Display.ToLower().Equals("block"));
				Assert.IsTrue(errorMessageDiv.Text.Equals("При выполнении операции возникла ошибка. Попробуйте повторить позднее"));
			}
		}

		[Test]
		public void Send_message_to_user()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open(string.Format("Client/{0}", client.Id)))
			{
				var username = client.Users[0].GetLoginOrName();
				browser.Link(Find.ByText("Биллинг")).Click();
				browser.SelectList(Find.ByName("NewClientMessage.ClientCode")).Select(username);
				var messageText = "test message for user " + username;
				browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(messageText);
				browser.Button(Find.ByValue("Отправить сообщение")).Click();
				Assert.That(browser.Text, Text.Contains("Сообщение сохранено"));
				Assert.That(browser.Text, Text.Contains(String.Format("Остались не показанные сообщения для пользователя {0}", username)));
				browser.Link(Find.ByText("Просмотреть сообщение")).Click();
				Thread.Sleep(500);
				Assert.That(browser.Text, Text.Contains(messageText));
				var messages = Client.Find(client.Id).Users.Select(u => ClientMessage.Find(u.Id)).ToList();
				messages[0].Refresh();
				Assert.That(messages[0].Message, Is.EqualTo(messageText));
				Assert.That(messages[0].ShowMessageCount, Is.EqualTo(1));
			}
		}

		[Test]
		public void Cancel_message_for_user()
		{
			ClientMessage message;
			var messageText = String.Empty;

			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open(String.Format("Client/{0}", client.Id)))
			{
				client.Refresh();
				var username = client.Users[0].GetLoginOrName();
				browser.Link(Find.ByText("Биллинг")).Click();
				browser.SelectList(Find.ByName("NewClientMessage.ClientCode")).Select(username);
				messageText = "test message for user " + username;
				browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText(messageText);
				browser.Button(Find.ByValue("Отправить сообщение")).Click();
				browser.Link(Find.ByText("Просмотреть сообщение")).Click();
				Thread.Sleep(500);
				browser.Button(String.Format("CancelViewMessage{0}", client.Users[0].Id)).Click();
				var messages = Client.Find(client.Id).Users.Select(u => ClientMessage.Find(u.Id)).ToList();
				message = messages[0];
			}

			message.Refresh();
			Assert.That(message.Message, Is.EqualTo(messageText));
			Assert.That(message.ShowMessageCount, Is.EqualTo(0));
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
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Refresh();
			client.Users[0].Name = String.Format("Test username for Accounting [{0}]", client.Users[0].Id);
			client.Users[0].UpdateAndFlush();

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
				client.AddAddress(address);
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

		[Test, Description("Проверка фильтрации записей в статистике вкл./откл. по пользователю")]
		public void FilterLogMessagesByUser()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			AddUsers(client, 3);
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
					// Проверяем, что логины пользователей - это ссылки
				foreach (var usr in client.Users)
					Assert.IsTrue(browser.Link(Find.ByText(usr.Login)).Exists);

				// Кликаем по логину одного из пользователей
				var user = client.Users[2];
				browser.Link(Find.ByText(user.Login)).Click();
				// В таблице статистики вкл./откл. должны остаться видимыми только строки, относящиеся к выбранному пользователю
				// остальные строки должны быть невидимы
				var logRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("logRow"));
				foreach (TableRow row in logRows)
				{
					if (row.Id.Equals("logRowUserLog" + user.Id))
						Assert.That(row.Style.Display, Is.Null);
					else
						Assert.That(row.Style.Display, Is.StringContaining("none"));
				}
				// Логин-ссылка должна стать невидимой, видимым должен быть логин-текст (такое обозначение у выделенного пользователя)
				Assert.That(browser.Link("UserLink" + user.Id).Style.Display, Is.EqualTo("none"));
				Assert.That(browser.Div("UserValue" + user.Id).Style.Display, Is.Null);
			}
		}

		[Test, Description("Проверка, что по клику на логин, этот пользователь выбирается в выпадающем списке 'Сообщение для пользователя:'")]
		public void SelectUserForSendMessage()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			AddUsers(client, 3);
			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				client.Refresh();
				// Кликаем по логину одного из пользователей
				var user = client.Users[2];
				browser.Link(Find.ByText(user.Login)).Click();
				// Этот пользователь должен стать выделенным в списке "Сообщение для пользователя"
				Assert.That(browser.SelectList(Find.ByName("NewClientMessage.ClientCode")).SelectedOption.Text, Is.EqualTo(user.GetLoginOrName()));
			}
		}

		[Test, Description("Проверка фильтрации записей в статистике вкл./откл. по адресу доставки")]
		public void FilterLogMessagesByAddress()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
		}

		[Test, Description("Проверка, что при выделении клиента, отображаются адреса и пользователи только для выбранного клиента")]
		public void ShowUsersOnlyForSelectedClient()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var client2 = DataMother.CreateTestClientWithAddressAndUser();
			uint testUserId = 0;

			client.Name += client.Id;
			client.UpdateAndFlush();
			client2.Name += client2.Id;
			client2.BillingInstance = client.BillingInstance;
			client2.UpdateAndFlush();
			client.Refresh();
			client2.Refresh();
			testUserId = client2.Users[0].Id;

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var clientRows = browser.TableRows.Where(row => (row != null) && (row.Id != null) && row.Id.Contains("ClientRow")).ToList();
				Assert.That(clientRows.Count, Is.EqualTo(2));
				browser.Div(Find.ById("ClientListHeader")).Click();
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
		}

		[Test]
		public void ShowAddressesOnlyForSelectedClient()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var client2 = DataMother.CreateTestClientWithAddressAndUser();
			uint testAddressId = 0;
			client.Name += client.Id;
			client.UpdateAndFlush();
			client2.Name += client2.Id;
			client2.BillingInstance = client.BillingInstance;
			client2.UpdateAndFlush();
			client.Refresh();
			client2.Refresh();
			testAddressId = client2.Addresses[0].Id;

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
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
		}

		[Test]
		public void Show_all_users_for_payer()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var client2 = DataMother.CreateTestClientWithAddressAndUser();

			client.Name += client.Id;
			client.UpdateAndFlush();
			client2.Name += client2.Id;
			client2.BillingInstance = client.BillingInstance;
			client2.UpdateAndFlush();
			client.Refresh();
			client2.Refresh();

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				var usersTable = browser.Table("UsersTable");
				var countVisibleRows = 0;
				foreach (TableRow row in usersTable.TableRows)
					if ((row != null) && (row.Id != null) && !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"))
						countVisibleRows++;
				browser.Link(Find.ById("ShowOrHideUsers")).Click();
				Thread.Sleep(1000);

				var countVisibleRows2 = 0;
				foreach (TableRow row in usersTable.TableRows)
					if ((row != null) && (row.Id != null) && !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"))
						countVisibleRows2++;
				Assert.That(countVisibleRows, Is.LessThan(countVisibleRows2));
				browser.Link(Find.ById("ShowOrHideUsers")).Click();
				Thread.Sleep(1000);

				var countVisibleRows3 = 0;
				foreach (TableRow row in usersTable.TableRows)
					if ((row != null) && (row.Id != null) && !row.Id.Contains("UserRowHidden") && (row.Style.Display != "none"))
						countVisibleRows3++;
				Assert.That(countVisibleRows, Is.EqualTo(countVisibleRows3));
				browser.Link(Find.ById("ShowOrHideUsers")).Click();
				Thread.Sleep(1000);
			}
		}

		[Test]
		public void Show_payer_with_no_addresses()
		{
			var client = DataMother.CreateTestClientWithUser();

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				Assert.That(browser.Text, Is.StringContaining(String.Format("Плательщик {0}", client.BillingInstance.ShortName)));
			}
		}

		[Test]
		public void Show_payer_with_no_users()
		{
			var client = DataMother.CreateTestClientWithAddress();

			using (var browser = Open("Billing/Edit.rails?BillingCode=" + client.BillingInstance.PayerID))
			{
				Assert.That(browser.Text, Is.StringContaining(String.Format("Плательщик {0}", client.BillingInstance.ShortName)));
			}
		}

		[Test]
		public void Change_recipient_for_payer()
		{
			var client = DataMother.CreateTestClientWithAddress();
			using(var browser = Open("Billing/Edit.rails?billingCode="+ client.BillingInstance.Id))
			{
				browser.Link(Find.ByText("Юридические лица")).Click();
				var juridicalOrganization = client.BillingInstance.JuridicalOrganizations.Single();
				browser.Link(Find.ById(String.Format("JuridicalOrganization{0}Link", juridicalOrganization.Id))).Click();
				var select = browser.Div(String.Format("JuridicalOrganization{0}Body", juridicalOrganization.Id)).SelectList(Find.ByName("JuridicalOrganization.Recipient.Id"));
				Assert.That(select.SelectedItem, Is.Null);
				select.Select("ООО «АналитФармация»");
				browser.Div("juridicalOrganization-tab").Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.That(browser.Div(String.Format("JuridicalOrganization{0}Body", juridicalOrganization.Id)).SelectList(Find.ByName("JuridicalOrganization.Recipient.Id")).SelectedItem, Is.EqualTo("ООО «АналитФармация»"));
			}
		}

		[Test, Description("Тест перехода на закладку с юридическими лицами из таблицы с адресами (должно раскрываться только юр. лицо, с которым связан выбранный адрес)")]
		public void Go_to_juridical_organization_from_address()
		{
			Client client;

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				var juridicalOrganization = new JuridicalOrganization() {
					Name = "Test" + client.Id,
					Payer = client.BillingInstance,
				};
				juridicalOrganization.CreateAndFlush();
				scope.VoteCommit();
			}

			using (var browser = Open("Client/{0}/Info", client.Id))
			{
				
			}
		}

		[Test, Description("Создание нового юридического лица")]
		public void Create_new_juridical_organization()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();

			using (var browser = Open("Client/{0}/Info", client.Id))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				browser.Link(Find.ByText("Юридические лица")).Click();
				Assert.IsTrue(browser.Div(Find.ById("FormForAdding")).Exists);
				Assert.That(browser.Div(Find.ById("FormForAdding")).Style.Display, Is.EqualTo("none"));
				Assert.That(browser.Button(Find.ById("AddJuridicalOrganizationButton")).Style.Display, Is.Null);
				browser.Button(Find.ById("AddJuridicalOrganizationButton")).Click();
				Thread.Sleep(500);
				Assert.That(browser.Div(Find.ById("FormForAdding")).Style.Display, Is.EqualTo("block"));
				Assert.That(browser.Button(Find.ById("AddJuridicalOrganizationButton")).Style.Display, Is.EqualTo("none"));
				browser.TextField(Find.ByName("JuridicalOrganization.Name")).TypeText(String.Format("TestJuridicalNameForClient{0}", client.Id));
				browser.Button(Find.ById("SubmitFormButton")).Click();
				Assert.That(browser.Text, Is.StringContaining("Юридическое лицо создано"));

				var count = JuridicalOrganization.Queryable.Count(u => u.Name == String.Format("TestJuridicalNameForClient{0}", client.Id));
				Assert.That(count, Is.EqualTo(1));
			}
		}

		[Test]
		public void Update_juridical_organization_info()
		{
			Client client;
			JuridicalOrganization juridicalOrganization;

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				juridicalOrganization = new JuridicalOrganization() {
					Name = "Test" + client.Id,
					Payer = client.BillingInstance,
				};
				juridicalOrganization.CreateAndFlush();
				scope.VoteCommit();
			}

			using (var browser = Open("Client/{0}/Info", client.Id))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
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
}
