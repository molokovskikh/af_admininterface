using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminInterface.Controllers;
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
			payer = new Payer {
				Clients = new List<Client>(),
				Users = new List<User>(),
				Addresses = new List<Address>(),
				Recipient = new Recipient(),
				Ads = new List<Advertising>(),
				JuridicalOrganizations = new List<LegalEntity>{
					new LegalEntity {}
				}
			};
			client = new Client(payer);
			payer.Clients.Add(client);
			client.Users.Add(new User(client));
			payer.Users.Add(client.Users[0]);
		}

		[Test]
		public void Invoice_by_quater_contains_bill_for_every_month()
		{
			payer.Users.Each(a => a.Accounting.ReadyForAcounting = true);

			var invoice = new Invoice(payer, Period.FirstQuarter, DateTime.Now);
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
			client.Users.Add(new User(client));
			payer.Users.Add(client.Users[1]);
			client.AddAddress("test");
			client.AddAddress("test");
			client.AddAddress("test");
			payer.Addresses = new List<Address>(client.Addresses);
			payer.Addresses.Each(a => a.Accounting.ReadyForAcounting = true);
			payer.Users.Each(a => a.Accounting.ReadyForAcounting = true);

			var invoice = new Invoice(payer, Period.April, DateTime.Now);

			Assert.That(invoice.Parts[0].Count, Is.EqualTo(2));
			Assert.That(invoice.Parts[0].Cost, Is.EqualTo(800));
			Assert.That(invoice.Parts[0].Sum, Is.EqualTo(1600));
			Assert.That(invoice.Parts[1].Count, Is.EqualTo(1));
			Assert.That(invoice.Parts[1].Sum, Is.EqualTo(200));
		}

		[Test]
		public void Send_invoice_to_email_if_should()
		{
			payer.InvoiceSettings.EmailInvoice = true;
			var invoice = new Invoice(payer, Period.April, DateTime.Now);
			Assert.That(invoice.SendToEmail, Is.True);
		}

		[Test]
		public void Invoice_for_ad()
		{
			var ad = new Advertising(payer) {Cost = 800};
			var invoice = new Invoice(ad);
			Assert.That(invoice.Payer, Is.EqualTo(payer));
			Assert.That(invoice.Sum, Is.EqualTo(800));

			Assert.That(invoice.Period.ToString(),
				Is.EqualTo(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[DateTime.Now.Month]));
			var part = invoice.Parts.Single();
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Name, Is.EqualTo("Рекламное объявление в информационной системе"));
		}

		[Test]
		public void Include_ad_in_next_building_invoice()
		{
			var ad = new Advertising(payer) {Cost = 1500};
			payer.Ads.Add(ad);
			var invoice = new Invoice(payer, Invoice.GetPeriod(DateTime.Now), DateTime.Now);
			Assert.That(invoice.Sum, Is.EqualTo(1500));
			var part = invoice.Parts.Single();
			Assert.That(part.Name, Is.EqualTo("Рекламное объявление в информационной системе"));
			Assert.That(part.Cost, Is.EqualTo(1500));
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(ad.Invoice, Is.EqualTo(invoice));
		}
	}
}