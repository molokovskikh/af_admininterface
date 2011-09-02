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

		[Test]
		public void Show_predefine_invoice_positions()
		{
			Save(new Nomenclature("Мониторинг оптового фармрынка за июль"));

			Open(invoice, "Edit");
			AssertText("Редактирование счета");
			Assert.That(Css("#reference").SelectedItem, Is.EqualTo("Мониторинг оптового фармрынка за июль"));
			Click("Вставить услугу из справочника");
			Assert.That(Css("[name='invoice.parts[0].name']").Text, Is.EqualTo("Мониторинг оптового фармрынка за июль"));
		}

		[Test, Ignore]
		public void Edit_invoice()
		{
			Open(invoice, "Edit");
			Assert.That(browser.Text, Is.StringContaining(String.Format("Редактирование счета №{0}", invoice.Id)));
		}
	}
}
