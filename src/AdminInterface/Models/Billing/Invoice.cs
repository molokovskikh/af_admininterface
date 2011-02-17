using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;

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

		public Invoice(Payer payer)
		{
			Parts = new List<InvoicePart>();
			Date = DateTime.Now;
			SetPayer(payer);
		}

		public Invoice(Payer payer, Period period, DateTime invoiceDate)
			: this(payer)
		{
			Period = period;
			Sum = payer.TotalSum;
			Date = invoiceDate;
			CreatedOn = DateTime.Now;
			Parts = BuildParts();
			Sum = Parts.Sum(p => p.Sum);
			SendToEmail = Payer.InvoiceSettings.EmailInvoice;
			PayerName = payer.JuridicalName;
		}

		public void SetPayer(Payer payer)
		{
			Recipient = payer.Recipient;
			Payer = payer;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[
			BelongsTo,
			ValidateNonEmpty("У плательщика должен быть установлен получатель платежей")
		]
		public Recipient Recipient { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }

		[Property]
		public decimal Sum { get; set; }

		[Property]
		public string PayerName { get; set; }

		[Property, ValidateNonEmpty]
		public DateTime Date { get; set; }

		[Property, ValidateNonEmpty]
		public Period Period { get; set; }

		[Property]
		public DateTime CreatedOn { get; set; }

		[Property(NotNull = true, Default = "0")]
		public bool SendToEmail { get; set; }

		[Property]
		public DateTime? LastErrorNotification { get; set; }

		[
			HasMany(Cascade = ManyRelationCascadeEnum.All, Lazy = true),
			ValidateCollectionNotEmpty("Нужно задать список услуг")
		]
		public IList<InvoicePart> Parts { get; set; }

		public string SumInWords()
		{
			return ViewHelper.InWords((float) Sum);
		}

		public List<InvoicePart> BuildParts()
		{
			if (GetInvoicePeriod(Period) == InvoicePeriod.Quarter)
			{
				return quaterMap[Period].SelectMany(p => GetPartsForPeriod(p)).ToList();
			}
			else
			{
				return GetPartsForPeriod(Period).ToList();
			}
		}

		private IEnumerable<InvoicePart> GetPartsForPeriod(Period period)
		{
			return Payer.GetAccountings()
				.GroupBy(a => a.Payment)
				.Select(g => new InvoicePart(this, period, g.Key, g.Count()));
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

		public void Send()
		{
			if (Payer.InvoiceSettings.EmailInvoice)
			{
				var mailer = new MonorailMailer();
				mailer.Invoice(this);
				mailer.Send();
			}
			SendToEmail = false;
		}

		public bool ShouldNotify()
		{
			return LastErrorNotification == null || (DateTime.Now - LastErrorNotification.Value).TotalDays > 1;
		}
	}
}