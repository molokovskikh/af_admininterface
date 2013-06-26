using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using NHibernate.Linq;
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

			ClickLink("Регионы");
			AssertText("Регионы");
			AssertText("Телефон по умолчанию");
			AssertText("Регионы работы по умолчанию");
			AssertText("Стоимость копии для поставщика");
			AssertText("Регион для справки");

			ClickLink("Воронеж");
			AssertText("Телефон по умолчанию");
			AssertText("Временной сдвиг относительно Москвы");
			browser.Link(Find.ById("ShowRegionsLink")).Click();
			AssertText(@"Регионы работы
 по умолчанию");
			browser.TextField(Find.ById("region_DefaultPhone")).Value = "123-1233210";
			ClickButton("Сохранить");
			AssertText("Сохранено");
			ClickLink("Регионы");
			AssertText("123-1233210");
		}

		[Test]
		public void SaveMarkupTest()
		{
			var markup = new Markup() {
				Begin = 150,
				End = 170,
				RegionId = 1,
				Type = 0,
				Value = 111
			};
			session.Save(markup);
			session.Flush();
			Open("Main/Index");

			ClickLink("Регионы");
			AssertText("Регионы");
			Click("Воронеж");
			var field = browser.TextField(Find.ByValue("111,00"));
			var id = field.Id;
			field.Value = "112";
			Click("Сохранить");
			AssertText("Сохранено");
			session.Clear();
			field = browser.TextField(id);
			var savedMarkup = session.Query<Markup>().FirstOrDefault(m => m.Id == markup.Id);
			Assert.That(savedMarkup.Value, Is.EqualTo(112));
			Assert.That(field.Value, Is.EqualTo("112,00"));
		}
	}
}
