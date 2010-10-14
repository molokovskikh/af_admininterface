using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;

using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class StatContFixture : WatinFixture
	{
		[Test]
		public void Try_to_view_statistics()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{
				browser.Link(Find.ByText("Статистика обращений")).Click();
				Assert.That(browser.Text, Is.StringContaining("Выберите период или введите текст для поиска: "));
			}
		}

		[Test]
		public void Try_to_search_comment()
		{
			using (var browser = new IE(BuildTestUrl("statcont.aspx")))
			{
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_SearchText")).TypeText("тест");
				browser.Button(Find.ByValue("Показать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Выберите период или введите текст для поиска: "));
			}
		}
	}
}