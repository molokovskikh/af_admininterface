using System.IO;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.DialogHandlers;

namespace Functional.Billing
{
	public class RevisionActFixture : WatinFixture2
	{
		private Payer payer;

		[SetUp]
		public void SetUp()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
		}

		[Test]
		public void Show_revision_act()
		{
			Open(payer);
			ClickLink("Акт сверки");
			AssertText("Акт сверки");
		}

		[Test]
		public void Print()
		{
			Open("RevisionActs/{0}", payer.Id);
			ClickLink("Печать");
			AssertText("взаимных расчетов по состоянию");
		}

		[Test, Ignore("Зависает")]
		public void Excel()
		{
			var file = Path.Combine(Path.GetFullPath("."), "Акт сверки.xls");
			if (File.Exists(file))
				File.Delete(file);

			Open("RevisionActs/{0}", payer.Id);
			var handler = new FileDownloadHandler(file);
			browser.AddDialogHandler(handler);
			ClickLink("Excel");

			Assert.That(File.Exists("Акт сверки.xls"), Is.True);
		}

		[Test]
		public void Send_to_email()
		{
			Open("RevisionActs/{0}", payer.Id);
			browser.TextField(Find.ByName("emails")).TypeText("kvasovtest@analit.net");
			ClickButton("Отправить");
			AssertText("Отправлено");
		}
	}
}