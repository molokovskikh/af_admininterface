using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using ExcelLibrary.SpreadSheet;

namespace AdminInterface.Controllers
{
	public class RevisionActPart
	{
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
			BeginDate = begin;
			EndDate = end;

			BeginDebit = invoices.Where(i => i.Date < begin).Sum(i => i.Sum);
			BeginCredit = payments.Where(p => p.PayedOn < begin).Sum(p => p.Sum);
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

			Movements = movements;
		}

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
				PropertyBag["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
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

		public void Excel(uint id, DateTime? begin, DateTime? end)
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

			var act = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Invoice.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());
			CancelView();

			Response.Clear();
			Response.AppendHeader("Content-Disposition", 
				String.Format("attachment; filename=\"{0}\"", Uri.EscapeDataString("Акт сверки.xls")));
			Response.ContentType = "application/vnd.ms-excel";
			
			using(var stream = new MemoryStream())
			{
				var book = new Workbook();
				var worksheet = new Worksheet("Акт сверки");
				book.Worksheets.Add(worksheet);
				book.Save(stream);
				stream.Position = 0;
				stream.CopyTo(Response.OutputStream);
			}
		}

		private void BuildExcel(Worksheet worksheet)
		{
			
		}
	}

	public class ExcelReport
	{
		public void Build()
		{
			
		}

		public void Cells(params object[] args)
		{
			Cells("Акт сверки", new Merge(8));
			//Cells()
		}
	}

	public class Merge
	{
		public Merge(int count)
		{}

	}
}