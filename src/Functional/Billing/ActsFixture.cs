using System;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class ActsFixture : WatinFixture2
	{
		private Act act;

		[SetUp]
		public void Setup()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, DateTime.Now);
			invoice.Save();
			act = new Act(DateTime.Now, invoice);
			act.Save();
		}

		[Test]
		public void View_acts()
		{
			Open();
			Assert.That(browser.Text, Is.StringContaining("Административный интерфейс"));
			browser.Link(Find.ByText("Акты")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сформировать акты"));
		}

		[Test]
		public void View_act_print_form()
		{
			Open("/acts/");
			browser.Link(Find.ByText(String.Format("{0}", act.Id))).Click();
			Assert.That(browser.Text, Is.StringContaining("Акт сдачи-приемки"));
		}

		[Test]
		public void Edit_act()
		{
			Open("/acts/");
			browser.Link(Find.ByText("Редактировать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Редактирование акта"));

			Open(act, "Edit");
			var newActDate = DateTime.Today.AddDays(10);
			var date = browser.Css("input[name='act.ActDate']");
			date.Clear();
			date.TypeText(newActDate.ToString("dd.MM.yyyy"));
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			act.Refresh();
			Assert.That(act.ActDate, Is.EqualTo(newActDate));
		}
	}
}