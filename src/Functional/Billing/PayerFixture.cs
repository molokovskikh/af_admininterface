using System;
using System.Linq;
using System.Threading;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using WatiNCssSelectorExtensions;

namespace Functional.Billing
{
	[TestFixture]
	public class PayerFixture : WatinFixture
	{
		[Test]
		public void Show_balance_summary()
		{
			Payer payer;
			using(new SessionScope())
			{
				payer = DataMother.BuildPayerForBillingDocumentTest();
				new Invoice(payer, Period.January, new DateTime(2011, 1, 11)).Save();
				new Payment { Payer = payer, Recipient = payer.Recipient, PayedOn = new DateTime(2011, 1, 15), RegistredOn = DateTime.Now, Sum = 800 }.Save();
			}

			using (var browser = Open("Billing/Edit?BillingCode={0}", payer.Id))
			{
				Assert.That(browser.Text, Is.StringContaining("плательщик"));
				browser.Link(Find.ByText(@"Платежи/Счета")).Click();
				Thread.Sleep(1000);
				Assert.That(browser.Text, Is.StringContaining("11.01.2011"));
				Assert.That(browser.Text, Is.StringContaining("15.01.2011"));
			}
		}

		[Test]
		public void Custom_invoice()
		{
			Payer payer = null;
			using (var browser = Open(payer))
			{
				browser.Link(Find.ByText("Новый счет")).Click();
				browser.Css("#invoice_date").Value = DateTime.Now;
				browser.Css("#invoice_bills[0]_Name").Value = "Информационные услуги за октябрь";
				browser.Css("#invoice_bills[0]_Cost").Value = "500";
				browser.Css("#invoice_bills[0]_Count").Value = "1";
				browser.Button(Find.ByText("Сохранить")).Click();
			}
		}
	}
}