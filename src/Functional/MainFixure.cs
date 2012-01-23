using System.Linq;
using AdminInterface.Models.Logs;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

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
	}
}
