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
	}
}
