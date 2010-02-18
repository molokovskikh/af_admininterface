using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using NUnit.Framework;

using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class RegistrationFixture : WatinFixture
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
			_registerPageUrl = "Register/Register.rails";
		}

		[Test, Ignore("Регистрация поставщиков пока не реализована")]
		public void Try_register_supplier()
		{
			/*
			uint clientId;
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser, ClientType.Supplier);
				browser.SelectList(Find.ById("TypeDD")).Select("Поставщик");
				Thread.Sleep(1000);
				browser.CheckBox(Find.ById("EnterBillingInfo")).Click();
				Assert.That(browser.CheckBox(Find.ById("EnterBillingInfo")).Checked, Is.False);

				browser.Button(Find.ById("Register")).Click();
				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
				clientId = GetClientCodeFromRegistrationCard(browser);
			}
			using (new SessionScope())
			{
				var client = Client.Find(clientId);
				Assert.That(client.ContactGroupOwner.ContactGroups.Count, Is.EqualTo(3));
				var generalContacts = client.ContactGroupOwner.ContactGroups.Where(g => g.Type == ContactGroupType.General).Single();
				Assert.That(generalContacts.Contacts.Count, Is.EqualTo(2));
				Assert.That(generalContacts.Contacts.First().Type, Is.EqualTo(ContactType.Phone));
				Assert.That(generalContacts.Contacts.Skip(1).First().Type, Is.EqualTo(ContactType.Email));
			}
			/**/
		}

		[Test]
		public void Test_drugstore_validate_required_general_info()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				browser.SelectList("clientType").Select("Аптека");
				// Заполняем адрес доставки
				browser.TextField("deliveryAddress").TypeText(_randomClientName);
				browser.Button("RegisterButton").Click();
				Assert.That(browser.Text, Text.Contains("Это поле необходимо заполнить."));
				browser.TextField("JuridicalName").TypeText(_randomClientName);
				browser.TextField("ShortName").TypeText(_randomClientName);
				browser.Button("RegisterButton").Click();
				Assert.That(browser.Text, Text.Contains("Это поле необходимо заполнить."));
				browser.TextField("ClientContactPhone").TypeText("123-456789");
				browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
				ClickRegisterAndCheck(browser);
			}
		}

		[Test]
		public void Test_validate_contact_info()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				browser.SelectList("clientType").Select("Аптека");
				browser.TextField("deliveryAddress").TypeText("Test address");
				browser.TextField("JuridicalName").TypeText(_randomClientName);
				browser.TextField("ShortName").TypeText(_randomClientName);				
				browser.TextField("ClientContactPhone").TypeText("123456789");
				browser.Button("RegisterButton").Click();
				Assert.That(browser.Text, Text.Contains("Некорректный телефонный номер"));
				browser.TextField("ClientContactEmail").TypeText(_randomClientName + "test.test");
				browser.Button("RegisterButton").Click();
				Assert.That(browser.Text, Text.Contains("Некорректный адрес электронной почты"));
				browser.TextField("ClientContactPhone").TypeText("123-456789");
				browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
				ClickRegisterAndCheck(browser);
			}
		}

		[Test]
		public void Test_select_existsing_payer()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox(Find.ById("PayerExists")).Checked = true;	
				Test_search_and_select(browser, "Payer");
			}
		}

		[Test]
		public void Test_select_existing_supplier()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;
				Assert.That(browser.CheckBox(Find.ById("PayerExists")).Enabled, Is.False);
				Test_search_and_select(browser, "Supplier");
			}			
		}

		private void Test_search_and_select(IE browser, string namePart)
		{
			var errorMessage = "Выберите поставщика";
			if (namePart.Equals("Payer"))
				errorMessage = "Выберите плательщика";

			Assert.That(browser.CheckBox(Find.ById("FillBillingInfo")).Enabled, Is.False);
			Assert.That(browser.Div(Find.ById("Search" + namePart + "Div")).Style.Display, Is.EqualTo("block"));
			Assert.That(browser.Div(Find.ById("Select" + namePart + "Div")).Style.Display, Is.EqualTo("none"));
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Text.Contains(errorMessage));

			browser.TextField(Find.ById("Search" + namePart + "TextPattern")).TypeText("12");
			browser.Button(Find.ById("Search" + namePart + "Button")).Click();
			Assert.That(browser.Text, Text.Contains("Выполняется поиск"));

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
			Assert.That(browser.Text, Text.Contains("Ничего не найдено"));
		}

		private void ClickRegisterAndCheck(IE browser)
		{
			// Снимаем галку, чтобы не показывалась карта регистрации			
			browser.CheckBox("ShowRegistrationCard").Checked = false;
			// Снимаем галку, чтобы не заполнять информацию для биллинга
			if (browser.CheckBox("FillBillingInfo").Enabled)
				browser.CheckBox("FillBillingInfo").Checked = false;
			browser.Button("RegisterButton").Click();
			Assert.That(browser.Text, Text.Contains("Регистрация завершена успешно"));			
		}

		[Test]
		public void After_drugstore_registration_should_insert_record_in_user_update_info_table()
		{
			using (new SessionScope())
			{
				var defaults = DefaultValues.Get();
				defaults.AnalitFVersion = 705;
				defaults.Update();
			}

			uint clientCode;
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.SelectList(Find.ById("clientType")).Select("Аптека");
				browser.CheckBox(Find.ById("FillBillingInfo")).Checked = false;
				browser.Button(Find.ById("RegisterButton")).Click();
				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
				clientCode = GetClientCodeFromRegistrationCard(browser);
			}
			using(new SessionScope())
			{
				var client = Client.Find(clientCode);
				Console.WriteLine(client.Id);

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
				Assert.That(passwordChange.SmtpId, Is.Not.EqualTo(0));
				Assert.That(passwordChange.SentTo, Is.EqualTo(String.Format(_randomClientName + _mailSuffix)));
				Assert.That(updateInfo, Is.Not.Null, "Не создали запись в UserUpdateInfo");
			}
		}		

		[Test]
		public void Register_client_with_payer_info()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.Button(Find.ById("RegisterButton")).Click();
				browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");
				browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
				browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
			}
		}

		[Test]
		public void After_registration_prices_avaliable()
		{
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox("FillBillingInfo").Checked = false;
				browser.Button(Find.ById("RegisterButton")).Click();
				var clientCode = GetClientCodeFromRegistrationCard(browser);

				ArHelper.WithSession(s => {
					var client = Client.Find(clientCode);
					var command = new MySqlCommand("select count(*) from future.intersection where ClientId = ?ClientCode",
						(MySqlConnection)s.Connection);
					command.Parameters.AddWithValue("?ClientCode", clientCode);
					var count = Convert.ToUInt32(command.ExecuteScalar());
					Assert.That(count, Is.GreaterThan(0));

					command.CommandText = "select count(*) from future.Users where ClientId = ?ClientCode";
                    var usersCount = Convert.ToUInt32(command.ExecuteScalar());
					Assert.That(usersCount, Is.EqualTo(1), "у клиента нет пользователей");

					command.CommandText = "select Id from future.Users where ClientId = ?ClientCode";
					var userId = Convert.ToUInt32(command.ExecuteScalar());
					command.CommandText = "select count(*) from future.UserPrices where UserId = ?UserId";
					command.Parameters.AddWithValue("?UserId", userId);
					count = Convert.ToUInt32(command.ExecuteScalar());
					Assert.That(count, Is.GreaterThan(0));
                });
			}
		}

		[Test]
		public void Try_to_register_hiden_client()
		{
			uint clientcode;
			var testSupplierId = 234;
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox(Find.ById("ShowForOneSupplier")).Checked = true;

				browser.TextField(Find.ById("SearchSupplierTextPattern")).TypeText("тестирования");
				browser.Button(Find.ById("SearchSupplierButton")).Click();
				Thread.Sleep(2000);
				browser.SelectList(Find.ById("SupplierComboBox")).Select("234. Поставщик для тестирования");
				Assert.That(browser.CheckBox(Find.ById("FillBillingInfo")).Enabled, Is.False);

				browser.Button(Find.ById("RegisterButton")).Click();
				clientcode = GetClientCodeFromRegistrationCard(browser);
			}
			With.Connection(c => {
				var command = new MySqlCommand("select FirmCodeOnly from usersettings.retclientsset where clientcode = ?clientcode", c);
				command.Parameters.AddWithValue("?clientcode", clientcode);
				var firmCodeOnly = Convert.ToUInt32(command.ExecuteScalar());
				Assert.That(firmCodeOnly, Is.EqualTo(testSupplierId));

                command.CommandText = "select BillingCode from usersettings.ClientsData where FirmCode = ?FirmCode";
				command.Parameters.AddWithValue("?FirmCode", testSupplierId);
                var billingCode = Convert.ToUInt32(command.ExecuteScalar());
				var client = Client.Find(clientcode);
				Assert.That(client.BillingInstance.PayerID, Is.EqualTo(billingCode));

				var settings = DrugstoreSettings.Find(clientcode);
				Assert.That(settings.InvisibleOnFirm, Is.EqualTo(DrugstoreType.Hidden));
				Assert.That(settings.FirmCodeOnly, Is.EqualTo(testSupplierId)); 
			});
		}

		private uint GetClientCodeFromRegistrationCard(IE browser)
		{
			return Convert.ToUInt32(new Regex(@"\d+").Match(browser.FindText(new Regex(@"Регистрационная карта №\s*\d+", RegexOptions.IgnoreCase))).Value);
		}

		private void SetupGeneralInformation(IE browser, ClientType clientType)
		{
			browser.TextField(Find.ById("JuridicalName")).TypeText(_randomClientName);
			browser.TextField(Find.ById("ShortName")).TypeText(_randomClientName);
			// Заполняем контактную информацию для клиента
			browser.TextField("ClientContactPhone").TypeText("123-456789");
			browser.TextField("ClientContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Заполняем контактную информацию для пользователя
			browser.TextField("UserContactPhone").TypeText("123-456789");
			browser.TextField("UserContactEmail").TypeText(_randomClientName + _mailSuffix);
			// Если это аптека, заполняем адрес доставки			
			if (clientType == ClientType.Drugstore)
				browser.TextField(Find.ById("deliveryAddress")).TypeText(_randomClientName);
		}
	}
}