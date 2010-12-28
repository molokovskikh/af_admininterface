using System;
using System.Collections.Generic;
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
				JuridicalOrganizations = new List<LegalEntity>{
					new LegalEntity {
						Recipient = new Recipient()
					}
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
	}
}