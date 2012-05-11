using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class LogsFixture : WatinFixture2
	{
		[Test]
		public void ShowTest()
		{
			Open("Main/Stat");
			var link = browser.Elements.FirstOrDefault(e => e.Parent != null && e.Parent.Id == "StatisticsTD");
			link.Click();
			AssertText("Статистика по сертификатам");
		}
	}
}
