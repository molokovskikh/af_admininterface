using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class RevisionActFixture
	{
		[Test]
		public void Build_revision_act()
		{
			var payer = new Payer {
				Recipient = new Recipient()
			};
			var invoice = new Invoice(payer) {
				Date = new DateTime(2010, 12, 10),
			};
			invoice.Parts = new List<InvoicePart>{ new InvoicePart(invoice, Period.December, 1000, 1)};
			invoice.CalculateSum();

			var invoice1 = new Invoice(payer) {
				Id = 1,
				Date = new DateTime(2011, 1, 10),
			};
			invoice1.Parts = new List<InvoicePart>{ new InvoicePart(invoice1, Period.December, 500, 2)};
			invoice1.CalculateSum();

			var invoice2 = new Invoice(payer) {
				Date = new DateTime(2011, 1, 20),
			};
			invoice2.Parts = new List<InvoicePart>{ new InvoicePart(invoice2, Period.December, 1000, 1)};
			invoice2.CalculateSum();

			var act = new RevisionAct(payer,
				new DateTime(2011, 1, 1),
				new DateTime(2011, 2, 1),
				new List<Invoice> { invoice, invoice1, invoice2 },
				new List<Payment> {
					new Payment(payer, new DateTime(2011, 1, 15), 1000)
				});

			Assert.That(act.BeginDate, Is.EqualTo(new DateTime(2011, 1, 1)));
			Assert.That(act.EndDate, Is.EqualTo(new DateTime(2011, 2, 1)));

			Assert.That(act.BeginDebit, Is.EqualTo(1000));
			Assert.That(act.BeginCredit, Is.EqualTo(0));

			Assert.That(act.DebitSum, Is.EqualTo(2000));
			Assert.That(act.CreditSum, Is.EqualTo(1000));

			Assert.That(act.EndDebit, Is.EqualTo(2000));
			Assert.That(act.EndCredit, Is.EqualTo(0));

			Assert.That(act.Balance, Is.EqualTo(2000));

			Assert.That(act.Movements.Count(), Is.EqualTo(3));
			var move = act.Movements.First();
			Assert.That(move.Debit, Is.EqualTo(1000));
			Assert.That(move.Name, Is.EqualTo("Продажа (10.01.11 № 1)"));
		}
	}
}