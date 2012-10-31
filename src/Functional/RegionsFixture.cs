using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class RegionsFixture : WatinFixture2
	{
		[Test]
		public void RegionListTest()
		{
			Open("Main/Index");

			browser.Link(Find.ByText("Регионы")).Click();
			AssertText("Регионы");
			AssertText("Телефон по умолчанию");
			AssertText("Регионы работы по умолчанию");
			AssertText("Стоимость копии для поставщика");
			AssertText("Регион для справки");

			browser.Link(Find.ByText("Воронеж")).Click();
			AssertText("Телефон по умолчанию");
			AssertText("Временной сдвиг относительно Москвы");
			browser.Link(Find.ById("ShowRegionsLink")).Click();
			AssertText(@"Регионы работы
 по умолчанию");
			browser.TextField(Find.ById("region_DefaultPhone")).Value = "123-1233210";
			browser.Button(Find.ByValue("Сохранить")).Click();
			AssertText("Сохранено");
			browser.Link(Find.ByText("Регионы")).Click();
			AssertText("123-1233210");
		}
	}
}
