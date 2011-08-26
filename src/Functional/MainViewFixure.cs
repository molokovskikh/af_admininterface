using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;

namespace Functional
{
	public class MainViewFixure : WatinFixture2
	{
		[Test]
		public void Open_main_view()
		{
			Open("main/index");
			Assert.That(browser.Text, Is.StringContaining("Статистика"));
		}
	}
}
