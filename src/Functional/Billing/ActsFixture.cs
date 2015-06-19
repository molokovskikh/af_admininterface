using System;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;

namespace Functional.Billing
{
	[TestFixture]
	public class ActsFixture : FunctionalFixture
	{
		private Act act;

		[SetUp]
		public void Setup()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, DateTime.Now);
			invoice.Parts.Add(new InvoicePart(invoice, "Мониторинг оптового фармрынка", 500, 1, DateTime.Now));
			session.Save(invoice);
			act = new Act(DateTime.Now, invoice);
			session.Save(act);
		}

		[Test]
		public void View_acts()
		{
			Open();
			AssertText("Административный интерфейс");
			ClickLink("Акты");
			AssertText("Сформировать акты");
		}

		[Test]
		public void View_act_print_form()
		{
			Open("/acts/");
			ClickLink(String.Format("{0}", act.Id));
			AssertText("Акт сдачи-приемки");
		}

		[Test]
		public void Show_print_form()
		{
			Open("/acts/");
			AssertText("Акты");
			Click("Для печати");
			AssertText("Реестр актов");
		}

		[Test]
		public void Edit_act()
		{
			Open("/acts/");
			ClickLink("Редактировать");
			AssertText("Редактирование акта");

			Open(act, "Edit");
			var newActDate = DateTime.Today.AddDays(10);
			var date = Css("input[name='act.Date']");
			date.Clear();
			date.TypeText(newActDate.ToString("dd.MM.yyyy"));
			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(act);
			Assert.That(act.Date, Is.EqualTo(newActDate));
		}
	}
}