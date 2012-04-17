using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Tools;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;

namespace Functional.Billing
{
	public class PaymentFixture : WatinFixture2
	{
		private Payer payer;
		private Payment payment;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			payment = new Payment(payer);
			payment.Sum = 800;
			payment.RegisterPayment();
			payment.Save();
		}

		[Test]
		public void Set_payment_for_advertising()
		{
			Open(payment, "Edit");
			Assert.That(browser.Text, Is.StringContaining("Редактирование платежа"));
			browser.CheckBox(Find.ByName("payment.ForAd")).Checked = true;
			browser.TextField(Find.ByName("payment.AdSum")).TypeText(payment.Sum.ToString());
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
		}

		[Test]
		public void View_payments()
		{
			Open();
			Click("Платежи");
			AssertText("Платежи");
		}

		[Test]
		public void Confirm_unknown_payment()
		{
			var begin = DateTime.Now;

			Open();
			Click("Разнесение платежей");
			AssertText("Разнесение платежей");
			Css("input[name='Payment.Sum']").TypeText("500");
			Click("Добавить");
			AssertText("Создать неопознанный платеж?");
			var continueButton =  browser.Spans.Where(s => s.Text == "Продолжить").Select(s => (Button)s.Parent).First();
			continueButton.Click();

			var payments = Payments();
			Assert.That(payments.Text, Is.StringContaining("500"));

			var saved = session.Query<Payment>().Where(p => p.RegistredOn >= begin && p.Id != payment.Id).ToList();
			Assert.That(saved.Count, Is.EqualTo(1), saved.Implode());
			var savedPayment = saved[0];
			Assert.That(savedPayment.Sum, Is.EqualTo(500));
		}

		public Table Payments()
		{
			return Css(".DataTable");
		}
	}
}