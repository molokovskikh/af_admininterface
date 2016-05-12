using System;
using System.Linq;
using System.Threading;
using Common.Tools.Calendar;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.UtilityClasses;
using WatiN.CssSelectorExtensions;

namespace Functional.ForTesting
{
	public class FunctionalFixture : WatinFixture2
	{
		public DataMother DataMother;
		public string DataRoot;

		[SetUp]
		public void FunctionalSetup()
		{
			DataRoot = "../../../AdminInterface/Data/";
			DataMother = new DataMother(session);
		}

		protected Element SearchRoot(string title)
		{
			return browser.Element(Find.ByText(title).And(Find.ByClass("search-title"))).Parent;
		}

		protected SelectList SearchV2Root(Element root, string term)
		{
			var searchInput = root.Css(".term");
			//значит у нас есть значение и его нужно перевыбрать
			if (searchInput == null) {
				root.Css("[type=button]").Click();
				WaitForCss(".term", root);
				searchInput = root.Css(".term");
			}
			searchInput.TypeText(term);
			//иногда javascript на странице не замечает введенного текста, принудительно обновлеям
			browser.Eval("$(\".term\").change();");
			Click((IElementContainer)root, "Найти");
			new TryFuncUntilTimeOut(3.Second()) {
				SleepTime = 50.Millisecond(),
				ExceptionMessage = () => String.Format("не удалось ничего найти '{0}'", term)
			}.Try(() => root.CssSelect("select") != null);
			return (SelectList)root.CssSelect("select");
		}

		protected SelectList SearchV2(Element element, string term)
		{
			return SearchV2Root(element.Parent, term);
		}

		protected SelectList Search(string term, string title = null)
		{
			Element root = null;
			if (String.IsNullOrEmpty(title)) {
				Css(".term").TypeText(term);
				Css(".search[type=button]").Click();
			}
			else {
				root = SearchRoot(title);
				root.Css(".term").TypeText(term);
				root.Css(".search[type=button]").Click();
			}
			Thread.Sleep(1000);
			if (root != null)
				return root.Css("select");
			return null;
		}

		public void ConfirmDialog()
		{
			var buttons = browser.Buttons.Where(b => !string.IsNullOrEmpty(b.ClassName) && b.ClassName.Contains("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).ToList();
			buttons.First(b => b.InnerHtml.Contains("Продолжить")).Click();
		}
	}
}