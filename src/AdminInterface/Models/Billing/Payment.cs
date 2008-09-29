using System;
using System.Globalization;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Billing
{
	public static class PeriodExtension
	{
		public static DateTime GetPeriodBegin(this Period period)
		{
			switch (period)
			{
				case Period.January:
					return new DateTime(DateTime.Today.Year, 1, 1);
				case Period.February:
					return new DateTime(DateTime.Today.Year, 2, 1);
				case Period.March:
					return new DateTime(DateTime.Today.Year, 3, 1);
				case Period.May:
					return new DateTime(DateTime.Today.Year, 4, 1);
				case Period.April:
					return new DateTime(DateTime.Today.Year, 5, 1);
				case Period.June:
					return new DateTime(DateTime.Today.Year, 6, 1);
				case Period.July:
					return new DateTime(DateTime.Today.Year, 7, 1);
				case Period.August:
					return new DateTime(DateTime.Today.Year, 8, 1);
				case Period.September:
					return new DateTime(DateTime.Today.Year, 9, 1);
				case Period.November:
					return new DateTime(DateTime.Today.Year, 10, 1);
				case Period.October:
					return new DateTime(DateTime.Today.Year, 11, 1);
				case Period.December:
					return new DateTime(DateTime.Today.Year, 12, 1);
				case Period.FirstQuarter:
					return new DateTime(DateTime.Today.Year, 1, 1);
				case Period.SecondQuarter:
					return new DateTime(DateTime.Today.Year, 4, 1);
				case Period.ThirdQuarter:
					return new DateTime(DateTime.Today.Year, 7, 1);
				case Period.FourthQuarter:
					return new DateTime(DateTime.Today.Year, 10, 1);
				default:
					throw new NotImplementedException();
			}
		}

		public static DateTime GetPeriodEnd(this Period period)
		{
			var calendar = CultureInfo.GetCultureInfo("ru-Ru").Calendar;
			var year = DateTime.Today.Year;
			switch (period)
			{
				case Period.January:
					return new DateTime(year, 1, calendar.GetDaysInMonth(year, 1));
				case Period.February:
					return new DateTime(year, 2, calendar.GetDaysInMonth(year, 2));
				case Period.March:
					return new DateTime(year, 3, calendar.GetDaysInMonth(year, 3));
				case Period.May:
					return new DateTime(year, 4, calendar.GetDaysInMonth(year, 4));
				case Period.April:
					return new DateTime(year, 5, calendar.GetDaysInMonth(year, 5));
				case Period.June:
					return new DateTime(year, 6, calendar.GetDaysInMonth(year, 6));
				case Period.July:
					return new DateTime(year, 7, calendar.GetDaysInMonth(year, 7));
				case Period.August:
					return new DateTime(year, 8, calendar.GetDaysInMonth(year, 8));
				case Period.September:
					return new DateTime(year, 9, calendar.GetDaysInMonth(year, 9));
				case Period.November:
					return new DateTime(year, 10, calendar.GetDaysInMonth(year, 10));
				case Period.October:
					return new DateTime(year, 11, calendar.GetDaysInMonth(year, 11));
				case Period.December:
					return new DateTime(year, 12, calendar.GetDaysInMonth(year, 12));
				case Period.FirstQuarter:
					return new DateTime(year, 3, calendar.GetDaysInMonth(year, 1));
				case Period.SecondQuarter:
					return new DateTime(year, 6, calendar.GetDaysInMonth(year, 4));
				case Period.ThirdQuarter:
					return new DateTime(year, 9, calendar.GetDaysInMonth(year, 7));
				case Period.FourthQuarter:
					return new DateTime(year, 12, calendar.GetDaysInMonth(year, 10));
				default:
					throw new NotImplementedException();
			}
		}
	}

	public enum PaymentType
	{
		Charge = 0,
		ChargeOff = 1,
	}

	[ActiveRecord("Payments", Schema = "Billing")]
	public class Payment : ActiveRecordBase<Payment>
	{
		public Payment()
		{}

		[Property] 
		public string Name { get; set; }

		[Property]
		public PaymentType PaymentType { get; set; }

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public DateTime PayedOn { get; set; }

		[Property]
		public float Sum { get; set; }

		[BelongsTo(Column = "PayerId")]
		public Payer Payer { get; set; }

		public static Payment[] FindBetwen(Payer payer, DateTime form, DateTime to)
		{
			return FindAll(Order.Asc("PayedOn"),
			               Expression.Between("PayedOn", form, to)
						   && Expression.Eq("Payer", payer));
		}

		public static Payment[] FindCharges(Payer payer, DateTime? from, DateTime? to)
		{
			if (from == null)
				from = payer.DefaultBeginPeriod();
			if (to == null)
				to = payer.DefaultEndPeriod();

			return FindAll(Order.Asc("PayedOn"),
						   Expression.Eq("PaymentType", PaymentType.Charge)
			               && Expression.Between("PayedOn", from, to)
						   && Expression.Eq("Payer", payer));
		}

		public static Payment[] FindChargeOffs(Payer payer, Period period)
		{
			return FindAll(Order.Asc("PayedOn"),
						   Expression.Eq("PaymentType", PaymentType.ChargeOff)
			               && Expression.Between("PayedOn", period.GetPeriodBegin(), period.GetPeriodEnd())
			               && Expression.Eq("Payer", payer));
		}

		public float Debit
		{
			get
			{
				if (PaymentType == PaymentType.ChargeOff)
					return Sum;
				return 0;
			}
		}

		public float Credit
		{
			get
			{
				if (PaymentType == PaymentType.Charge)
					return Sum;
				return 0;
			}
		}

		public static Payment ChargeOff()
		{
			return new Payment {PaymentType = PaymentType.ChargeOff};
		}

		public static Payment Charge()
		{
			return new Payment { PaymentType = PaymentType.ChargeOff, Name = "Оплата"};
		}

		public static float CreditOn(Payer payer, DateTime on)
		{
			return SumFor(on, payer, PaymentType.Charge);
		}

		private static float SumFor(DateTime on, Payer payer, PaymentType paymentType)
		{
			var val = ArHelper.WithSession(session => session.CreateQuery(@"
select sum(p.Sum)
from Payment as p 
where p.PayedOn < :on and p.Payer = :payer and p.PaymentType = :paymentType")
			                                               	.SetParameter("on", on)
			                                               	.SetParameter("payer", payer)
			                                               	.SetParameter("paymentType", paymentType)
			                                               	.UniqueResult());
			return Convert.ToSingle(val);
		}

		public static float DebitOn(Payer payer, DateTime on)
		{
			return SumFor(on, payer, PaymentType.ChargeOff);
		}
	}
}
