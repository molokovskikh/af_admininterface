using System;
using System.Text.RegularExpressions;
using AdminInterface.Helpers;
using AdminInterface.Test.ForTesting;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
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
				browser.CheckBox(Find.ById("EnterBillingInfo")).Checked = false;

				browser.Button(Find.ById("Register")).Click();
				CheckForError(browser);
				Assert.That(browser.ContainsText("Регистрационная карта №"), browser.Text);
			}
		}

		[Test]
		public void After_drugstore_registration_should_insert_record_in_user_update_info_table()
		{
			string clientCode;
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.SelectList(Find.ById("TypeDD")).Select("Аптека");
				browser.CheckBox(Find.ById("EnterBillingInfo")).Checked = false;

				browser.Button(Find.ById("Register")).Click();
				Assert.That(browser.ContainsText("Регистрационная карта №"));
				clientCode = new Regex(@"\d+")
					.Match(browser.FindText(new Regex(@"Регистрационная карта №\s*\d+", RegexOptions.IgnoreCase))).Value;
				Console.WriteLine(clientCode);
			}
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var command = new MySqlCommand(@"
select UserId 
from usersettings.OsUserAccessRight ouar
	join usersettings.UserUpdateInfo uui on uui.UserId = ouar.RowId
where clientcode = ?ClientCode", connection);
				command.Parameters.AddWithValue("?ClientCode", clientCode);
				var userId = command.ExecuteScalar();
				Assert.That(userId, Is.Not.EqualTo(""));
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

				Assert.IsTrue(browser.ContainsText("Регистрация завершена успешно."), "не переадресовали");
			}
		}

		[Test]
		public void EditPayerInfo()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetupGeneralInformation(browser);
				browser.Button(Find.ById("Register")).Click();

				browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");

				browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
				browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
				browser.Button(Find.ByValue("Сохранить")).Click();

				Assert.That(browser.ContainsText("Регистрационная карта №"));
			}
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
