using System;
using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class ActFixture
	{
		private Payer payer;
		private Client client;
		private Invoice invoice;

		[SetUp]
		public void Setup()
		{
			payer = new Payer {
				JuridicalName = "ООО 'Рога и копыта'",
				Recipient = new Recipient(),
				Addresses = new List<Address>(),
				Ads = new List<Advertising>()
			};
			client = new Client(payer);
			var user = new User(client);
			user.Accounting.ReadyForAcounting = true;
			payer.Users = new List<User> {
				user
			};

			invoice = new Invoice(payer, Period.January, DateTime.Now);
		}

		[Test]
		public void Build_act()
		{
			var act = new Act(DateTime.Now, invoice);
			Assert.That(act.Parts.Count, Is.EqualTo(1));
			var part = act.Parts[0];
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Name, Is.StringContaining("Мониторинг оптового фармрынка"));
		}

		[Test]
		public void Build_multi_invoice_act()
		{
			var invoice1 = new Invoice();
			invoice1.SetPayer(payer);
			invoice1.Period = Period.January;
			invoice1.Date = DateTime.Now;
			invoice1.Parts = new List<InvoicePart>();
			invoice1.Parts.Add(new InvoicePart(invoice1, Period.January, 150, 1) { Name = "Информационные услуги" });

			var act = new Act(DateTime.Now, invoice, invoice1);
			Assert.That(act.Parts.Count, Is.EqualTo(2));
			var part = act.Parts[0];
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Name, Is.StringContaining("Мониторинг оптового фармрынка"));
			var part1 = act.Parts[1];
			Assert.That(part1.Count, Is.EqualTo(1));
			Assert.That(part1.Cost, Is.EqualTo(150));
			Assert.That(part1.Name, Is.EqualTo("Информационные услуги"));
		}

		[Test]
		public void Payer_name_in_act_should_be_same_as_invoice()
		{
			payer.JuridicalName = "ООО 'Хвосты и плетки'";
			var act = new Act(DateTime.Now, invoice);
			Assert.That(act.PayerName, Is.EqualTo("ООО 'Рога и копыта'"));
		}

		[Test]
		public void After_build_act_update_ad_act_reference()
		{
			var ad = new Advertising(payer) {Cost = 1500};
			payer.Ads.Add(ad);
			invoice = new Invoice(payer, Invoice.GetPeriod(DateTime.Now), DateTime.Now);
			var act = new Act(DateTime.Now, invoice);
			Assert.That(ad.Act, Is.EqualTo(act));
			Assert.That(act.Sum, Is.EqualTo(2300));
		}
	}
}