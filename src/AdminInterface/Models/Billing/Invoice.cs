using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "billing")]
	public class Invoice : ActiveRecordLinqBase<Invoice>
	{
		private static Dictionary<Period, Period[]> quaterMap = new Dictionary<Period, Period[]> {
			{ Period.FirstQuarter, new[] { Period.January, Period.February, Period.March } },
			{ Period.SecondQuarter, new[] { Period.April, Period.May, Period.June } },
			{ Period.ThirdQuarter, new[] { Period.July, Period.August, Period.September } },
			{ Period.FourthQuarter, new[] { Period.November, Period.October, Period.December } }
		};

		public Invoice()
		{}

		public Invoice(Payer payer, Period period, DateTime date)
		{
			Recipient = payer.JuridicalOrganizations.First(j => j.Recipient != null).Recipient;
			Payer = payer;
			Period = period;
			Sum = payer.TotalSum;
			Date = date;
			CreatedOn = DateTime.Now;
			if (GetInvoicePeriod(Period) == InvoicePeriod.Quarter)
				Sum *= 3;
		}

		private IEnumerable<InvoicePart> GetBillsForPeriod(Period period)
		{
			return Payer.GetAccountings()
				.GroupBy(a => a.Payment)
				.Select(g => new InvoicePart(period, g.Key, g.Count()));
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo]
		public Recipient Recipient { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }

		[Property]
		public decimal Sum { get; set; }

		[Property]
		public DateTime Date { get; set; }

		[Property]
		public Period Period { get; set; }

		[Property]
		public DateTime CreatedOn { get; set; }

		public string SumInWords()
		{
			return ViewHelper.InWords((float) Sum);
		}

		public List<InvoicePart> Bills
		{
			get
			{
				if (GetInvoicePeriod(Period) == InvoicePeriod.Quarter)
				{
					return quaterMap[Period].SelectMany(p => GetBillsForPeriod(p)).ToList();
				}
				else
				{
					return GetBillsForPeriod(Period).ToList();
				}
			}
		}

		public void Send(Controller controller)
		{
			if (Payer.InvoiceSettings.PrintInvoice)
			{
				//new Printer().Print(controller.Context.Services.ViewEngineManager, this);
			}
			if (Payer.InvoiceSettings.EmailInvoice)
			{
				var mailer = new MonorailMailer();
				mailer.Invoice(this);
				mailer.Send();
			}
			Payer.Balance -= Sum;
		}

		public static InvoicePeriod GetInvoicePeriod(Period period)
		{
			if (period == Period.FirstQuarter ||
				period == Period.SecondQuarter ||
				period == Period.FirstQuarter ||
				period == Period.FourthQuarter)
				return InvoicePeriod.Quarter;
			return InvoicePeriod.Month;
		}

		public void Cancel()
		{
			Payer.Balance += Sum;
			Delete();
		}
	}

	public class InvoicePart
	{
		public string Name { get; set; }
		public decimal Cost { get; set; }
		public decimal Sum { get; set; }
		public int Count { get; set; }

		public InvoicePart(Period period, decimal cost, int count)
		{
			Name = String.Format("Мониторинг оптового фармрынка за {0}", BindingHelper.GetDescription(period).ToLower());
			Cost = cost;
			Count = count;
			Sum = Cost * Count;
		}
	}
}