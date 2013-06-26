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
	public class ActsFixture : WatinFixture2
	{
		private Act act;

		[SetUp]
		public void Setup()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, DateTime.Now);
			session.SaveOrUpdate(invoice);
			act = new Act(DateTime.Now, invoice);
			session.SaveOrUpdate(act);
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
			var date = Css("input[name='act.ActDate']");
			date.Clear();
			date.TypeText(newActDate.ToString("dd.MM.yyyy"));
			ClickButton("Сохранить");
			AssertText("Сохранено");

			act.Refresh();
			Assert.That(act.ActDate, Is.EqualTo(newActDate));
		}
	}
}