using Functional.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;

namespace Functional
{
	[TestFixture]
	public class MessagesFixture : WatinFixture2
	{
		[Test]
		public void Try_to_view_statistics()
		{
			Open();
			ClickLink("История обращений");
			AssertText("История обращений");
		}

		[Test]
		public void Try_to_search_comment()
		{
			Open("messages");
			AssertText("История обращений");
			Css("#filter_SearchText").TypeText("тест");
			ClickButton("Показать");
			AssertText("История обращений");
		}
	}
}