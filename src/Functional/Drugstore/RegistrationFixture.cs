﻿using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Functional.ForTesting;

namespace Functional.Drugstore
{
	[TestFixture]
	public class RegistrationFixture : WatinFixture2
	{
		private string _randomClientName;
		private string _mailSuffix;
		private string _registerPageUrl;

		[SetUp]
		public void Setup()
		{
			var random = new Random();
			_randomClientName = "test" + random.Next(100000);
			_mailSuffix = "@test.test";
			_registerPageUrl = "Register/RegisterClient";
			Open(_registerPageUrl);
		}

		[Test]
		public void Test_drugstore_validate_required_general_info()
		{
			// Заполняем адрес доставки
			browser.TextField("deliveryAddress").TypeText(_randomClientName);
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить."));
			browser.TextField("JuridicalName").TypeText(_randomClientName);
			browser.TextField("ShortName").TypeText(_randomClientName);
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить."));
			browser.TextField("ClientContactPhone").TypeText("123-456789");
			browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			ClickRegisterAndCheck(browser);
		}

		[Test]
		public void Test_validate_contact_info()
		{
			browser.TextField("deliveryAddress").TypeText("Test address");
			browser.TextField("JuridicalName").TypeText(_randomClientName);
			browser.TextField("ShortName").TypeText(_randomClientName);
			browser.TextField("ClientContactPhone").TypeText("123456789");
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Некорректный телефонный номер"));
			browser.TextField("ClientContactEmail").TypeText(_randomClientName + "test.test");
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Некорректный адрес электронной почты"));
			browser.TextField("ClientContactPhone").TypeText("123-456789");
			browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			ClickRegisterAndCheck(browser);
		}

		[Test]
		public void Test_select_existsing_payer()
		{
			var payer = DataMother.TestClient().Payers.First().MakeNameUniq();
			scope.Flush();

			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("PayerExists")).Checked = true;
			Test_search_and_select(browser, "Payer", payer.Name);
		}

		[Test]
		public void Test_select_existing_supplier()
		{
			var supplier = DataMother.CreateSupplier();

			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;
			Assert.That(browser.CheckBox(Find.ById("PayerExists")).Enabled, Is.False);
			Test_search_and_select(browser, "Supplier", supplier.Name);
		}

