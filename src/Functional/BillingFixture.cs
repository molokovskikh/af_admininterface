using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace Functional
{
	public class BillingFixture : WatinFixture
	{
		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			using (var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				Assert.That(browser.Text, Text.Contains("Фильтр плательщиков"));
				browser.RadioButton(Find.ById("SearchByBillingId")).Click();
				browser.TextField(Find.ById("SearchText")).TypeText("921");
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.Text, Text.Contains("Офис123"));
				browser.Link(Find.ByText("921")).Click();
				Assert.That(browser.Text, Text.Contains("Платильщик Офис123"));
			}
		}

		private IE Open(string uri)
		{
			return new IE(BuildTestUrl(uri));
		}
	}
}
