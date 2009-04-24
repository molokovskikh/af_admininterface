using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class StatContFixture : WatinFixture
	{
		[Test]
		public void Try_to_view_statistics()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{
				browser.Link(Find.ByText("Статистика обращений ")).Click();
				Assert.That(browser.Text, Text.Contains("Выберите период или введите текст для поиска: "));
			}
		}

		[Test]
		public void Try_to_search_comment()
		{
			using (var browser = new IE(BuildTestUrl("statcont.aspx")))
			{
				browser.TextField(Find.ByName("SearchText")).TypeText("тест");
				browser.Button(Find.ByValue("Показать")).Click();
				Assert.That(browser.Text, Text.Contains("Выберите период или введите текст для поиска: "));
			}			
		}
	}
}