		[Test]
		public void Register_client_for_supplier()
		{
			Supplier supplier;
			using (new SessionScope())
			{
				supplier = DataMother.CreateSupplier();
				supplier.Save();
				supplier.Name = "Тестовый поставщик " + supplier.Id;
				supplier.Save();
			}

			SetupGeneralInformation(browser);
			browser.Css("#ShowForOneSupplier").Checked = true;
			Assert.That(browser.Css("#PayerExists").Enabled, Is.False);
			browser.Css("#SearchSupplierTextPattern").TypeText(supplier.Name);
			browser.Css("#SearchSupplierButton").Click();

			Thread.Sleep(500);
			Assert.That(browser.Text, Is.StringContaining(supplier.Name), "не нашли поставщика");
			browser.SelectList(Find.ById("SupplierComboBox")).Select(String.Format("{0}. {1}", supplier.Id, supplier.Name));
			Assert.That(browser.CheckBox(Find.ById("FillBillingInfo")).Enabled, Is.False);
			Assert.That(browser.Css("#SelectSupplierDiv").Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Css("#SearchSupplierDiv").Style.Display, Is.EqualTo("none"));
			Assert.That(browser.Css("#SupplierComboBox").AllContents.Count, Is.GreaterThan(0));
			Assert.That(browser.Css("#SupplierComboBox").SelectedItem.Length, Is.GreaterThan(0));

			browser.Css("#SendRegistrationCard").Click();

			browser.Click("Зарегистрировать");

			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
			var client = GetRegistredClient();
			var settings = client.Settings;
			Assert.That(settings.NoiseCosts, Is.True);
			Assert.That(settings.NoiseCostExceptSupplier.Id, Is.EqualTo(supplier.Id));
			Assert.That(settings.InvisibleOnFirm, Is.EqualTo(DrugstoreType.Hidden));
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id)); 
		}

		private void Test_search_and_select(Browser browser, string namePart, string searchText)
		{
			var errorMessage = "Выберите поставщика";
			if (namePart.Equals("Payer"))
				errorMessage = "Выберите плательщика";

			Assert.That(browser.CheckBox(Find.ById("FillBillingInfo")).Enabled, Is.False);
			Assert.That(browser.Div(Find.ById("Search" + namePart + "Div")).Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Div(Find.ById("Select" + namePart + "Div")).Style.Display, Is.EqualTo("none"));
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining(errorMessage));

			browser.TextField(Find.ById("Search" + namePart + "TextPattern")).TypeText(searchText);
			browser.Button(Find.ById("Search" + namePart + "Button")).Click();
			Assert.That(browser.Text, Is.StringContaining("Выполняется поиск"));

			Thread.Sleep(5000);
			Assert.That(browser.Div(Find.ById("Select" + namePart + "Div")).Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Div(Find.ById("Search" + namePart + "Div")).Style.Display, Is.EqualTo("none"));
			Assert.That(browser.SelectList(Find.ById(namePart + "ComboBox")).AllContents.Count, Is.GreaterThan(0));
			Assert.That(browser.SelectList(Find.ById(namePart + "ComboBox")).SelectedItem.Length, Is.GreaterThan(0));

			// Нажимаем сброс и ищем так, чтобы ничего не найти
			browser.Button(Find.ById("Reset" + namePart + "Button")).Click();
			browser.TextField(Find.ById("Search" + namePart + "TextPattern")).TypeText("124567890sdffffasd");
			browser.Button(Find.ById("Search" + namePart + "Button")).Click();
			Thread.Sleep(5000);
			Assert.That(browser.Text, Is.StringContaining("Ничего не найдено"));
		}

		private void ClickRegisterAndCheck(Browser browser)
		{
			// Снимаем галку, чтобы не показывалась карта регистрации
			browser.CheckBox("ShowRegistrationCard").Checked = false;
			// Снимаем галку, чтобы не заполнять информацию для биллинга
			if (browser.CheckBox("FillBillingInfo").Enabled)
				browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрация завершена успешно"));
		}

		public Client GetRegistredClient()
		{
			var id = Helper.GetClientCodeFromRegistrationCard(browser);
			return Client.Find(id);
		}

		[Test]
		public void After_drugstore_registration_should_insert_record_in_user_update_info_table()
		{
			var defaults = DefaultValues.Get();
			defaults.AnalitFVersion = 705;
			defaults.Update();
			scope.Flush();

			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("FillBillingInfo")).Checked = false;
			browser.Button(Find.ById("RegisterButton")).Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта №"));

			var client = GetRegistredClient();
			var user = client.Users.First();
			var updateInfo = UserUpdateInfo.Find(user.Id);
			Assert.That(updateInfo.AFAppVersion, Is.EqualTo(705u));

			Assert.That(client.Segment, Is.EqualTo(Segment.Wholesale));
			Assert.That(client.Status, Is.EqualTo(ClientStatus.On));
			Assert.That(client.Addresses.Count, Is.EqualTo(1), "не создали адрес доставки");

			var logs = PasswordChangeLogEntity.GetByLogin(user.Login, DateTime.Today, DateTime.Today.AddDays(1));
			var passwordChange = logs.SingleOrDefault();
			Assert.That(passwordChange, Is.Not.Null);
			Assert.That(passwordChange.UserName, Is.EqualTo(Environment.UserName));
			Assert.That(passwordChange.TargetUserName, Is.EqualTo(user.Login));
			Assert.That(passwordChange.SentTo.Contains(String.Format(_randomClientName + _mailSuffix)), Is.True);
			Assert.That(passwordChange.SmtpId, Is.Not.EqualTo(0));
			Assert.That(updateInfo, Is.Not.Null, "Не создали запись в UserUpdateInfo");
		}

		[Test]
		public void Register_client_with_payer_info()
		{
			SetupGeneralInformation(browser);
			browser.Button(Find.ById("RegisterButton")).Click();
			browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");
			browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
			browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта №"));
		}

		[Test]
		public void After_registration_prices_avaliable()
		{
			var supplier = DataMother.CreateSupplier(s => s.AddRegion(Region.Find(524288ul)));
			Save(supplier);
			scope.Flush();

			SetupGeneralInformation(browser);
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button(Find.ById("RegisterButton")).Click();
			var client = GetRegistredClient();

			Assert.That(client.GetIntersectionCount(), Is.GreaterThan(0));
			Assert.That(client.Users.Count, Is.GreaterThan(0));
			Assert.That(client.Users[0].GetUserPriceCount(), Is.GreaterThan(0));
		}

		private void SetupGeneralInformation(Browser browser)
		{
			browser.TextField(Find.ById("JuridicalName")).TypeText(_randomClientName);
			browser.TextField(Find.ById("ShortName")).TypeText(_randomClientName);
			// Заполняем контактную информацию для клиента
			browser.TextField("ClientContactPhone").TypeText("123-456789");
			browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Заполняем контактную информацию для пользователя
			browser.TextField("userContactPhone").TypeText("123-456789");
			browser.TextField("userContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Если это аптека, заполняем адрес доставки
			browser.TextField(Find.ById("deliveryAddress")).TypeText(_randomClientName);
		}

		[Test]
		public void Try_to_register_with_existing_payer()
		{
			Payer payer;
			using (new SessionScope())
			{
				var payerClient = DataMother.TestClient();
				payerClient.SaveAndFlush();
				payer = payerClient.Payers.First();
				payer.Name = "Тестовый плательщик " + payer.Id;
				payer.Update();
			}

			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("PayerExists")).Checked = true;

			browser.TextField(Find.ById("SearchPayerTextPattern")).TypeText(String.Format("Тестовый плательщик {0}", payer.Id));
			browser.Button(Find.ById("SearchPayerButton")).Click();
			Thread.Sleep(2000);
			Css("#PayerComboBox").Select(String.Format("{0}. Тестовый плательщик {0}", payer.Id));
			Assert.That(Css("#FillBillingInfo").Enabled, Is.False);

			browser.Button(Find.ById("RegisterButton")).Click();

			var client = GetRegistredClient();
			Assert.That(client.Payers.First().Id, Is.EqualTo(payer.Id));
			var settings = client.Settings;
			Assert.That(settings.InvisibleOnFirm, Is.EqualTo(DrugstoreType.Standart));
			Assert.That(settings.FirmCodeOnly, Is.Null);
		}

		[Test]
		public void Check_user_regions_after_client_registration()
		{
			SetupGeneralInformation(browser);
			browser.Link(Find.ByText("Показать все регионы")).Click();
			Thread.Sleep(500);
			var regions = Region.FindAllByProperty("Name", "Чебоксары");
			var checkBox = browser.CheckBox(Find.ById("browseRegion" + regions.First().Id));
			Assert.IsTrue(checkBox.Exists);
			checkBox.Checked = true;
			// Снимаем галку, чтобы не заполнять информацию для биллинга
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", clientCode)));
			using (new SessionScope())
			{
				var client = Client.Find(clientCode);
				browser.Link(Find.ByText(client.Users[0].Login)).Click();
				var pass = false;
				for (var i = 0; i < 10; i++)
				{
					var regionCheckBox = browser.CheckBox(Find.ByName(String.Format("WorkRegions[{0}]", i)));
					Assert.IsTrue(regionCheckBox.Exists);
					if (regionCheckBox.GetValue("value").Equals(regions[0].Id.ToString()))
					{
						pass = true;
						break;
					}
				}
				Assert.IsTrue(pass);
			}
		}

		[Test]
		public void Register_client_with_user_contact_person_info()
		{
			SetupGeneralInformation(browser);
			Css("input[name='userPersons[0].Name']").TypeText("Alice");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var user = client.Users[0];
			Open(user, "Edit");
			var elementName = String.Format("persons[{0}].Name", client.Users[0].ContactGroup.Persons[0].Id);
			Assert.That(browser.TextField(Find.ByName(elementName)).Text, Is.EqualTo("Alice"));
		}

		[Test]
		public void Register_with_flag_ignore_new_prices()
		{
			SetupGeneralInformation(browser);
			browser.CheckBox("FillBillingInfo").Checked = false;
			Assert.IsFalse(browser.CheckBox(Find.ById("ignoreNewPrices")).Checked);
			browser.CheckBox(Find.ById("ignoreNewPrices")).Checked = true;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			Assert.IsTrue(client.Settings.IgnoreNewPrices);
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими телефонами для клиента")]
		public void Register_with_multiple_client_phones()
		{
			SetupGeneralInformation(browser);
			browser.Link("clientaddPhoneLink").Click();
			browser.Link("clientaddPhoneLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("clientContacts[0].ContactText")).TypeText("111-111111");
			browser.TextField(Find.ByName("clientContacts[0].Comment")).TypeText("some comment");
			browser.TextField(Find.ByName("clientContacts[2].ContactText")).TypeText("211-111111");
			browser.TextField(Find.ByName("clientContacts[3].ContactText")).TypeText("311-111111");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", clientCode)));
			Assert.That(browser.Text, Is.StringContaining("111-111111 - some comment, 211-111111, 311-111111"));

			var client = Client.Find(clientCode);
			var contacts = client.ContactGroupOwner.ContactGroups.First(g => g.Type == ContactGroupType.General).Contacts;
			Assert.That(contacts.Count, Is.EqualTo(4));
			Assert.That(contacts[0].ContactText, Is.EqualTo("111-111111"));
			Assert.That(contacts[0].Comment, Is.EqualTo("some comment"));
			Assert.That(contacts[1].ContactText, Is.EqualTo("211-111111"));
			Assert.That(contacts[2].ContactText, Is.EqualTo("311-111111"));
			Assert.That(contacts[3].Type, Is.EqualTo(ContactType.Email));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими телефонами для пользователя")]
		public void Register_with_multiple_user_phones()
		{
			SetupGeneralInformation(browser);
			browser.Link("useraddPhoneLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userContacts[0].ContactText")).TypeText("111-111111");
			browser.TextField(Find.ByName("userContacts[2].ContactText")).TypeText("222-111111");
			browser.TextField(Find.ByName("userContacts[2].Comment")).TypeText("comment for user phone");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var user = client.Users[0];
			var contacts = client.Users[0].ContactGroup.Contacts;
			Assert.That(contacts.Count, Is.EqualTo(3));
			Assert.That(contacts[0].ContactText, Is.EqualTo("111-111111"));
			Assert.That(contacts[1].Comment, Is.EqualTo("comment for user phone"));
			Assert.That(contacts[1].ContactText, Is.EqualTo("222-111111"));
			Assert.That(contacts[2].Type, Is.EqualTo(ContactType.Email));

			Open(user, "Edit");
			Assert.That(browser.Text, Is.StringContaining("Пользователь"));
			var text = browser.TextField(Find.ByName(String.Format("contacts[{0}].ContactText", contacts[1].Id))).Text;
			var comment = browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", contacts[1].Id))).Text;
			Assert.That(contacts[1].ContactText, Is.EqualTo(text));
			Assert.That(contacts[1].Comment, Is.EqualTo(comment));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими email для клиента")]
		public void Register_with_multiple_client_email()
		{
			SetupGeneralInformation(browser);
			browser.Link("clientaddEmailLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("clientContacts[1].ContactText")).TypeText("qwerty1@qq.qq");
			browser.TextField(Find.ByName("clientContacts[2].ContactText")).TypeText("qwerty2@qq.qq");
			browser.TextField(Find.ByName("clientContacts[2].Comment")).TypeText("some comment for email");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var contacts = client.ContactGroupOwner.ContactGroups[0].Contacts;
			Assert.That(contacts.Count, Is.EqualTo(3));
			Assert.That(contacts[0].Type, Is.EqualTo(ContactType.Phone));
			Assert.That(contacts[1].ContactText, Is.EqualTo("qwerty1@qq.qq"));
			Assert.That(contacts[1].Comment, Is.Null);
			Assert.That(contacts[2].ContactText, Is.EqualTo("qwerty2@qq.qq"));
			Assert.That(contacts[2].Comment, Is.EqualTo("some comment for email"));

			Open(client);
			Assert.That(browser.Text, Is.StringContaining("qwerty1@qq.qq, qwerty2@qq.qq - some comment for email"));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими email для пользователя")]
		public void Register_with_multiple_user_email()
		{
			SetupGeneralInformation(browser);
			browser.Link("useraddEmailLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userContacts[1].ContactText")).TypeText("qwerty1@qq.qq");
			browser.TextField(Find.ByName("userContacts[2].ContactText")).TypeText("qwerty2@qq.qq");
			browser.TextField(Find.ByName("userContacts[2].Comment")).TypeText("some comment for email");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			var client = Client.Find(clientCode);
			var user = client.Users[0];
			var contacts = client.Users[0].ContactGroup.Contacts;
			Assert.That(contacts.Count, Is.EqualTo(3));
			Assert.That(contacts[0].Type, Is.EqualTo(ContactType.Phone));
			Assert.That(contacts[1].ContactText, Is.EqualTo("qwerty1@qq.qq"));
			Assert.That(contacts[2].Comment, Is.EqualTo("some comment for email"));
			Assert.That(contacts[2].ContactText, Is.EqualTo("qwerty2@qq.qq"));

			Open(user, "Edit");
			var text = browser.TextField(Find.ByName(String.Format("contacts[{0}].ContactText", contacts[2].Id))).Text;
			var comment = browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", contacts[2].Id))).Text;
			Assert.That(contacts[2].ContactText, Is.EqualTo(text));
			Assert.That(contacts[2].Comment, Is.EqualTo(comment));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими контактными лицами для пользователя")]
		public void Register_with_multiple_user_persons()
		{
			SetupGeneralInformation(browser);
			browser.Link("useraddPersonLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userPersons[0].Name")).TypeText("person1");
			browser.TextField(Find.ByName("userPersons[1].Name")).TypeText("person2");
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var user = client.Users[0];
			var persons = client.Users[0].ContactGroup.Persons;
			Assert.That(persons.Count, Is.EqualTo(2));
			Assert.That(persons[0].Name, Is.EqualTo("person1"));
			Assert.That(persons[1].Name, Is.EqualTo("person2"));

			Open(user, "Edit");
			var person = browser.TextField(Find.ByName(String.Format("persons[{0}].Name", persons[0].Id))).Text;
			Assert.That(persons[0].Name, Is.EqualTo(person));
			person = browser.TextField(Find.ByName(String.Format("persons[{0}].Name", persons[1].Id))).Text;
			Assert.That(persons[1].Name, Is.EqualTo(person));
		}

		[Test, NUnit.Framework.Description("После регистрации клиента, должны бюыть выставлены флаги 'Получать накладные' и 'Получать отказы'")]
		public void After_client_registration_SendWaybills_and_SendRejects_must_be_selected()
		{
			SetupGeneralInformation(browser);
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			Assert.That(client.Users.Count, Is.EqualTo(1));
			Assert.That(client.Users[0].SendWaybills, Is.True);
			Assert.That(client.Users[0].SendRejects, Is.True);
		}

		[Test, NUnit.Framework.Description("При регистрации клиента была попытка зарегистрировать скрытую копию, но поставщика не нашли")]
		public void Register_client_with_failed_supplier_searching()
		{
			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;
			browser.TextField(Find.ByName("SearchSupplierTextPattern")).TypeText("12839046eqwuiywiuryer");
			browser.Button(Find.ByName("SearchSupplierButton")).Click();
			Thread.Sleep(1000);
			Assert.That(browser.Text, Is.StringContaining("Ничего не найдено"));
			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = false;
			browser.CheckBox("ShowRegistrationCard").Checked = false;
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрация завершена успешно"));
		}

		[Test, NUnit.Framework.Description("При регистрации клиента была попытка зарегистрировать с существующим плательщиком, но плательщика не нашли")]
		public void Register_client_with_failed_payer_searching()
		{
			SetupGeneralInformation(browser);
			browser.CheckBox(Find.ById("PayerExists")).Checked = true;
			browser.TextField(Find.ByName("SearchPayerTextPattern")).TypeText("12839046eqwuiywiuryer");
			browser.Button(Find.ByName("SearchPayerButton")).Click();
			Thread.Sleep(1000);
			Assert.That(browser.Text, Is.StringContaining("Ничего не найдено"));
			browser.CheckBox("ShowRegistrationCard").Checked = false;
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Выберите плательщика"));
			browser.CheckBox(Find.ById("PayerExists")).Checked = false;
			browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрация завершена успешно"));
		}

		[Test]
		public void Search_supplier_after_reset()
		{
			var supplier1 = DataMother.CreateSupplier();
			supplier1.Save();
			supplier1.MakeNameUniq();
			supplier1.Save();

			var supplier2 = DataMother.CreateSupplier();
			supplier2.Save();
			supplier2.MakeNameUniq();
			supplier1.Save();
			Flush();

			Css("#ShowForOneSupplier").Click();
			SearchSupplier(supplier1.Name);

			Assert.That(Css("#SupplierComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#SupplierComboBox").SelectedItem, Is.StringEnding(supplier1.Name));
			Css("#ResetSupplierButton").Click();

			SearchSupplier(supplier2.Name);
			Assert.That(Css("#SupplierComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#SupplierComboBox").SelectedItem, Is.StringEnding(supplier2.Name));
		}

		private void SearchSupplier(string text)
		{
			Css("#SearchSupplierTextPattern").TypeText(text);
			Css("#SearchSupplierButton").Click();
			Thread.Sleep(1000);
		}
	}
}