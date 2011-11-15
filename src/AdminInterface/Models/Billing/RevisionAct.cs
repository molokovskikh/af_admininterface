using System;
using System.Collections.Generic;
using System.Linq;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	public class RevisionAct
	{
		public RevisionAct(Payer payer, DateTime begin, DateTime end,
			IEnumerable<Act> acts,
			IEnumerable<Payment> payments,
			IEnumerable<BalanceOperation> operations)
		{
			if (payer.Recipient == null)
				throw new Exception("У плательщика не указан получатель платежей, выберете получателя платежей.");

			Payer = payer;
			BeginDate = begin;
			EndDate = end;

			var parts = acts.Select(i => new RevisionActPart(i))
				.Union(payments.Select(p => new RevisionActPart(p)))
				.Union(operations.Select(d => new RevisionActPart(d)))
				.ToList();

			var beginDebit = parts.Where(i => i.Date < begin).Sum(i => i.Debit);
			var beginCredit = parts.Where(p => p.Date < begin).Sum(p => p.Credit);

			var beginBalance = beginCredit - beginDebit;
			beginBalance += payer.BeginBalance;

			if (beginBalance > 0)
				BeginCredit = beginBalance;
			else
				BeginDebit = Math.Abs(beginBalance);

			var movements = parts.Where(p => p.Date >= begin && p.Date <= end)
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

	public class RevisionActPart
	{
		public RevisionActPart(string name, decimal debit, decimal credit, DateTime date)
		{
			Name = name;
			Debit = debit;
			Credit = credit;
			Date = date;
		}

		public RevisionActPart(Act act)
		{
			Name = String.Format("Продажа ({0:dd.MM.yy} № {1})", act.ActDate, act.Id);
			Date = act.ActDate;
			Debit = act.Sum;
		}

		public RevisionActPart(Payment payment)
		{
			Name = String.Format("Оплата ({0:dd.MM.yy} № {1})", payment.PayedOn, payment.Id);
			Date = payment.PayedOn;
			Credit = payment.Sum;
		}

		public RevisionActPart(BalanceOperation operation)
		{
			Name = String.Format("{0} ({1:dd.MM.yy} № {2})",
				BindingHelper.GetDescription(operation.Type),
				operation.Date,
				operation.Id);
			Date = operation.Date;
			if (operation.Type == OperationType.DebtRelief)
				Debit = Decimal.Negate(operation.Sum);
			else
				Credit = Decimal.Negate(operation.Sum);
		}

		public string Name { get; set; }
		public DateTime Date { get; set; }
		public decimal Debit { get; set; }
		public decimal Credit { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}