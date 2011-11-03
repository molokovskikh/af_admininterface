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
		private RevisionAct revisionAct;
		private Payer payer;
		private List<Payment> payments;

		[SetUp]
		public void Setup()
		{
			payer = new Payer {
				Recipient = new Recipient {
					FullName = "ООО \"АналитФАРМАЦИЯ\""
				}
			};
			payments = new List<Payment> {
				new Payment(payer, new DateTime(2011, 1, 15), 1000)
			};
			BuildAct();
		}

		private void BuildAct()
		{
			var invoice = new Invoice(payer,
				Period.December,
				new DateTime(2010, 12, 10),
				new List<InvoicePart>{ new InvoicePart(null, "Мониторинг оптового фармрынка за декабрь", 1000, 1, DateTime.Now)});
			var act = new Act(invoice.Date, invoice);

			var invoice1 = new Invoice(payer,
				Period.December,
				new DateTime(2011, 1, 10),
				new List<InvoicePart>{ new InvoicePart(null, "Мониторинг оптового фармрынка за январь", 500, 2, DateTime.Now)});
			var act1 = new Act(invoice1.Date, invoice1);
			act1.Id = 1;

			var invoice2 = new Invoice(payer,
				Period.December,
				new DateTime(2011, 1, 20),
				new List<InvoicePart>{ new InvoicePart(null, "Мониторинг оптового фармрынка за январь", 1000, 1, DateTime.Now)});
			var act2 = new Act(invoice2.Date, invoice2);
			act2.Id = 2;

			revisionAct = new RevisionAct(payer,
				new DateTime(2011, 1, 1),
				new DateTime(2011, 2, 1),
				new List<Act> { act, act1, act2 },
				payments);
		}

		[Test]
		public void Build_revision_act()
		{
			Assert.That(revisionAct.BeginDate, Is.EqualTo(new DateTime(2011, 1, 1)));
			Assert.That(revisionAct.EndDate, Is.EqualTo(new DateTime(2011, 2, 1)));

			Assert.That(revisionAct.BeginDebit, Is.EqualTo(1000));
			Assert.That(revisionAct.BeginCredit, Is.EqualTo(0));

			Assert.That(revisionAct.DebitSum, Is.EqualTo(2000));
			Assert.That(revisionAct.CreditSum, Is.EqualTo(1000));

			Assert.That(revisionAct.EndDebit, Is.EqualTo(2000));
			Assert.That(revisionAct.EndCredit, Is.EqualTo(0));

			Assert.That(revisionAct.Balance, Is.EqualTo(2000));

			Assert.That(revisionAct.Movements.Count(), Is.EqualTo(6));
			var move = revisionAct.Movements.ElementAt(1);
			Assert.That(move.Debit, Is.EqualTo(1000));
			Assert.That(move.Name, Is.EqualTo("Продажа (10.01.11 № 1)"));
		}

		[Test]
		public void Move_should_contains_init_and_closing_moves()
		{
			Assert.That(revisionAct.Movements.Count(), Is.EqualTo(6));
			var begin = revisionAct.Movements.First();
			Assert.That(begin.Name, Is.EqualTo("Сальдо на 01.01.2011"));
			Assert.That(begin.Debit, Is.EqualTo(1000));
			Assert.That(begin.Credit, Is.EqualTo(0));

			var total = revisionAct.Movements.ElementAt(revisionAct.Movements.Count() - 2);
			Assert.That(total.Name, Is.EqualTo("Обороты за период"));
			Assert.That(total.Debit, Is.EqualTo(2000));
			Assert.That(total.Credit, Is.EqualTo(1000));

			var end = revisionAct.Movements.Last();
			Assert.That(end.Name, Is.EqualTo("Сальдо на 01.02.2011"));
			Assert.That(end.Debit, Is.EqualTo(2000));
			Assert.That(end.Credit, Is.EqualTo(0));
		}

		[Test]
		public void Calculate_begin_saldo()
		{
			payments.Add(new Payment(payer, new DateTime(2010, 12, 15), 1000));
			BuildAct();
			Assert.That(revisionAct.BeginCredit, Is.EqualTo(0));
			Assert.That(revisionAct.BeginDebit, Is.EqualTo(0));
		}

		[Test]
		public void Result_text()
		{
			Assert.That(revisionAct.Result, Is.EqualTo("на 01.02.2011 задолженность в пользу ООО \"АналитФАРМАЦИЯ\" 2000 руб."));
		}

		[Test]
		public void Respect_begin_balance()
		{
			payer.BeginBalance = -500;

			BuildAct();
			Assert.That(revisionAct.BeginDebit, Is.EqualTo(1500));
			Assert.That(revisionAct.BeginCredit, Is.EqualTo(0));
		}
	}
}