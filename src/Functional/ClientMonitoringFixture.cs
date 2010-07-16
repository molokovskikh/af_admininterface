using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class ClientMonitoringFixture : WatinFixture
	{
		[Test, Ignore("Мониторинг сломан")]
		public void Monitor_client_updates()
		{
			using(var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Мониторинг работы клиентов")).Click();
				Assert.That(browser.Text, Is.StringContaining("Мониторинг работы клиентов"));
				Assert.That(browser.SelectList(Find.ByName("filter")).SelectedOption.Text, Is.EqualTo("Список необновляющихся копий"));
			}
		}

		[Test, Ignore("Мониторинг сломан")]
		public void Monitor_client_orders()
		{
			using(var browser = Open("monitoring/clients"))
			{
				Assert.That(browser.Text, Is.StringContaining("Мониторинг работы клиентов"));
				browser.SelectList(Find.ByName("filter")).Select("Список не заказывающихся аптек");
				browser.Button(Find.ByValue("Показать")).Click();
				Assert.That(browser.SelectList(Find.ByName("filter")).SelectedOption.Text, Is.EqualTo("Список не заказывающихся аптек"));
			}
		}
	}
}
