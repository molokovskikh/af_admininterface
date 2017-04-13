using System;
using System.Linq;
using AdminInterface.Models;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Functional
{
	[TestFixture]
	public class RegionsFixture : AdmSeleniumFixture
	{
		[Test]
		public void RegionListTest()
		{
			Open("Main/Index");

			ClickLink("Регионы");
			WaitForText("Регионы");
			AssertText("Телефон по умолчанию");
			AssertText("Регионы работы по умолчанию");
			AssertText("Стоимость копии для поставщика");
			AssertText("Регион для справки");

			ClickLink("Воронеж");
			WaitForText("Телефон по умолчанию");
			AssertText("Временной сдвиг относительно Москвы");
			Css("#ShowRegionsLink").Click();
			WaitForText("Показываемые Регионы");
			Css("#region_DefaultPhone").Clear();
			Css("#region_DefaultPhone").SendKeys("123-1233210");
			ClickButton("Сохранить");
			WaitForText("Сохранено");
			ClickLink("Регионы");
			WaitForText("123-1233210");
		}

		[Test]
		public void SaveMarkupTest()
		{
			//Хрень, а не тест - при многократном запуске перестанет работать
			var markup = new Markup() {
				Begin = 150,
				End = 170,
				RegionId = 1,
				Type = 0,
				Value = 111
			};
			session.Save(markup);

			Open("Main/Index");
			ClickLink("Регионы");
			AssertText("Регионы");
			Click("Воронеж");
			var field = Css("input[value='111,00']") as IWebElement;
			var id = field.GetAttribute("id");
			field.Clear();
			field.SendKeys("112");
			Click("Сохранить");
			AssertText("Сохранено");
			session.Clear();
			field = Css("#" + id);
			var savedMarkup = session.Query<Markup>().FirstOrDefault(m => m.Id == markup.Id);
			Assert.That(savedMarkup.Value, Is.EqualTo(112));
			Assert.That(field.GetAttribute("value"), Is.EqualTo("112,00"));
			Assert.That(savedMarkup.Begin, Is.EqualTo(150));
			Assert.That(savedMarkup.End, Is.EqualTo(170));
		}
	}
}
