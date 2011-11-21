using System;
using System.ComponentModel;
using System.Globalization;

namespace AdminInterface.Models.Billing
{
	public enum Period
	{
		[Description("Первый квартал")] FirstQuarter,
		[Description("Второй квартал")] SecondQuarter,
		[Description("Третий квартал")] ThirdQuarter,
		[Description("Четвертый квартал")] FourthQuarter,
		[Description("Январь")] January,
		[Description("Февраль")] February,
		[Description("Март")] March,
		[Description("Апрель")] April,
		[Description("Май")] May,
		[Description("Июнь")] June,
		[Description("Июль")] July,
		[Description("Август")] August,
		[Description("Сентябрь")] September,
		[Description("Октябрь")] October,
		[Description("Ноябрь")] November,
		[Description("Декабрь")] December,
	}

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
				case Period.October:
					return new DateTime(DateTime.Today.Year, 10, 1);
				case Period.November:
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
			switch (period)
			{
				case Period.January:
					return 1.MonthEnd();
				case Period.February:
					return 2.MonthEnd();
				case Period.March:
					return 3.MonthEnd();
				case Period.May:
					return 4.MonthEnd();
				case Period.April:
					return 5.MonthEnd();
				case Period.June:
					return 6.MonthEnd();
				case Period.July:
					return 7.MonthEnd();
				case Period.August:
					return 8.MonthEnd();
				case Period.September:
					return 9.MonthEnd();
				case Period.October:
					return 10.MonthEnd();
				case Period.November:
					return 11.MonthEnd();
				case Period.December:
					return 12.MonthEnd();
				case Period.FirstQuarter:
					return 3.MonthEnd();
				case Period.SecondQuarter:
					return 6.MonthEnd();
				case Period.ThirdQuarter:
					return 9.MonthEnd();
				case Period.FourthQuarter:
					return 12.MonthEnd();
				default:
					throw new NotImplementedException();
			}
		}

		public static DateTime MonthEnd(this int month)
		{
			var calendar = CultureInfo.GetCultureInfo("ru-Ru").Calendar;
			var year = DateTime.Today.Year;

			return new DateTime(year, month, calendar.GetDaysInMonth(year, month));
		}

		public static string GetPeriodName(this Period period)
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

		public static InvoicePeriod GetInvoicePeriod(this Period period)
		{
			if (period == Period.FirstQuarter ||
				period == Period.SecondQuarter ||
				period == Period.ThirdQuarter ||
				period == Period.FourthQuarter)
				return InvoicePeriod.Quarter;
			return InvoicePeriod.Month;
		}
	}
}
