using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class ManagerReportsFixture : WatinFixture2
	{
		[Test]
		public void BaseShowTest()
		{
			Open();
			browser.Link(Find.ByText("Отчеты менеджеров")).Click();
			Assert.That(browser.Text, Is.StringContaining("Отчеты для менеджеров"));
			browser.Link(Find.ByText("Пользователи и адреса")).Click();
			Assert.That(browser.Text, Is.StringContaining("Зарегистрированные пользователи и адреса в регионе"));
			browser.Button(Find.ByText("Показать"));
			Assert.That(browser.Text, Is.StringContaining("Зарегистрированные пользователи и адреса в регионе"));
			browser.RadioButton(Find.ByValue("Adresses")).Click();
			browser.Button(Find.ByText("Показать"));
			Assert.That(browser.Text, Is.StringContaining("Зарегистрированные пользователи и адреса в регионе"));
		}
	}
}
