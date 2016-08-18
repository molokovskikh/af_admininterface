using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Tools.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Functional
{
	public class MainFixure : AdmSeleniumFixture
	{
		[Test]
		public void Open_main_view()
		{
			Open();
			AssertText("Статистика");
			session.CreateSQLQuery("delete from usersettings.defaults;").ExecuteUpdate();
			var sender = session.Get<OrderHandler>(1u);
			var formatter = session.Get<OrderHandler>(12u);
			var defaults = new DefaultValues {
				EmailFooter = "С уважением",
				Phones = "Москва +7 499 7097350",
				AllowedMiniMailExtensions = "doc, xls, gif, tiff, tif, jpg, pdf, txt, docx, xlsx, csv",
				ResponseSubjectMiniMailOnUnknownProvider = "Ваше Сообщение не доставлено одной или нескольким аптекам",
				ResponseBodyMiniMailOnUnknownProvider = "Добрый день.",
				ResponseSubjectMiniMailOnEmptyRecipients = "Ваше Сообщение не доставлено одной или нескольким аптекам",
				ResponseBodyMiniMailOnEmptyRecipients = "Добрый день.",
				ResponseSubjectMiniMailOnMaxAttachment = "Ваше Сообщение не доставлено одной или нескольким аптекам",
				ResponseBodyMiniMailOnMaxAttachment = "Добрый день.",
				ResponseSubjectMiniMailOnAllowedExtensions = "Ваше Сообщение не доставлено одной или нескольким аптекам",
				ResponseBodyMiniMailOnAllowedExtensions = "Добрый день.",
				AddressesHelpText = "Шаблон написания адреса",
				TechOperatingModeTemplate = "<p>будни: с {0} до {1}</p>",
				TechOperatingModeBegin = "7.00",
				TechOperatingModeEnd = "19.00",
				Sender = sender,
				Formater = formatter,
				AnalitFVersion = 999u,
				ProcessingAboutFirmBody = "temp",
				ProcessingAboutFirmSubject = "temp",
				ProcessingAboutNamesBody = "temp",
				ProcessingAboutNamesSubject = "temp",
				DeletingMiniMailText = "temp",
				NewSupplierMailSubject = "Аналитфармация",
				NewSupplierMailText = "Письмо новому поставщику",
				PromotionModerationEscapeBody = "temp",
				PromotionModerationAllowedSubject = "temp",
				PromotionModerationDeniedBody = "temp",
				PromotionModerationAllowedBody = "temp",
				PromotionModerationDeniedSubject = "temp",
				PromotionModerationEscapeSubject = "temp"
			};
			session.Save(defaults);
		}

		[Test]
		public void OpenSettingsView()
		{
			Open("main/Settings");
			AssertText("Настройки по умолчанию");
			AssertText("Общие настройки");
			AssertText("Настройки мини-почты");
		}

		[Test(Description = "Проверяет корректность сохранения режима работы техподдержки")]
		public void TechOperatingModeSettingsTest()
		{
			Open("main/Settings");
			AssertText("Настройки по умолчанию");
			Click("Режим работы техподдержки");
			Css("#defaults_TechOperatingModeBegin").Clear();
			Css("#defaults_TechOperatingModeBegin").SendKeys("70.30");
			Css("#defaults_TechOperatingModeEnd").Clear();
			Css("#defaults_TechOperatingModeEnd").SendKeys("19.80");
			Click("Сохранить");
			AssertText("Некорректное время начала рабочего дня");
			AssertText("Некорректное время окончания рабочего дня");
			Css("#defaults_TechOperatingModeBegin").Clear();
			Css("#defaults_TechOperatingModeBegin").SendKeys("8.00");
			Css("#defaults_TechOperatingModeEnd").Clear();
			Css("#defaults_TechOperatingModeEnd").SendKeys("20.30");
			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void Pricessing_settings_test()
		{
			Open("main/Settings");
			AssertText("Настройки по умолчанию");
			Click("Уведомления Обработки");
			Css("#Defaults_ProcessingAboutFirmSubject").SendKeys("testFirmSubject_interface");
			Css("#Defaults_ProcessingAboutFirmBody").SendKeys("testFirmBody_interface");
			Css("#Defaults_ProcessingAboutNamesSubject").SendKeys("testNameSubject_interface");
			Css("#Defaults_ProcessingAboutNamesBody").SendKeys("testNameBody_interface");
			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void Stat()
		{
			Open();
			Click("Статистика");
			AssertText("Фильтр статистики");
		}

		[Test]
		public void Show_update_in_process()
		{
			var client = DataMother.CreateTestClientWithUser();
			var log = new PrgDataLog(client.Users.First(), "FileHandler");
			session.Save(log);

			Open();
			Css("#updates-monitoring").Click();
			WaitHelper.WaitOrFail(10.Second(), () => GlobalDriver.WindowHandles.Count > 1, "Не удалось дождаться окна");

			var handle = GlobalDriver.WindowHandles.First(x => x != GlobalDriver.CurrentWindowHandle);
			GlobalDriver.SwitchTo().Window(handle);
			AssertText("Обновляющиеся клиенты");
			AssertText("FileHandler");
		}

		[Test]
		public void Show_monitoring_priceList_page()
		{
			Open();
			WaitAjax();
			Click("Всего: 0, загруженные: 0, перепроводимые: 0, ошибок: 0");
			AssertText("Очередь обработки прайс листов");
			AssertText("Загруженные");
			AssertText("Перепроводимые");
			AssertText("Домашний регион");
			AssertText("test2");
			AssertText("AAA");
			AssertText("789");
		}

		[Test]
		public void AF_Net_Updates()
		{
			var client = DataMother.CreateTestClientWithUser();
			var log = new RequestLog(client.Users.First()) {
				Version = "1.11",
				IsCompleted = true,
				UpdateType = "MainController",
				ErrorType = 1
			};
			session.Save(log);
			Open();
			AssertText("AnalitF.Net:");
			Css("#af-net-ban").Click();
			AssertText("История обновлений AnalitF.net");
		}

		[Test]
		public void Edit_tokens()
		{
			session.DeleteEach(session.Query<FederalSupplierToken>());
			session.Save(new FederalSupplierToken(Guid.NewGuid().ToString()));
			var newToken = Guid.NewGuid().ToString();

			Open();
			Click("Настройки");
			AssertText("Признаки федеральности");
			Click("Признаки федеральности");
			AssertText("Признаки федеральности");

			Click("Удалить");
			Click("Создать?");
			Css("input[name=\"items[0].Name\"]").SendKeys(newToken);
			Click("Сохранить");
			AssertText("Сохранено");

			var tokens = session.Query<FederalSupplierToken>().ToArray();
			Assert.AreEqual(newToken, tokens.Implode(x => x.Name));
		}

		[Test]
		public void CheckFormalizeTimeTest()
		{
			var nowDate = DateTime.Now;
			CreateDownloadPrice(nowDate);
			CreateFormalizePrice(nowDate.AddMinutes(16));

			Open();
			var lastdownElem = browser.FindElement(By.XPath(ElemTempl("lastdown-status", "service-stoped")));
			var lastformElem = browser.FindElement(By.XPath(ElemTempl("lastform-status", "service-stoped")));
			Assert.IsNotNull(lastdownElem);
			Assert.IsNotNull(lastformElem);
			Assert.AreEqual(lastdownElem.Text, nowDate.ToLongTimeString());
			Assert.AreEqual(lastformElem.Text, nowDate.AddMinutes(16).ToLongTimeString());

			CreateDownloadPrice(nowDate.AddDays(-3));
			CreateFormalizePrice(nowDate.AddDays(-3).AddMinutes(15));

			Open(String.Format("?from={0}&to={1}", nowDate.AddDays(-3).ToShortDateString(), nowDate.AddDays(-3).ToShortDateString()));
			lastdownElem = browser.FindElement(By.XPath(ElemTempl("lastdown-status")));
			lastformElem = browser.FindElement(By.XPath(ElemTempl("lastform-status")));
			Assert.IsNotNull(lastdownElem);
			Assert.IsNotNull(lastformElem);
			Assert.AreEqual(lastdownElem.Text, nowDate.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.AreEqual(lastformElem.Text, nowDate.AddDays(-3).AddMinutes(15).ToString("yyyy-MM-dd HH:mm:ss"));
		}

		private string ElemTempl(string id, string alarmCss = "")
		{
			return String.Format("//td[contains(@class, 'statistic-value {0}') and contains(@id, '{1}')]", alarmCss, id);
		}

		private void CreateDownloadPrice(DateTime date)
		{
			session.CreateSQLQuery("insert into logs.downlogs(LogTime, ResultCode)" +
				String.Format(" values('{0:yyyy-MM-dd HH:mm:ss}', 2)", date))
				.ExecuteUpdate();
		}

		private void CreateFormalizePrice(DateTime date)
		{
			session.CreateSQLQuery("insert into logs.formlogs(LogTime, ResultId)" +
				String.Format(" values('{0:yyyy-MM-dd HH:mm:ss}', 2)", date))
				.ExecuteUpdate();
		}
	}
}