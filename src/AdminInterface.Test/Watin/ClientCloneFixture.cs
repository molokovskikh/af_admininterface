using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class ClientCloneFixture : WatinFixture
	{
		[Test]
		public void Try_to_clone_client()
		{
			using (var browser = new IE(BuildTestUrl("CopySynonym.aspx")))
			{
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_FromTB")).TypeText("ТестерК");
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_ToTB")).TypeText("ТестерК2");
				browser.Button(Find.ByValue("Найти")).Click();
				browser.Button(Find.ByValue("Присвоить")).Click();
				Assert.That(browser.Text, Text.Contains("Присвоение успешно завершен"));
			}
		}
	}
}
