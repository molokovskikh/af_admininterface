using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.MySql;
using MySql.Data.MySqlClient;
using NUnit.Framework;

using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class RegistrationFixture : WatinFixture
	{
		private string _randomClientName;

		[SetUp]
		public void Setup()
		{
			var random = new Random();
			_randomClientName = "test" + random.Next(100000);
		}

		[Test]
		public void Try_register_supplier()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.SelectList(Find.ById("TypeDD")).Select("Поставщик");
				Thread.Sleep(1000);
				browser.CheckBox(Find.ById("EnterBillingInfo")).Click();
				Assert.That(browser.CheckBox(Find.ById("EnterBillingInfo")).Checked, Is.False);


				browser.Button(Find.ById("Register")).Click();
				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
			}
		}

		[Test]
		public void After_drugstore_registration_should_insert_record_in_user_update_info_table()
		{
			uint clientCode;
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.SelectList(Find.ById("TypeDD")).Select("Аптека");
				browser.CheckBox(Find.ById("EnterBillingInfo")).Checked = false;

				browser.Button(Find.ById("Register")).Click();
				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
				clientCode = GetClientCodeFromRegistrationCard(browser);
			}

			using(new SessionScope())
			{
				var client = Client.Find(clientCode);
				var user = client.Users.First();
				var updateInfo = (from info in UserUpdateInfo.Queryable
				                  where info.User.Id == user.Id
				                  select info).FirstOrDefault();

				var logs = PasswordChangeLogEntity.GetByLogin(user.Login, DateTime.Today, DateTime.Today.AddDays(1));
				var passwordChange = logs.SingleOrDefault();
				Assert.That(passwordChange, Is.Not.Null);
				Assert.That(passwordChange.UserName, Is.EqualTo("kvasov"));
				Assert.That(passwordChange.TargetUserName, Is.EqualTo(user.Login));
				Assert.That(passwordChange.SmtpId, Is.Not.EqualTo(0));
				Assert.That(passwordChange.SentTo, Is.EqualTo(String.Format("{0}@mail.ru", user.Login)));

				Assert.That(updateInfo, Is.Not.Null, "Не создали запись в UserUpdateInfo");
			}
		}

		[Test]
		public void Register_client_if_email_or_phone_contains_space_or_tab_or_nextline_at_begin_or_end()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);

				browser.TextField(Find.ById("EmailTB")).TypeText(" \t  " + _randomClientName + "@mail.ru  \t  ");
				browser.TextField(Find.ById("PhoneTB")).TypeText(" \t  4732-606000  \t  \t");
				browser.TextField(Find.ById("TBOrderManagerPhone")).TypeText(" \t  4732-606000  \t  \t");
				browser.TextField(Find.ById("TBOrderManagerMail")).TypeText(" \t  " + _randomClientName + "@mail.ru  \t  ");

				browser.CheckBox(Find.ById("EnterBillingInfo")).Checked = false;
				browser.CheckBox(Find.ById("ShowRegistrationCard")).Checked = false;
				browser.TextField(Find.ById("AdditionEmailToSendRegistrationCard")).TypeText("tech@analit.net");

				browser.Button(Find.ById("Register")).Click();

				//пока не реализована, после успешной регистрации на странице клиента нужно показывать сообщение
				//Assert.That(browser.Text, Text.Contains("Регистрация завершена успешно."));
				Assert.That(browser.Text, Text.Contains("Информация о клиенте"));
			}
		}

		[Test]
		public void Redirect_to_client_info_page_if_base_client_registred()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.CheckBox(Find.ById("IncludeCB")).Click();
				browser.TextField(Find.ById("IncludeSTB")).TypeText("ТестерК");
				browser.Button(Find.ById("IncludeSB")).Click();
				
				browser.SelectList(Find.ById("IncludeType")).Select("Базовый");
				browser.Button(Find.ById("Register")).Click();

				Assert.That(browser.Text, Text.Contains("Информация о клиенте"));
			}
		}

		[Test]
		public void Try_to_register_network_client()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.CheckBox(Find.ById("IncludeCB")).Click();
				browser.TextField(Find.ById("IncludeSTB")).TypeText("ТестерК");
				browser.Button(Find.ById("IncludeSB")).Click();
				
				browser.SelectList(Find.ById("IncludeType")).Select("Сеть");
				browser.Button(Find.ById("Register")).Click();

				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
			}
		}

		[Test]
		public void Register_client_with_exists_payer()
		{
			using(var browser = new IE(BuildTestUrl("register.aspx")))
			{
				browser.CheckBox(Find.ById("PayerPresentCB")).Click();
				browser.TextField(Find.ById("PayerFTB")).TypeText("Офис");
				browser.Button(Find.ById("FindPayerB")).Click();
				Assert.That(browser.SelectList(Find.ById("PayerDDL")).SelectedItem, Is.EqualTo("921. Офис123"));
			}
		}

		[Test]
		public void Register_client_with_payer_info()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.Button(Find.ById("Register")).Click();

				browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");

				browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
				browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
				browser.Button(Find.ByValue("Сохранить")).Click();

				Assert.That(browser.Text, Text.Contains("Регистрационная карта №"));
			}
		}

		[Test]
		public void Try_to_register_hiden_client()
		{
			uint clientcode;
			using(var browser = Open("register.aspx"))
			{
				SetupGeneralInformation(browser);
				browser.SelectList(Find.ById("CustomerType")).Select("Скрытый");
				browser.TextField(Find.ById("SupplierSearchText")).TypeText("Тестирования");
				browser.Button(Find.ById("SearchSupplier")).Click();
				browser.SelectList(Find.ById("Suppliers")).Select("234. Поставщик для тестирования");
				browser.CheckBox(Find.ById("EnterBillingInfo")).Checked = false;

				browser.Button(Find.ByValue("Зарегистрировать")).Click();
				clientcode = GetClientCodeFromRegistrationCard(browser);
			}

			With.Connection(c => {
				var command = new MySqlCommand("select FirmCodeOnly from usersettings.retclientsset where clientcode = ?clientcode", c);
				command.Parameters.AddWithValue("?clientcode", clientcode);
				var firmCodeOnly = Convert.ToUInt32(command.ExecuteScalar());
				Assert.That(firmCodeOnly, Is.EqualTo(234));
			});
		}

		private uint GetClientCodeFromRegistrationCard(IE browser)
		{
			return Convert.ToUInt32(new Regex(@"\d+")
			                        	.Match(browser.FindText(new Regex(@"Регистрационная карта №\s*\d+", RegexOptions.IgnoreCase))).Value);
		}

		private void SetupGeneralInformation(IE browser)
		{
			browser.TextField(Find.ById("FullNameTB")).TypeText(_randomClientName);
			browser.TextField(Find.ById("ShortNameTB")).TypeText(_randomClientName);
			browser.TextField(Find.ById("AddressTB")).TypeText(_randomClientName);
			browser.TextField(Find.ById("LoginTB")).TypeText(_randomClientName);

			browser.TextField(Find.ById("EmailTB")).TypeText(_randomClientName + "@mail.ru");
			browser.TextField(Find.ById("PhoneTB")).TypeText("4732-606000");
		}
	}
}