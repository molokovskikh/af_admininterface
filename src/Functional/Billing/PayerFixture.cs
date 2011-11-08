using System;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class PayerFixture : WatinFixture2
	{
		[Test]
		public void Show_balance_summary()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			new Invoice(payer, Period.January, new DateTime(2011, 1, 11)).Save();
			new Payment { Payer = payer, Recipient = payer.Recipient, PayedOn = new DateTime(2011, 1, 15), RegistredOn = DateTime.Now, Sum = 800 }.Save();

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining("плательщик"));
			browser.Link(Find.ByText(@"Платежи/Счета")).Click();
			Thread.Sleep(1000);
			Assert.That(browser.Text, Is.StringContaining("11.01.2011"));
			Assert.That(browser.Text, Is.StringContaining("15.01.2011"));
		}

		[Test]
		public void Show_error_on_revision_act_in_not_configured_payer()
		{
			var payer = DataMother.CreatePayer();
			payer.Save();

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));
			Click("Акт сверки");
			Assert.That(browser.Text, Is.StringContaining("У плательщика не указан получатель платежей, выберете получателя платежей"));
		}

		[Test]
		public void Create_act()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			Click("Доп. акт");
			AssertText("Формирование дополнительного акта");
			Css("#act_ActDate").Value = DateTime.Now.ToShortDateString();
			Css("#act_parts_0__name").Value = "Информационные услуги за октябрь";
			Css("#act_parts_0__cost").Value = "800";
			Css("#act_parts_0__count").Value = "2";
			Click("Сохранить");
			AssertText("Акт сформирован");
		}

		[Test, Ignore("Не реализованно")]
		public void Custom_invoice()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			Click("Новый счет");
			Css("#invoice_date").Value = DateTime.Now;
			Css("#invoice_bills[0]_Name").Value = "Информационные услуги за октябрь";
			Css("#invoice_bills[0]_Cost").Value = "500";
			Css("#invoice_bills[0]_Count").Value = "1";
			Click("Сохранить");
		}
	}
}