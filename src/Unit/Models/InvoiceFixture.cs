using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Tools;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class InvoiceFixture
	{
		private Payer payer;
		private Client client;

		[SetUp]
		public void Setup()
		{
			payer = new Payer("Тестовый плательщик") {
				Recipient = new Recipient(),
			};
			client = new Client(payer, Data.DefaultRegion);
			payer.Clients.Add(client);
			client.AddUser(new User(client));

			payer.Users.Each(a => a.Accounting.ReadyForAccounting = true);
		}

		[Test]
		public void Invoice_by_quater_contains_bill_for_every_month()
		{
			var invoice = new Invoice(payer, new Period(2011, Interval.FirstQuarter), DateTime.Now);
			Assert.That(invoice.Parts.Count, Is.EqualTo(3));
			Assert.That(invoice.Parts[0].Name, Is.EqualTo("Мониторинг оптового фармрынка за январь"));
			Assert.That(invoice.Parts[0].Sum, Is.EqualTo(800));
			Assert.That(invoice.Parts[1].Name, Is.StringContaining("февраль"));
			Assert.That(invoice.Parts[1].Sum, Is.EqualTo(800));
			Assert.That(invoice.Parts[2].Name, Is.StringContaining("март"));
			Assert.That(invoice.Parts[2].Sum, Is.EqualTo(800));
			Assert.That(invoice.Sum, Is.EqualTo(2400));
		}

		[Test]
		public void Split_services_by_type()
		{
			client.AddUser(new User(client));
			client.AddAddress("test");
			client.AddAddress("test");
			client.AddAddress("test");
			payer.Addresses = new List<Address>(client.Addresses);
			payer.Addresses.Each(a => {
				a.Accounting.ReadyForAccounting = true;
				a.AvaliableForUsers.Add(client.Users[0]);
			});
			payer.Users.Each(a => a.Accounting.ReadyForAccounting = true);

			var invoice = new Invoice(payer, new Period(2011, Interval.April), DateTime.Now);

			var part = invoice.Parts[0];
			Assert.That(part.Count, Is.EqualTo(2), part.ToString());
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Sum, Is.EqualTo(1600));
			Assert.That(invoice.Parts[1].Count, Is.EqualTo(1));
			Assert.That(invoice.Parts[1].Sum, Is.EqualTo(200));
		}

		[Test]
		public void Send_invoice_to_email_if_should()
		{
			payer.InvoiceSettings.EmailInvoice = true;
			var invoice = new Invoice(payer, new Period(2011, Interval.April), DateTime.Now);
			Assert.That(invoice.SendToEmail, Is.True);
		}

		[Test]
		public void Invoice_for_ad()
		{
			var ad = new Advertising(payer) {Cost = 800};
			var invoice = new Invoice(ad);
			Assert.That(invoice.Payer, Is.EqualTo(payer));
			Assert.That(invoice.Sum, Is.EqualTo(800));

			Assert.That(invoice.Period,
				Is.EqualTo(DateTime.Now.ToPeriod()));
			var part = invoice.Parts.Single();
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Name, Is.EqualTo("Рекламное объявление в информационной системе"));
		}

		[Test]
		public void Include_ad_in_next_building_invoice()
		{
			payer.Users.Each(a => a.Accounting.ReadyForAccounting = false);

			var ad = new Advertising(payer) {Cost = 1500};
			payer.Ads.Add(ad);
			var invoice = new Invoice(payer, DateTime.Now.ToPeriod(), DateTime.Now);
			Assert.That(invoice.Sum, Is.EqualTo(1500));
			var part = invoice.Parts.Single();
			Assert.That(part.Name, Is.EqualTo("Рекламное объявление в информационной системе"));
			Assert.That(part.Cost, Is.EqualTo(1500));
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Ad, Is.EqualTo(ad));
		}

		[Test]
		public void Document_on_last_working_day()
		{
			payer.InvoiceSettings.DocumentsOnLastWorkingDay = true;
			var invoice = new Invoice(payer, new DateTime(2011, 1, 10));
			Assert.That(invoice.Date, Is.EqualTo(new DateTime(2011, 1, 31)));
		}

		[Test]
		public void Do_not_group_parts()
		{
			payer.InvoiceSettings.DoNotGroupParts = true;
			client.AddUser(new User(client));
			payer.Users.Each(a => a.Accounting.ReadyForAccounting = true);

			var invoice = new Invoice(payer, DateTime.Now.ToPeriod(), DateTime.Now);
			Assert.That(invoice.Parts.Count, Is.EqualTo(2), invoice.Parts.Implode());
			Assert.That(invoice.Parts[0].Sum, Is.EqualTo(800), invoice.Parts.Implode());
			Assert.That(invoice.Parts[0].Count, Is.EqualTo(1), invoice.Parts.Implode());
			Assert.That(invoice.Parts[1].Sum, Is.EqualTo(800), invoice.Parts.Implode());
			Assert.That(invoice.Parts[1].Count, Is.EqualTo(1), invoice.Parts.Implode());
		}

		[Test]
		public void Build_invoice_for_invoice_groups()
		{
			client.AddUser(new User(client));
			payer.Users[0].Accounting.ReadyForAccounting = true;
			payer.Users[1].Accounting.ReadyForAccounting = true;
			payer.Users[1].Accounting.Payment = 600;
			payer.Users[1].Accounting.InvoiceGroup = 1;

			var invoices = payer.BuildInvoices(DateTime.Now, DateTime.Now.ToPeriod()).ToList();
			Assert.That(invoices.Count, Is.EqualTo(2));
			var invoice = invoices[0];
			Assert.That(invoice.Parts.Count, Is.EqualTo(1));
			Assert.That(invoice.Parts[0].Sum, Is.EqualTo(800));
			invoice = invoices[1];
			Assert.That(invoice.Parts.Count, Is.EqualTo(1));
			Assert.That(invoice.Parts[0].Sum, Is.EqualTo(600));
		}

		
		[Test]
		public void Set_pay_date()
		{
			var invoice = new Invoice(payer, DateTime.Now.ToPeriod(), DateTime.Now);
			var part = invoice.Parts[0];
			Assert.That(part.PayDate, Is.EqualTo(invoice.Date));
			Assert.That(part.Processed, Is.False);
		}

		[Test]
		public void Set_quarter_period()
		{
			var invoice = new Invoice(payer, new Period(2011, Interval.FirstQuarter), new DateTime(2011, 1, 10));
			Assert.That(invoice.Parts.Count, Is.EqualTo(3));
			Assert.That(invoice.Parts[0].PayDate, Is.EqualTo(new DateTime(2011, 1, 10)));
			Assert.That(invoice.Parts[1].PayDate, Is.EqualTo(new DateTime(2011, 2, 10)));
			Assert.That(invoice.Parts[2].PayDate, Is.EqualTo(new DateTime(2011, 3, 10)));
		}
	}
}