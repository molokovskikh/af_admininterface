using System;
using System.Threading;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional.ForTesting
{
	public class FunctionalFixture : WatinFixture2
	{
		protected DataMother dataMother;

		[SetUp]
		public void Setup()
		{
			dataMother = new DataMother(session);
		}

		protected Element SearchRoot(string title)
		{
			return browser.Element(Find.ByText(title).And(Find.ByClass("search-title"))).Parent;
		}

		protected void Search(string term, string title = null)
		{
			if (String.IsNullOrEmpty(title)) {
				Css(".term").TypeText(term);
				Css(".search[type=button]").Click();
			}
			else {
				var searchRoot = SearchRoot(title);
				searchRoot.Css(".term").TypeText(term);
				searchRoot.Css(".search[type=button]").Click();
			}
			Thread.Sleep(1000);
		}
	}
}