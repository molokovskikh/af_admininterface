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

		[Test]
		public void OpenSettingsView()
		{
			Open("main/Settings");
			Assert.That(browser.Text, Is.StringContaining("Настройки по умолчанию"));
			Assert.That(browser.Text, Is.StringContaining("Общие настройки"));
			Assert.That(browser.Text, Is.StringContaining("Настройки мини-почты"));
		}
	}
}
