using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture, Ignore("Временно до починки")]
	public class CallHistoryFixture : WatinFixture
	{
		[Test]
		public void TestViewCallHistory()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{
				browser.Link(Find.ByText("История звонков")).Click();
				browser.TextField(Find.ByName("SearchBy.BeginDate")).TypeText("01.01.2009");
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.ContainsText("Дата звонка"));
				Assert.That(browser.ContainsText("Куда звонил"));
				Assert.That(browser.ContainsText("Кому звонил"));
				Assert.That(browser.ContainsText("Тип звонка"));
				Assert.That(browser.ContainsText("Номер звонившего"));
				Assert.That(browser.ContainsText("Имя звонившего"));
			}
		}
	}
}
