using Functional.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;

namespace Functional
{
	[TestFixture]
	public class MessagesFixture : FunctionalFixture
	{
		[Test]
		public void Try_to_view_statistics()
		{
			Open();
			ClickLink("История обращений");
			WaitForText("История обращений");
		}

		[Test]
		public void Try_to_search_comment()
		{
			Open("messages");
			AssertText("История обращений");
			Css("#filter_SearchText").TypeText("тест");
			ClickButton("Показать");
			WaitForText("История обращений");
		}
	}
}