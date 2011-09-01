using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	public class AdFixture : WatinFixture
	{
		private Advertising ad;
		private Payer payer;

		public AdFixture()
		{
			UseTestScope = true;
		}

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
			browser = Open(ad, "Edit");
			browser.Button(Find.ByValue("������������ ����")).Click();
			Assert.That(browser.Text, Is.StringContaining("����"));
		}

		[Test]
		public void Build_act()
		{
			browser = Open(ad, "Edit");
			browser.Button(Find.ByValue("������������ ���")).Click();
			Assert.That(browser.Text, Is.StringContaining("���"));
		}
	}
}