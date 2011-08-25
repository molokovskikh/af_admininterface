using System.IO;
using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using WatiN.Core.DialogHandlers;

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
			payer = DataMother.CreatePayerForBillingDocumentTest();
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

		[Test, Ignore("Зависает")]
		public void Excel()
		{
			var file = Path.Combine(Path.GetFullPath("."), "Акт сверки.xls");
			if (File.Exists(file))
				File.Delete(file);

			browser = Open("RevisionActs/{0}", payer.Id);
			var handler = new FileDownloadHandler(file);
			browser.AddDialogHandler(handler);
			browser.Link(Find.ByText("Excel")).Click();

			Assert.That(File.Exists("Акт сверки.xls"), Is.True);
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