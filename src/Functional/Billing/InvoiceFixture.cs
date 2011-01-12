using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
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
	public class InvoiceFixture : WatinFixture
	{
		[Test]
		public void Cancel_invoice()
		{
			Invoice invoice;
			using(new SessionScope())
			{
				var client = DataMother.CreateTestClientWithAddressAndUser();
				client.Payer.Recipient = Recipient.Queryable.First();
				invoice = new Invoice(client.Payer, Period.January, new DateTime(2010, 12, 27));
				invoice.Save();
			}

			using (var browser = Open("Invoices/"))
			{
				Assert.That(browser.Text, Is.StringContaining("Реестр счетов"));
				browser.LinkFor(invoice, "Cancel").Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}
		}
	}

	public static class WatinExtentions
	{
		public static Link LinkFor(this Browser browser, object item, string action)
		{
			var url = AppHelper.GetUrl(item, action);
			return browser.Link(l => l.Url.EndsWith(url));
		}

		public static dynamic Css(this Browser browser, string selector)
		{
			return browser.CssSelect(selector);
		}
	}
}