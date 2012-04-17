using Functional.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core; using Test.Support.Web;

namespace Functional
{
	[TestFixture]
	public class MessagesFixture : WatinFixture2
	{
		[Test]
		public void Try_to_view_statistics()
		{
			Open();
			browser.Link(Find.ByText("История обращений")).Click();
			Assert.That(browser.Text, Is.StringContaining("История обращений"));
		}

		[Test]
		public void Try_to_search_comment()
		{
			Open("messages");
			Assert.That(browser.Text, Is.StringContaining("История обращений"));
			Css("#filter_SearchText").TypeText("тест");
			browser.Button(Find.ByValue("Показать")).Click();
			Assert.That(browser.Text, Is.StringContaining("История обращений"));
		}
	}
}