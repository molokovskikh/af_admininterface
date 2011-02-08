using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class ClientCloneFixture : WatinFixture
	{
		[Test]
		public void Try_to_clone_client()
		{
			var client1 = DataMother.TestClient(c => {
				c.Name = "тестовый 1";
			});

			var client2 = DataMother.TestClient(c => {
				c.Name = "тестовый 2";
			});

			using (var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Клонирование")).Click();
				Assert.That(browser.Text, Is.StringContaining("Создание предварительного набора данных для клиента"));

				browser.SelectList("ctl00_MainContentPlaceHolder_RegionDD").Select(client1.HomeRegion.Name);
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_FromTB")).TypeText("тестовый 1");
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_ToTB")).TypeText("тестовый 2");
				browser.Button(Find.ByValue("Найти")).Click();
				browser.Button(Find.ByValue("Присвоить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Клонирование успешно завершено"));
			}
		}
	}
}