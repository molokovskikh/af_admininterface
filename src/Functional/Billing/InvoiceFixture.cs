using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;

namespace Functional.Billing
{
	[TestFixture]
	public class InvoiceFixture : WatinFixture2
	{
		private Payer payer;
		private Invoice invoice;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			invoice = new Invoice(payer, Period.January, new DateTime(2010, 12, 27));
			invoice.Save();
		}

		[Test]
		public void Cancel_invoice()
		{
			Open("Invoices/");
			Assert.That(browser.Text, Is.StringContaining("Реестр счетов"));
			browser.Button(invoice, "Cancel").Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
		}

		[Test, Ignore]
		public void Edit_invoice()
		{
			Open(invoice, "Edit");
			Assert.That(browser.Text, Is.StringContaining(String.Format("Редактирование счета №{0}", invoice.Id)));
		}
	}
}