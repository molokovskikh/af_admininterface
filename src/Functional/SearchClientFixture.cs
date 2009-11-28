using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class SearchClientFixture : WatinFixture
	{
		[Test]
		public void Try_to_seach_by_user_name()
		{
			using(var browser = Open("searchc.aspx"))
			{
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_FindTB")).TypeText("kvasov");
				browser.Button(Find.ByValue("Найти")).Click();
				Assert.That(browser.Text, Is.StringContaining("kvasov"));
			}
		}

		[Test]
		public void Search_by_payer_id()
		{
			using(var browser = Open("searchc.aspx"))
			{
				Assert.That(browser.Text, Is.StringContaining("Статистика работы клиента"));
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_FindTB")).TypeText("921");
				browser.RadioButton("ctl00_MainContentPlaceHolder_FindRB_3").Click();
				browser.Button(Find.ByValue("Найти")).Click();
				Assert.That(browser.Text, Is.StringContaining("Статистика работы клиента"));
			}
		}
	}
}
