﻿using System;
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
			SetPayer(payer);
			Period = period;
			Sum = payer.TotalSum;
			Date = date;
			CreatedOn = DateTime.Now;
			if (GetInvoicePeriod(Period) == InvoicePeriod.Quarter)
				Sum *= 3;
			Parts = BuildParts();
		}

		public void SetPayer(Payer payer)
		{
			Recipient = payer.Recipient;
			Payer = payer;
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

		[Property(NotNull = true, Default = "0")]
		public bool SendToEmail { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.All)]
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
	}

	[ActiveRecord(Schema = "billing")]
	public class InvoicePart
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public decimal Cost { get; set; }

		[Property]
		public int Count { get; set; }

		[BelongsTo]
		public Invoice Invoice { get; set; }

		public decimal Sum
		{
			get
			{
				return Cost * Count;
			}
		}

		public InvoicePart()
		{}

		public InvoicePart(Invoice invoice, Period period, decimal cost, int count)
		{
			Invoice = invoice;
			if (invoice.Recipient.Id == 4)
				Name = String.Format("Обеспечение доступа к ИС (мониторингу фармрынка) в {0}", GetPeriodName(period));
			else
				Name = String.Format("Мониторинг оптового фармрынка за {0}", BindingHelper.GetDescription(period).ToLower());
			Cost = cost;
			Count = count;
		}

		public string GetPeriodName(Period period)
		{
			switch (period)
			{
				case Period.January:
					return "январе";
				case Period.February:
					return "феврале";
				case Period.March:
					return "марте";
				case Period.April:
					return "апреле";
				case Period.August:
					return "августе";
				case Period.December:
					return "декабре";
				case Period.July:
					return "июле";
				case Period.June:
					return "июне";
				case Period.May:
					return "мае";
				case Period.November:
					return "ноябре";
				case Period.October:
					return "октябре";
				case Period.September:
					return "сентябре";
				case Period.FirstQuarter:
					return "первом квартале";
				case Period.SecondQuarter:
					return "втором квартале";
				case Period.ThirdQuarter:
					return "третьем квартале";
				case Period.FourthQuarter:
					return "четвертом квартале";
				default:
					throw new Exception(String.Format("не знаю что за период такой {0}", period));
			}
		}
	}
}