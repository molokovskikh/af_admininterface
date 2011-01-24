using System;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "billing")]
	public class InvoicePart
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property, ValidateNonEmpty]
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