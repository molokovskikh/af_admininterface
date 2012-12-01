using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;

namespace Functional.Billing
{
	public class AdFixture : WatinFixture2
	{
		private Advertising ad;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			ad = new Advertising(payer);
			session.Save(ad);
		}

		[Test]
		public void Build_invoice()
		{
			Open(ad, "Edit");
			browser.Button(Find.ByValue("Сформировать счет")).Click();
			Assert.That(browser.Text, Is.StringContaining("Счет"));
		}

		[Test]
		public void Build_act()
		{
			Open(ad, "Edit");
			browser.Button(Find.ByValue("Сформировать акт")).Click();
			Assert.That(browser.Text, Is.StringContaining("Акт"));
		}
	}
}