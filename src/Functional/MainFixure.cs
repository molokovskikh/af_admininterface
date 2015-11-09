using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools.Calendar;
using Common.Tools.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Chrome;
using WatiN.Core.Native.Windows;

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
				NewSupplierMailText = "Письмо новому поставщику"
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
			Save(log);

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
			Click("Всего: 0, загруженные: 0, перепроводимые: 0, Error: 0");
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
	}
}