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
using Functional.ForTesting;

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
				clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
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
				Assert.That(passwordChange.SentTo.Contains(String.Format(_randomClientName + _mailSuffix)), Is.True);
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
				var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);

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
				clientcode = Helper.GetClientCodeFromRegistrationCard(browser);
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

		[Test]
		public void Try_to_register_with_existing_payer()
		{
			uint clientcode;
			var testPayerId = 921;
			using (var browser = new IE(BuildTestUrl(_registerPageUrl)))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox(Find.ById("PayerExists")).Checked = true;

				browser.TextField(Find.ById("SearchPayerTextPattern")).TypeText("офис");
				browser.Button(Find.ById("SearchPayerButton")).Click();
				Thread.Sleep(2000);
				browser.SelectList(Find.ById("PayerComboBox")).Select("921. Офис123");
				Assert.That(browser.CheckBox(Find.ById("FillBillingInfo")).Enabled, Is.False);

				browser.Button(Find.ById("RegisterButton")).Click();
				clientcode = Helper.GetClientCodeFromRegistrationCard(browser);

				var client = Client.Find(clientcode);
				Assert.That(client.BillingInstance.PayerID, Is.EqualTo(testPayerId));

				var settings = DrugstoreSettings.Find(clientcode);
				Assert.That(settings.InvisibleOnFirm, Is.EqualTo(DrugstoreType.Standart));
				Assert.That(settings.FirmCodeOnly, Is.Null);
			}
		}

		[Test(Description = "Тест для проверки состояния галок 'Получать накладные' и 'Получать отказы' при регистрации нового пользователя")]
		public void Check_flags_by_adding_user()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				browser.Link(Find.ByText("Новый пользователь")).Click();
				browser.Button(Find.ByValue("Создать")).Click();
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					Assert.That(client.Users.Count, Is.GreaterThan(0));
					browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
					browser.Refresh();
					var userLink = browser.Link(Find.ByText(client.Users[0].Login));
					Assert.IsTrue(userLink.Exists);
					userLink.Click();
					browser.Link(Find.ByText("Настройка")).Click();
					Assert.IsTrue(browser.CheckBox(Find.ByName("user.SendWaybills")).Checked);
					Assert.IsTrue(browser.CheckBox(Find.ByName("user.SendRejects")).Checked);
				}
			}
		}

		[Test]
		public void Check_user_regions_after_client_registration()
		{
			using (var browser = Open("Register/Register.rails"))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
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
				using (var scope = new SessionScope())
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
		}

		[Test]
		public void Register_client_with_user_contact_person_info()
		{
			using (var browser = Open("Register/Register.rails"))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.TextField(Find.ByName("contactPerson")).TypeText("Alice");
				browser.CheckBox("FillBillingInfo").Checked = false;
				browser.Button("RegisterButton").Click();
				var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
				using (new SessionScope())
				{
					var client = Client.Find(clientCode);
					var user = client.Users[0];
					browser.GoTo(BuildTestUrl(String.Format("users/{0}/edit", user.Login)));
					Assert.That(browser.TextField(Find.ByName("persons[0].Name")).Text, Is.EqualTo("Alice"));
				}
			}
		}

		[Test]
		public void Register_with_flag_ignore_new_prices()
		{
			using (var browser = Open("Register/Register.rails"))
			{
				SetupGeneralInformation(browser, ClientType.Drugstore);
				browser.CheckBox("FillBillingInfo").Checked = false;
				Assert.IsFalse(browser.CheckBox(Find.ById("ignoreNewPrices")).Checked);
				browser.CheckBox(Find.ById("ignoreNewPrices")).Checked = true;
				browser.Button("RegisterButton").Click();
				var clientCode = Helper.GetClientCodeFromRegistrationCard(browser);
				var settings = DrugstoreSettings.Find(clientCode);
				Assert.IsTrue(settings.IgnoreNewPrices);
			}
		}
	}
}