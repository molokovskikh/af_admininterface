﻿using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using Functional.ForTesting;
using WatiN.Core.Native.Windows;
using WatiN.CssSelectorExtensions;

namespace Functional.Drugstore
{
	[TestFixture]
	public class RegistrationFixture : FunctionalFixture
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
			var defaultSettings = session.Query<DefaultValues>().First();
			defaultSettings.AddressesHelpText = "Тестовый текст памятки адреса при регистрации";
			session.Save(defaultSettings);
			Open(_registerPageUrl);
		}

		[Test]
		public void Test_drugstore_validate_required_general_info()
		{
			Css("#user_Name").TypeText("Тестовый пользователь");

			// Заполняем адрес доставки
			Css("#address_Value").TypeText(_randomClientName);
			browser.Button("RegisterButton").Click();
			AssertText("Это поле необходимо заполнить.");
			Css("#client_FullName").TypeText(_randomClientName);
			Css("#client_Name").TypeText(_randomClientName);
			browser.Button("RegisterButton").Click();
			AssertText("Это поле необходимо заполнить.");
			Css("#ClientContactPhone").TypeText("123-4567890");
			Css("#ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			ClickRegisterAndCheck(browser);
		}

		[Test]
		public void Test_validate_contact_info()
		{
			Css("#user_Name").TypeText("Тестовый пользователь");

			Css("#address_Value").TypeText("Test address");
			Css("#client_FullName").TypeText(_randomClientName);
			Css("#client_Name").TypeText(_randomClientName);
			Css("#ClientContactPhone").TypeText("123456789");
			browser.Button("RegisterButton").Click();
			AssertText("Некорректный телефонный номер");
			Css("#ClientContactEmail").TypeText(_randomClientName + "test.test");
			browser.Button("RegisterButton").Click();
			AssertText("Некорректный адрес электронной почты");
			Css("#ClientContactPhone").TypeText("123-4567890");
			Css("#ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			ClickRegisterAndCheck(browser);
		}

		[Test]
		public void Test_select_existsing_payer()
		{
			var payer = DataMother.TestClient().Payers.First().MakeNameUniq();
			FlushAndCommit();

			SetupGeneralInformation();
			browser.CheckBox(Find.ById("PayerExists")).Checked = true;
			Test_search_and_select(browser, "Payer", payer.Name);
		}

		[Test]
		public void Test_select_existing_supplier()
		{
			var supplier = DataMother.CreateSupplier();

			SetupGeneralInformation();
			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;
			Assert.That(browser.CheckBox(Find.ById("PayerExists")).Enabled, Is.False);
			Test_search_and_select(browser, "Supplier", supplier.Name);
		}

		[Test]
		public void Register_client_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			supplier.Name = "Тестовый поставщик " + supplier.Id;
			Save(supplier);
			FlushAndCommit();

			SetupGeneralInformation();
			Css("#ShowForOneSupplier").Checked = true;
			Assert.That(Css("#PayerExists").Enabled, Is.False);
			Css("#SearchSupplierTextPattern").TypeText(supplier.Name);
			Css("#SearchSupplierButton").Click();

			WaitForText(supplier.Name);
			Assert.That(browser.Text, Is.StringContaining(supplier.Name), "не нашли поставщика");
			browser.SelectList(Find.ById("SupplierComboBox")).Select(String.Format("{0}. {1}", supplier.Id, supplier.Name));
			Assert.That(Css("#options_FillBillingInfo").Enabled, Is.False);
			Assert.That(Css("#SelectSupplierDiv").Style.Display, Is.EqualTo("block"));
			Assert.That(Css("#SearchSupplierDiv").Style.Display, Is.EqualTo("none"));
			Assert.That(Css("#SupplierComboBox").AllContents.Count, Is.GreaterThan(0));
			Assert.That(Css("#SupplierComboBox").SelectedItem.Length, Is.GreaterThan(0));

			Css("#options_SendRegistrationCard").Click();

			Click("Зарегистрировать");

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

			Assert.That(Css("#options_FillBillingInfo").Enabled, Is.False);
			Assert.That(browser.Div(Find.ById("Search" + namePart + "Div")).Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Div(Find.ById("Select" + namePart + "Div")).Style.Display, Is.EqualTo("none"));
			browser.Button("RegisterButton").Click();
			AssertText(errorMessage);

			browser.TextField(Find.ById("Search" + namePart + "TextPattern")).TypeText(searchText);
			browser.Button(Find.ById("Search" + namePart + "Button")).Click();
			AssertText("Выполняется поиск");

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
			AssertText("Ничего не найдено");
		}

		[Test]
		public void After_drugstore_registration_should_insert_record_in_user_update_info_table()
		{
			var defaults = session.Query<DefaultValues>().First();
			defaults.AnalitFVersion = 705;
			Save(defaults);
			FlushAndCommit();

			SetupGeneralInformation();
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button(Find.ById("RegisterButton")).Click();

			var client = GetRegistredClient();
			var user = client.Users.First();
			var updateInfo = session.Load<UserUpdateInfo>(user.Id);
			Assert.That(updateInfo.AFAppVersion, Is.EqualTo(705u));

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
			SetupGeneralInformation();
			browser.Button(Find.ById("RegisterButton")).Click();
			browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");
			browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
			browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
			ClickButton("Сохранить");
			AssertText("Регистрационная карта №");
		}

		[Test]
		public void After_registration_prices_avaliable()
		{
			var supplier = DataMother.CreateSupplier(s => s.AddRegion(session.Load<Region>(524288ul), session));
			Save(supplier);
			FlushAndCommit();

			SetupGeneralInformation();
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button(Find.ById("RegisterButton")).Click();
			var client = GetRegistredClient();

			Assert.That(client.GetIntersectionCount(session), Is.GreaterThan(0));
			Assert.That(client.Users.Count, Is.GreaterThan(0));
			Assert.That(client.Users[0].GetUserPriceCount(session), Is.GreaterThan(0));
		}

		[Test]
		public void Try_to_register_with_existing_payer()
		{
			var payerClient = DataMother.TestClient();
			session.SaveOrUpdate(payerClient);
			var payer = payerClient.Payers.First();
			payer.Name = "Тестовый плательщик " + payer.Id;
			session.SaveOrUpdate(payer);
			Refresh();

			SetupGeneralInformation();

			SearchPayer(String.Format("Тестовый плательщик {0}", payer.Id));

			Css("#PayerComboBox").Select(String.Format("{0}. Тестовый плательщик {0}", payer.Id));
			Assert.That(Css("#options_FillBillingInfo").Enabled, Is.False);

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
			SetupGeneralInformation();
			ClickLink("Показать все регионы");
			Thread.Sleep(500);
			var region = session.Query<Region>().First(r => r.Name == "Чебоксары");
			var checkBox = browser.CheckBox(Find.ById("browseRegion" + region.Id));
			Assert.IsTrue(checkBox.Exists);
			checkBox.Checked = true;
			// Снимаем галку, чтобы не заполнять информацию для биллинга
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", clientCode)));

			var client = session.Load<Client>(clientCode);
			ClickLink(client.Users[0].Login);
			Click("Настройка");
			var pass = false;
			for (var i = 0; i < 10; i++) {
				var regionCheckBox = browser.CheckBox(Find.ByName(String.Format("WorkRegions[{0}]", i)));
				Assert.IsTrue(regionCheckBox.Exists);
				if (regionCheckBox.GetValue("value").Equals(region.Id.ToString())) {
					pass = true;
					break;
				}
			}
			Assert.IsTrue(pass);
		}

		[Test]
		public void Register_client_with_user_contact_person_info()
		{
			SetupGeneralInformation();
			Css("input[name='userPersons[0].Name']").TypeText("Alice");
			Css("#options_FillBillingInfo").Checked = false;
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
			SetupGeneralInformation();
			Css("#options_FillBillingInfo").Checked = false;
			Assert.IsFalse(browser.CheckBox(Find.ById("client_Settings_IgnoreNewPrices")).Checked);
			browser.CheckBox(Find.ById("client_Settings_IgnoreNewPrices")).Checked = true;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			Assert.IsTrue(client.Settings.IgnoreNewPrices);
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими телефонами для клиента")]
		public void Register_with_multiple_client_phones()
		{
			SetupGeneralInformation();
			browser.Link("clientaddPhoneLink").Click();
			browser.Link("clientaddPhoneLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("clientContacts[0].ContactText")).TypeText("111-1111111");
			browser.TextField(Find.ByName("clientContacts[0].Comment")).TypeText("some comment");
			browser.TextField(Find.ByName("clientContacts[2].ContactText")).TypeText("211-1111111");
			browser.TextField(Find.ByName("clientContacts[3].ContactText")).TypeText("311-1111111");
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", clientCode)));
			AssertText("111-1111111 - some comment, 211-1111111, 311-1111111");

			var client = session.Load<Client>(clientCode);
			var contacts = client.ContactGroupOwner.ContactGroups
				.First(g => g.Type == ContactGroupType.General && !g.Specialized)
				.Contacts;
			Assert.That(contacts.Count, Is.EqualTo(4));
			Assert.That(contacts[0].ContactText, Is.EqualTo("111-1111111"));
			Assert.That(contacts[0].Comment, Is.EqualTo("some comment"));
			Assert.That(contacts[1].ContactText, Is.EqualTo("211-1111111"));
			Assert.That(contacts[2].ContactText, Is.EqualTo("311-1111111"));
			Assert.That(contacts[3].Type, Is.EqualTo(ContactType.Email));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими телефонами для пользователя")]
		public void Register_with_multiple_user_phones()
		{
			SetupGeneralInformation();
			browser.Link("useraddPhoneLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userContacts[0].ContactText")).TypeText("111-1111111");
			browser.TextField(Find.ByName("userContacts[2].ContactText")).TypeText("222-1111111");
			browser.TextField(Find.ByName("userContacts[2].Comment")).TypeText("comment for user phone");
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var user = client.Users[0];
			var contacts = client.Users[0].ContactGroup.Contacts;
			Assert.That(contacts.Count, Is.EqualTo(3));
			Assert.That(contacts[0].ContactText, Is.EqualTo("111-1111111"));
			Assert.That(contacts[1].Comment, Is.EqualTo("comment for user phone"));
			Assert.That(contacts[1].ContactText, Is.EqualTo("222-1111111"));
			Assert.That(contacts[2].Type, Is.EqualTo(ContactType.Email));

			Open(user, "Edit");
			AssertText("Пользователь");
			var text = browser.TextField(Find.ByName(String.Format("contacts[{0}].ContactText", contacts[1].Id))).Text;
			var comment = browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", contacts[1].Id))).Text;
			Assert.That(contacts[1].ContactText, Is.EqualTo(text));
			Assert.That(contacts[1].Comment, Is.EqualTo(comment));
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими email для клиента")]
		public void Register_with_multiple_client_email()
		{
			SetupGeneralInformation();
			browser.Link("clientaddEmailLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("clientContacts[1].ContactText")).TypeText("qwerty1@qq.qq");
			browser.TextField(Find.ByName("clientContacts[2].ContactText")).TypeText("qwerty2@qq.qq");
			browser.TextField(Find.ByName("clientContacts[2].Comment")).TypeText("some comment for email");
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			var contacts = client.ContactGroupOwner.ContactGroups
				.First(g => g.Type == ContactGroupType.General && !g.Specialized).Contacts;
			Assert.That(contacts.Count, Is.EqualTo(3), contacts.Implode());
			Assert.That(contacts[0].Type, Is.EqualTo(ContactType.Phone));
			Assert.That(contacts[1].ContactText, Is.EqualTo("qwerty1@qq.qq"));
			Assert.That(contacts[1].Comment, Is.Null);
			Assert.That(contacts[2].ContactText, Is.EqualTo("qwerty2@qq.qq"));
			Assert.That(contacts[2].Comment, Is.EqualTo("some comment for email"));

			Open(client);
			AssertText("qwerty1@qq.qq, qwerty2@qq.qq - some comment for email");
		}

		[Test, NUnit.Framework.Description("Регистрация клиента с несколькими email для пользователя")]
		public void Register_with_multiple_user_email()
		{
			SetupGeneralInformation();
			browser.Link("useraddEmailLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userContacts[1].ContactText")).TypeText("qwerty1@qq.qq");
			browser.TextField(Find.ByName("userContacts[2].ContactText")).TypeText("qwerty2@qq.qq");
			browser.TextField(Find.ByName("userContacts[2].Comment")).TypeText("some comment for email");
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
			var client = session.Load<Client>(clientCode);
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
			SetupGeneralInformation();
			browser.Link("useraddPersonLink").Click();
			Thread.Sleep(500);
			browser.TextField(Find.ByName("userPersons[0].Name")).TypeText("person1");
			browser.TextField(Find.ByName("userPersons[1].Name")).TypeText("person2");
			Css("#options_FillBillingInfo").Checked = false;
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
			var client = Register();
			Assert.That(client.Users.Count, Is.EqualTo(1));
			Assert.That(client.Users[0].SendWaybills, Is.False);
			Assert.That(client.Users[0].SendRejects, Is.True);
		}

		[Test, NUnit.Framework.Description("При регистрации клиента была попытка зарегистрировать скрытую копию, но поставщика не нашли")]
		public void Register_client_with_failed_supplier_searching()
		{
			SetupGeneralInformation();

			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;
			SearchSupplier("12839046eqwuiywiuryer");
			AssertText("Ничего не найдено");
			browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = false;
			Css("#options_ShowRegistrationCard").Checked = false;
			browser.Button("RegisterButton").Click();
			AssertText("Регистрация завершена успешно");
		}

		[Test, NUnit.Framework.Description("При регистрации клиента была попытка зарегистрировать с существующим плательщиком, но плательщика не нашли")]
		public void Register_client_with_failed_payer_searching()
		{
			SetupGeneralInformation();

			SearchPayer("12839046eqwuiywiuryer");
			AssertText("Ничего не найдено");
			Css("#options_ShowRegistrationCard").Checked = false;
			browser.Button("RegisterButton").Click();
			AssertText("Выберите плательщика");
			browser.CheckBox(Find.ById("PayerExists")).Checked = false;
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			AssertText("Регистрация завершена успешно");
		}

		[Test]
		public void Search_supplier_after_reset()
		{
			var supplier1 = DataMother.CreateSupplier();
			MakeNameUniq(supplier1);

			var supplier2 = DataMother.CreateSupplier();
			MakeNameUniq(supplier2);
			FlushAndCommit();

			Css("#ShowForOneSupplier").Click();
			SearchSupplier(supplier1.Name);

			Assert.That(Css("#SupplierComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#SupplierComboBox").SelectedItem, Is.StringEnding(supplier1.Name));
			Css("#ResetSupplierButton").Click();

			SearchSupplier(supplier2.Name);
			Assert.That(Css("#SupplierComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#SupplierComboBox").SelectedItem, Is.StringEnding(supplier2.Name));
		}

		[Test]
		public void Select_mask_region_after_home_region_change()
		{
			var region = Region.All(session).First(r => r.Name == "Златоуст");
			Css("#HomeRegionComboBox").Select("Златоуст");
			Assert.That(Css($"#browseRegion{region.Id}").Checked, Is.True);

			var client = Register();
			Assert.That(client.HomeRegion, Is.EqualTo(region));
			Assert.That(client.MaskRegion, Is.EqualTo(region.Id));
		}

		[Test]
		public void Register_client_for_supplier_with_payer()
		{
			var supplier = DataMother.CreateSupplier();
			MakeNameUniq(supplier.Payer);
			MakeNameUniq(supplier);
			Refresh();

			SetupGeneralInformation();
			SearchPayer(supplier.Payer.Name);
			Assert.That(Css("#PayerComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#PayerComboBox").SelectedItem, Is.StringEnding(supplier.Payer.Name));

			Css("#ShowForOneSupplier").Click();
			SearchSupplier(supplier.Name);
			Assert.That(Css("#SupplierComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#SupplierComboBox").SelectedItem, Is.StringEnding(supplier.Name));

			Click("Зарегистрировать");

			var client = GetRegistredClient();
			Assert.That(client.Payers[0], Is.EqualTo(supplier.Payer));
			Assert.That(client.Settings.NoiseCosts, Is.True);
			Assert.That(client.Settings.NoiseCostExceptSupplier, Is.EqualTo(supplier));
		}

		[Test]
		public void Select_org()
		{
			var payer = DataMother.TestClient().Payers[0];
			MakeNameUniq(payer);
			var org = new LegalEntity(String.Format("Тестовое юр.лицо 2 {0}", payer.Id), payer);
			payer.JuridicalOrganizations.Add(org);
			Save(payer);
			Refresh();

			SetupGeneralInformation();

			SearchPayer(payer.Name);
			Assert.That(Css("#PayerComboBox").Options.Count, Is.EqualTo(1));
			Assert.That(Css("#PayerComboBox").SelectedItem, Is.StringEnding(payer.Name));
			Assert.That(Css("#address_LegalEntity_Id").Options.Count, Is.EqualTo(2));

			Css("#address_LegalEntity_Id").Select(org.Name);
			Click("Зарегистрировать");

			var client = GetRegistredClient();
			Assert.That(client.Addresses[0].LegalEntity, Is.EqualTo(org));
		}

		[Test]
		public void Register_empty()
		{
			SetupClient();
			Css("#options_FillBillingInfo").Checked = false;
			Css("#options_RegisterEmpty").Click();
			browser.Eval("$('#options_RegisterEmpty').change()");
			Click("Зарегистрировать");

			AssertText("Регистрация завершена успешно");
		}

		[Test]
		public void Check_memo_about_writing_addresses_for_register()
		{
			AssertText("Тестовый текст памятки адреса при регистрации");
		}

		private void ClickRegisterAndCheck(Browser browser)
		{
			// Снимаем галку, чтобы не показывалась карта регистрации
			Css("#options_ShowRegistrationCard").Checked = false;
			// Снимаем галку, чтобы не заполнять информацию для биллинга
			if (Css("#options_FillBillingInfo").Enabled)
				Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			AssertText("Регистрация завершена успешно");
		}

		public Client GetRegistredClient()
		{
			AssertText("Регистрационная карта");
			var id = Helper.GetClientCodeFromRegistrationCard(browser);
			return session.Load<Client>(id);
		}

		private void SetupGeneralInformation()
		{
			Css("#user_Name").TypeText("Тестовый пользователь");
			Css("#address_Value").TypeText("Тестовый адрес доставки");

			SetupClient();
		}

		private void SetupClient()
		{
			browser.TextField(Find.ById("client_FullName")).TypeText(_randomClientName);
			browser.TextField(Find.ById("client_Name")).TypeText(_randomClientName);
			// Заполняем контактную информацию для клиента
			Css("#ClientContactPhone").TypeText("123-4567890");
			Css("#ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Заполняем контактную информацию для пользователя
			Css("#userContactPhone").TypeText("123-4567890");
			Css("#userContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Если это аптека, заполняем адрес доставки
		}

		private void SearchSupplier(string text)
		{
			Css("#SearchSupplierTextPattern").TypeText(text);
			Css("#SearchSupplierButton").Click();
			Thread.Sleep(1000);
		}

		private void SearchPayer(string term)
		{
			browser.CheckBox(Find.ById("PayerExists")).Checked = true;
			browser.TextField(Find.ByName("SearchPayerTextPattern")).TypeText(term);
			browser.Button(Find.ByName("SearchPayerButton")).Click();
			Thread.Sleep(1000);
		}

		private Client Register()
		{
			SetupGeneralInformation();
			Css("#options_FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();

			var client = GetRegistredClient();
			return client;
		}
	}
}