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

		[SetUp]
		public void Setup()
		{
			payer = new Payer {
				Recipient = new Recipient(),
				Addresses = new List<Address>()
			};
			client = new Client(payer);
			var user = new User(client);
			user.Accounting.ReadyForAcounting = true;
			payer.Users = new List<User> {
				user
			};
		}

		[Test]
		public void Build_act()
		{
			var invoice = new Invoice(payer, Period.January, DateTime.Now);
			var act = new Act(DateTime.Now, invoice);
			Assert.That(act.Parts.Count, Is.EqualTo(1));
			var part = act.Parts[0];
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Name, Is.StringContaining("���������� �������� ���������"));
		}

		[Test]
		public void Build_multi_invoice_act()
		{
			var invoice = new Invoice(payer, Period.January, DateTime.Now);
			var invoice1 = new Invoice();
			invoice1.SetPayer(payer);
			invoice1.Period = Period.January;
			invoice1.Date = DateTime.Now;
			invoice1.Parts = new List<InvoicePart>();
			invoice1.Parts.Add(new InvoicePart(invoice1, Period.January, 150, 1) { Name = "�������������� ������" });

			var act = new Act(DateTime.Now, invoice, invoice1);
			Assert.That(act.Parts.Count, Is.EqualTo(2));
			var part = act.Parts[0];
			Assert.That(part.Count, Is.EqualTo(1));
			Assert.That(part.Cost, Is.EqualTo(800));
			Assert.That(part.Name, Is.StringContaining("���������� �������� ���������"));
			var part1 = act.Parts[1];
			Assert.That(part1.Count, Is.EqualTo(1));
			Assert.That(part1.Cost, Is.EqualTo(150));
			Assert.That(part1.Name, Is.EqualTo("�������������� ������"));
		}
	}
}