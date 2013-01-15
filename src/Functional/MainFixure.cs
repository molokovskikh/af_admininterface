using System.Linq;
using AdminInterface.Models.Logs;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional
{
	public class MainFixure : WatinFixture2
	{
		[Test]
		public void Open_main_view()
		{
			Open();
			Assert.That(browser.Text, Is.StringContaining("Статистика"));
		}

		[Test]
		public void OpenSettingsView()
		{
			Open("main/Settings");
			Assert.That(browser.Text, Is.StringContaining("Настройки по умолчанию"));
			Assert.That(browser.Text, Is.StringContaining("Общие настройки"));
			Assert.That(browser.Text, Is.StringContaining("Настройки мини-почты"));
		}

		[Test(Description = "Проверяет корректность сохранения режима работы техподдержки")]
		public void TechOperatingModeSettingsTest()
		{
			Open("main/Settings");
			AssertText("Настройки по умолчанию");
			Click("Режим работы техподдержки");
			browser.ShowWindow(NativeMethods.WindowShowStyle.Maximize);
			browser.TextField("defaults_TechOperatingModeBegin").Value = "70.30";
			browser.TextField("defaults_TechOperatingModeEnd").Value = "19.80";
			Click("Сохранить");
			AssertText("Некорректное время начала рабочего дня");
			AssertText("Некорректное время окончания рабочего дня");
			browser.TextField("defaults_TechOperatingModeBegin").Value = "8.00";
			browser.TextField("defaults_TechOperatingModeEnd").Value = "20.30";
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
			var link = browser.Links.First(l => l.Url.EndsWith("Monitoring/Updates"));
			link.Click();

			OpenedWindow("Обновляющиеся клиенты");
			AssertText("Обновляющиеся клиенты");
			AssertText("FileHandler");
		}

		[Test]
		public void Show_monitoring_priceList_page()
		{
			Open();
			Click("0(0/0)");
			AssertText("Очередь обработки прайс листов");
			AssertText("Загруженные");
			AssertText("Перепроведенные");
			AssertText("AAA");
			AssertText("789");
		}
	}
}