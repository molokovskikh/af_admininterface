using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.WebHost;
using NUnit.Framework;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class RegistrationFixture
	{
		private Server _webServer;
		private string _randomClientName;

		[TestFixtureSetUp]
		public void SetupFixture()
		{
			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
		}

		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			_webServer.Stop();
		}

		[SetUp]
		public void Setup()
		{
			var random = new Random();
			_randomClientName = "test" + random.Next(100000);
		}

		private static string BuildTestUrl(string urlPart)
		{
			return String.Format("http://localhost:{0}/{1}",
								 ConfigurationManager.AppSettings["webPort"],
								 urlPart);
		}

		[Test]
		public void Register_client_if_email_or_phone_contains_space_or_tab_or_nextline_at_begin_or_end()
		{
			using (var browser = new IE(BuildTestUrl("register.aspx")))
			{
				SetUpTestClient(browser);

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
				SetUpTestClient(browser);
				browser.Button(Find.ById("Register")).Click();

				browser.ContainsText("Реистрация клиента, шаг 2: Заполнения информации о плательщике");

				browser.TextField(Find.ByName("PaymentOptions.Comment")).TypeText("Комментарий");
				browser.TextField(Find.ByName("PaymentOptions.PaymentPeriodBeginDate")).TypeText(DateTime.Now.AddDays(10).ToShortDateString());
				browser.Button(Find.ByValue("Сохранить")).Click();

				Assert.That(browser.ContainsText("Регистрационная карта №"));
			}
		}

		private void SetUpTestClient(IE browser)
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
