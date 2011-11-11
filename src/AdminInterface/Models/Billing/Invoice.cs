using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Engine;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "billing")]
	public class Invoice : BalanceUpdater<Invoice>
	{
		private static Dictionary<Period, Period[]> quaterMap = new Dictionary<Period, Period[]> {
			{ Period.FirstQuarter, new[] { Period.January, Period.February, Period.March } },
			{ Period.SecondQuarter, new[] { Period.April, Period.May, Period.June } },
			{ Period.ThirdQuarter, new[] { Period.July, Period.August, Period.September } },
			{ Period.FourthQuarter, new[] { Period.October, Period.November, Period.December } }
		};

		public Invoice()
			: base(BalanceUpdaterType.ChargeOff)
		{}

		public Invoice(Advertising ad)
			: this(ad.Payer)
		{
			Period = Date.ToPeriod();
			Parts.Add(PartForAd(ad));
			CalculateSum();
		}

		public Invoice(Payer payer)
			: this(payer, DateTime.Now)
		{}

		public Invoice(Payer payer, DateTime date)
			: this()
		{
			Parts = new List<InvoicePart>();
			CreatedOn = DateTime.Now;

			SetPayer(payer);
			Date = Payer.GetDocumentDate(date);
			Period = Date.ToPeriod();
		}

		public Invoice(Payer payer, Period period, DateTime invoiceDate, IEnumerable<InvoicePart> parts)
			: this(payer, invoiceDate)
		{
			Period = period;
			foreach (var part in parts)
				part.Invoice = this;
			Parts = parts.ToList();
			CalculateSum();
		}

		public Invoice(Payer payer, Period period, DateTime invoiceDate, int invoiceGroup = 0)
			: this(payer, invoiceDate)
		{
			Period = period;
			Parts = BuildParts(invoiceGroup);
			CalculateSum();
		}

		private InvoicePart PartForAd(Advertising ad)
		{
			return new InvoicePart(this,
				"Рекламное объявление в информационной системе",
				ad.Cost,
				1,
				Date) {
					Ad = ad
				};
		}

		public void SetPayer(Payer payer)
		{
			Recipient = payer.Recipient;
			Payer = payer;
			SendToEmail = Payer.InvoiceSettings.EmailInvoice;
			PayerName = payer.JuridicalName;
			Customer = payer.Customer;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[
			BelongsTo,
			ValidateNonEmpty("У плательщика должен быть установлен получатель платежей")
		]
		public Recipient Recipient { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public override Payer Payer { get; set; }

		[Property]
		public virtual decimal Sum { get; set; }

		[Property]
		public virtual decimal PaidSum { get; set; }

		[Property]
		public string PayerName { get; set; }

		[Property]
		public string Customer { get; set; }

		[Property, ValidateNonEmpty]
		public DateTime Date { get; set; }

		[Property, ValidateNonEmpty]
		public Period Period { get; set; }

		[Property]
		public DateTime CreatedOn { get; set; }

		[Property]
		public bool SendToEmail { get; set; }

		[Property]
		public DateTime? LastErrorNotification { get; set; }

		[BelongsTo(Lazy = FetchWhen.OnInvoke)]
		public virtual Act Act { get; set; }

		[
			HasMany(Cascade = ManyRelationCascadeEnum.All, Lazy = true),
			ValidateCollectionNotEmpty("Нужно задать список услуг")
		]
		public IList<InvoicePart> Parts { get; set; }

		protected override void OnDelete()
		{
			foreach (var part in Parts.Where(p => p.Ad != null))
				part.Ad.Invoice = null;
			base.OnDelete();
		}

		protected override decimal GetSum()
		{
			return PaidSum;
		}

		protected override string GetSumProperty()
		{
			return "PaidSum";
		}

		protected override void OnSave()
		{
			RegisterInvoice();
			base.OnSave();
		}

		protected override void OnUpdate()
		{
			RegisterInvoice();
			base.OnUpdate();
		}

		private void RegisterInvoice()
		{
			foreach (var part in Parts.Where(p => p.Ad != null))
				part.Ad.Invoice = this;
		}

		public string SumInWords()
		{
			return ViewHelper.InWords((float) Sum);
		}

		public List<InvoicePart> BuildParts(int invoiceGroup)
		{
			var result = new List<InvoicePart>();
			foreach (var ad in Payer.Ads.Where(a => a.Invoice == null))
				result.Add(PartForAd(ad));

			var accounts = Payer.GetAccounts().Where(a => a.InvoiceGroup == invoiceGroup);
			if (GetInvoicePeriod(Period) == InvoicePeriod.Quarter)
			{
				result.AddRange(quaterMap[Period].SelectMany(p => GetPartsForPeriod(p, accounts, GetPeriodDate(p))).ToList());
			}
			else
				result.AddRange(GetPartsForPeriod(Period, accounts, Date).ToList());
			return result;
		}

		private DateTime GetPeriodDate(Period period)
		{
			var month = ((int)period) - 3;
			var maxDays = CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(Date.Year, month);
			
			return new DateTime(Date.Year, month, Math.Min(maxDays, Date.Day));
		}

		private IEnumerable<InvoicePart> GetPartsForPeriod(Period period, IEnumerable<Account> accounts, DateTime payDate)
		{
			if (Payer.InvoiceSettings.DoNotGroupParts)
			{
				return accounts
					.Select(a => new InvoicePart(this, FormatPartDescription(a.Description, period), a.Payment, 1, payDate));
			}
			else
			{
				return accounts
					.GroupBy(a => new {a.Description, a.Payment})
					.Select(g => new InvoicePart(this, FormatPartDescription(g.Key.Description, period), g.Key.Payment, g.Count(), payDate));
			}
		}

		public string FormatPartDescription(string description, Period period)
		{
			if (IsSpecialLangCase(description))
				return description.Replace("{0}", period.GetPeriodName().ToLower());
			return description.Replace("{0}", BindingHelper.GetDescription(period).ToLower());
		}

		public bool IsSpecialLangCase(string description)
		{
			var word = description.Split(' ');
			var formatMarkIndex = Array.IndexOf(word, "{0}");
			if (formatMarkIndex < 1)
				return false;
			if (word[formatMarkIndex - 1].ToLower() == "в")
				return true;
			return false;
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
		}

		public static InvoicePeriod GetInvoicePeriod(Period period)
		{
			if (period == Period.FirstQuarter ||
				period == Period.SecondQuarter ||
				period == Period.ThirdQuarter ||
				period == Period.FourthQuarter)
				return InvoicePeriod.Quarter;
			return InvoicePeriod.Month;
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

		public void CalculateSum()
		{
			Sum = Parts.Sum(p => p.Sum);
			PaidSum = Parts.Where(p => p.Processed).Sum(p => p.Sum);
		}
	}
}