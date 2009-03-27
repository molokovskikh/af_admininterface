using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture
	{
		[Test]
		public void Try_to_send_email_notification()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
				Assert.That(browser.ContainsText("Конфигурация клиента"));
			}
		}

		[Test, Ignore("не корректно обрабатывается javascript на изменение текущего элемента в select")]
		public void Try_to_change_home_region_for_drugstore()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var homeRegionSelect = GetHomeRegionSelect(browser);
				var changeTo = homeRegionSelect.Options.First(o => o.Value != homeRegionSelect.SelectedOption.Value);
				homeRegionSelect.Select(changeTo.Text);
				browser.WaitForComplete(50000);
				browser.Button(b => b.Value.Equals("Применить"));
				Assert.That(browser.ContainsText("Сохранено"), Is.True);
				Assert.That(homeRegionSelect.SelectedOption.Text, Is.EqualTo(changeTo.Text));
			}
		}

		private SelectList GetHomeRegionSelect(IE browser)
		{
			return (SelectList)browser.Label(l => l.Text.Contains("Домашний регион")).NextSibling;
		}
	}
}
