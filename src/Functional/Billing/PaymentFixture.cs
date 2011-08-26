using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

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
			Open("/");
			browser.Link(Find.ByText("Платежи")).Click();
		}
	}
}