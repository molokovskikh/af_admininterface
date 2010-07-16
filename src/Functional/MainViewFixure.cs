using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;

namespace Functional
{
	public class MainViewFixure : WatinFixture
	{
		[Test]
		public void Open_main_view()
		{
			using(var browser = Open("main/index"))
			{
				Assert.That(browser.Text, Is.StringContaining("Статистика"));
			}
		}
	}
}
