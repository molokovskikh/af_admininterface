using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	public class RevisionActFixture : WatinFixture
	{
		public RevisionActFixture()
		{
			UseTestScope = true;
		}

		private Payer payer;

		[SetUp]
		public void SetUp()
		{
			payer = DataMother.BuildPayerForBillingDocumentTest();
		}

		[Test]
		public void Show_revision_act()
		{
			browser = Open(payer);
			browser.Link(Find.ByText("Акт сверки")).Click();
			Assert.That(browser.Text, Is.StringContaining("Акт сверки"));
		}

		[Test]
		public void Print()
		{
			browser = Open("RevisionActs/{0}", payer.Id);
			browser.Link(Find.ByText("Печать")).Click();
			Assert.That(browser.Text, Is.StringContaining("взаимных расчетов по состоянию"));
		}

		[Test]
		public void Excel()
		{
			browser = Open("RevisionActs/{0}", payer.Id);
			browser.Link(Find.ByText("Excel")).Click();
		}

		[Test]
		public void Send_to_email()
		{
			browser = Open("RevisionActs/{0}", payer.Id);
			browser.TextField(Find.ByName("emails")).TypeText("kvasovtest@analit.net");
			browser.Button(Find.ByValue("Отправить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Отправлено"));
		}
	}
}