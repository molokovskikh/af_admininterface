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
		private RevisionAct act;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = new Payer {
				Recipient = new Recipient {
					FullName = "ООО \"АналитФАРМАЦИЯ\""
				}
			};
			BuildAct();
		}

		private void BuildAct()
		{
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

			act = new RevisionAct(payer,
				new DateTime(2011, 1, 1),
				new DateTime(2011, 2, 1),
				new List<Invoice> { invoice, invoice1, invoice2 },
				new List<Payment> {
					new Payment(payer, new DateTime(2011, 1, 15), 1000)
				});
		}

		[Test]
		public void Build_revision_act()
		{
			Assert.That(act.BeginDate, Is.EqualTo(new DateTime(2011, 1, 1)));
			Assert.That(act.EndDate, Is.EqualTo(new DateTime(2011, 2, 1)));

			Assert.That(act.BeginDebit, Is.EqualTo(1000));
			Assert.That(act.BeginCredit, Is.EqualTo(0));

			Assert.That(act.DebitSum, Is.EqualTo(2000));
			Assert.That(act.CreditSum, Is.EqualTo(1000));

			Assert.That(act.EndDebit, Is.EqualTo(2000));
			Assert.That(act.EndCredit, Is.EqualTo(0));

			Assert.That(act.Balance, Is.EqualTo(2000));

			Assert.That(act.Movements.Count(), Is.EqualTo(6));
			var move = act.Movements.ElementAt(1);
			Assert.That(move.Debit, Is.EqualTo(1000));
			Assert.That(move.Name, Is.EqualTo("Продажа (10.01.11 № 1)"));
		}

		[Test]
		public void Move_should_contains_init_and_closing_moves()
		{
			Assert.That(act.Movements.Count(), Is.EqualTo(6));
			var begin = act.Movements.First();
			Assert.That(begin.Name, Is.EqualTo("Сальдо на 01.01.2011"));
			Assert.That(begin.Debit, Is.EqualTo(1000));
			Assert.That(begin.Credit, Is.EqualTo(0));

			var total = act.Movements.ElementAt(act.Movements.Count() - 2);
			Assert.That(total.Name, Is.EqualTo("Обороты за период"));
			Assert.That(total.Debit, Is.EqualTo(2000));
			Assert.That(total.Credit, Is.EqualTo(1000));

			var end = act.Movements.Last();
			Assert.That(end.Name, Is.EqualTo("Сальдо на 01.02.2011"));
			Assert.That(end.Debit, Is.EqualTo(2000));
			Assert.That(end.Credit, Is.EqualTo(0));
		}

		[Test]
		public void Result_text()
		{
			Assert.That(act.Result, Is.EqualTo("на 01.02.2011 задолженность в пользу ООО \"АналитФАРМАЦИЯ\" 2000 руб."));
		}

		[Test]
		public void Respect_begin_balance()
		{
			payer.BeginBalance = -500;

			BuildAct();
			Assert.That(act.BeginDebit, Is.EqualTo(1500));
			Assert.That(act.BeginCredit, Is.EqualTo(0));
		}
	}
}