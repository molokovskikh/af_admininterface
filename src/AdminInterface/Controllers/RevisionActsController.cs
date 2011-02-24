using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	public class RevisionActPart
	{
		public RevisionActPart(string name, decimal debit, decimal credit, DateTime date)
		{
			Name = name;
			Debit = debit;
			Credit = credit;
			Date = date;
		}

		public RevisionActPart(Invoice invoice)
		{
			Name = String.Format("Продажа ({0:dd.MM.yy} № {1})", invoice.Date, invoice.Id);
			Date = invoice.Date;
			Debit = invoice.Sum;
		}

		public RevisionActPart(Payment payment)
		{
			Name = String.Format("Оплата ({0:dd.MM.yy} № {1})", payment.PayedOn, payment.Id);
			Date = payment.PayedOn;
			Credit = payment.Sum;
		}

		public string Name { get; set; }
		public DateTime Date { get; set; }
		public decimal Debit { get; set; }
		public decimal Credit { get; set; }
	}

	public class RevisionAct
	{
		public RevisionAct(Payer payer, DateTime begin, DateTime end, IEnumerable<Invoice> invoices, IEnumerable<Payment> payments)
		{
			Payer = payer;
			BeginDate = begin;
			EndDate = end;

			var beginDebit = invoices.Where(i => i.Date < begin).Sum(i => i.Sum);
			var beginCredit = payments.Where(p => p.PayedOn < begin).Sum(p => p.Sum);

			var beginBalance = beginCredit - beginDebit;
			beginBalance += payer.BeginBalance;

			if (beginBalance > 0)
				BeginCredit = beginBalance;
			else
				BeginDebit = Math.Abs(beginBalance);

			var movements = invoices.Where(i => i.Date >= begin && i.Date <= end)
					.Select(i => new RevisionActPart(i))
				.Union(payments.Where(p => p.PayedOn >= begin && p.PayedOn <= end)
					.Select(p => new RevisionActPart(p)))
				.OrderBy(m => m.Date)
				.ToList();
			DebitSum = movements.Sum(m => m.Debit);
			CreditSum = movements.Sum(m => m.Credit);

			Balance = BeginDebit + DebitSum - (BeginCredit + CreditSum);
			if (Balance > 0)
				EndDebit = Balance;
			else
				EndCredit = Math.Abs(Balance);

			Balance = Math.Abs(Balance);

			movements.Insert(0,
				new RevisionActPart(String.Format("Сальдо на {0}", BeginDate.ToShortDateString()),
					BeginDebit,
					BeginCredit,
					BeginDate));

			movements.Add(
				new RevisionActPart("Обороты за период",
					DebitSum,
					CreditSum,
					EndDate));

			movements.Add(
				new RevisionActPart(String.Format("Сальдо на {0}", EndDate.ToShortDateString()),
					EndDebit,
					EndCredit,
					EndDate));

			Movements = movements;
		}

		public Payer Payer { get; set; }

		public IEnumerable<RevisionActPart> Movements { get; set; }

		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }

		public decimal CreditSum { get; set; }
		public decimal DebitSum { get; set; }

		public decimal BeginDebit { get; set; }
		public decimal BeginCredit { get; set; }

		public decimal EndDebit { get; set; }
		public decimal EndCredit { get; set; }

		public decimal Balance { get; set; }

		public string Result
		{
			get
			{
				string creditor;
				if (Balance == 0)
					return String.Format("на {0:dd.MM.yyyy} задолженность отсутствует.", EndDate);
				else if (EndDebit > 0)
					creditor = Payer.Recipient.FullName;
				else
					creditor = Payer.JuridicalName;

				return String.Format("на {0:dd.MM.yyyy} задолженность в пользу {1} {2} руб.", EndDate, creditor, Balance.ToString("#.#"));
			}
		}
	}

	[Layout("GeneralWithJQueryOnly")]
	public class RevisionActsController : SmartDispatcherController
	{
		public void Show(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				PropertyBag["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["beginDate"] = begin.Value;
			PropertyBag["endDate"] = end.Value;
			PropertyBag["act"] = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Invoice.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());
			PropertyBag["payer"] = payer;
		}

		public void Print(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["act"] = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Invoice.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());
			PropertyBag["payer"] = payer;
		}

		public void Mail(uint id, DateTime? begin, DateTime? end, string emails)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Invoice.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());

			this.Mail().RevisionAct(act, emails).Send();

			Flash["Message"] = Message.Notify("Отправлено");
			RedirectToReferrer();
		}

		public void Excel(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Invoice.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());

			Exporter.ToResponse(Response, Exporter.Export(act));
			CancelView();
		}
	}
}