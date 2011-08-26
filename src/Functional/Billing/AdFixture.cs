using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

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
			ad.Save();
		}

		[Test]
		public void Build_invoice()
		{
			Open(ad, "Edit");
			browser.Button(Find.ByValue("—формировать счет")).Click();
			Assert.That(browser.Text, Is.StringContaining("—чет"));
		}

		[Test]
		public void Build_act()
		{
			Open(ad, "Edit");
			browser.Button(Find.ByValue("—формировать акт")).Click();
			Assert.That(browser.Text, Is.StringContaining("јкт"));
		}
	}
}