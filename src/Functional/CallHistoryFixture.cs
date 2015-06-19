using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;

namespace Functional
{
	[TestFixture, Ignore("Временно до починки")]
	public class CallHistoryFixture : FunctionalFixture
	{
		[Test]
		public void TestViewCallHistory()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink("История звонков");
				browser.TextField(Find.ByName("SearchBy.BeginDate")).TypeText("01.01.2009");
				ClickButton("Найти");

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