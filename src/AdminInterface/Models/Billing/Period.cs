using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using ExcelLibrary.BinaryFileFormat;

namespace AdminInterface.Models.Billing
{
	public class Period : ICloneable, ICompisite
	{
		public static int StartYear = 2011;

		public static int[] Years
		{
			get
			{
				var yearCount = DateTime.Now.Year - StartYear + 2;
				return Enumerable.Range(StartYear, yearCount).ToArray();
			}
		}

		public Period()
		{
			Year = DateTime.Now.Year;
			Interval = Interval.FirstQuarter;
		}

		public Period(DateTime dateTime)
		{
			Year = dateTime.Year;
			Interval = (Interval)dateTime.Month + 3;
		}

		public Period(string dbValue)
		{
			var parts = dbValue.Split('-');
			Year = int.Parse(parts[0]);
			Interval = (Interval)int.Parse(parts[1]);
		}

		public Period(int year, Interval interval)
		{
			Year = year;
			Interval = interval;
		}

		public int Year { get; set; }
		public Interval Interval { get; set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(Period)) return false;
			return Equals((Period)obj);
		}

		public bool Equals(Period other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Year == Year && Equals(other.Interval, Interval);
		}

		public override int GetHashCode()
		{
			unchecked {
				return (Year * 397) ^ Interval.GetHashCode();
			}
		}

		public IEnumerable<Tuple<string, object>> Elements()
		{
			return new[] {
				new Tuple<string, object>("Year", Year),
				new Tuple<string, object>("Interval", (int)Interval),
			};
		}

		public object Clone()
		{
			return new Period(Year, Interval);
		}

		public string ToSqlString()
		{
			return String.Format("{0}-{1}", Year, (int)Interval);
		}

		public override string ToString()
		{
			return String.Format("{0}.{1}", Year, BindingHelper.GetDescription(Interval));
		}

		public static Period Parse(string input, out bool success)
		{
			success = false;
			var parts = input.Split('.');
			if (parts.Length < 2)
				return null;

			int year;
			if (!int.TryParse(parts[0], out year))
				return null;

			var descriptions = BindingHelper.GetDescriptionsDictionary(typeof(Interval));
			var pair = descriptions.FirstOrDefault(p => String.Equals(p.Value, parts[1], StringComparison.CurrentCultureIgnoreCase));
			if (pair.Key == null)
				return null;

			var period = new Period(year, (Interval)pair.Key);
			success = true;
			return period;
		}
	}

	public enum Interval
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
			int year = period.Year;
			switch (period.Interval) {
				case Interval.January:
					return new DateTime(year, 1, 1);
				case Interval.February:
					return new DateTime(year, 2, 1);
				case Interval.March:
					return new DateTime(year, 3, 1);
				case Interval.May:
					return new DateTime(year, 5, 1);
				case Interval.April:
					return new DateTime(year, 4, 1);
				case Interval.June:
					return new DateTime(year, 6, 1);
				case Interval.July:
					return new DateTime(year, 7, 1);
				case Interval.August:
					return new DateTime(year, 8, 1);
				case Interval.September:
					return new DateTime(year, 9, 1);
				case Interval.October:
					return new DateTime(year, 10, 1);
				case Interval.November:
					return new DateTime(year, 11, 1);
				case Interval.December:
					return new DateTime(year, 12, 1);
				case Interval.FirstQuarter:
					return new DateTime(year, 1, 1);
				case Interval.SecondQuarter:
					return new DateTime(year, 4, 1);
				case Interval.ThirdQuarter:
					return new DateTime(year, 7, 1);
				case Interval.FourthQuarter:
					return new DateTime(year, 10, 1);
				default:
					throw new NotImplementedException();
			}
		}

		public static DateTime GetPeriodEnd(this Period period)
		{
			if (period.GetInvoicePeriod() == InvoicePeriod.Month)
				return period.GetPeriodBegin().LastDayOfMonth().Add(new TimeSpan(23, 59, 59));
			else
				return period.GetPeriodBegin().AddMonths(3).AddDays(-1).Add(new TimeSpan(23, 59, 59));
		}

		public static DateTime MonthEnd(this int month)
		{
			var calendar = CultureInfo.GetCultureInfo("ru-Ru").Calendar;
			var year = DateTime.Today.Year;

			return new DateTime(year, month, calendar.GetDaysInMonth(year, month));
		}

		public static string GetPeriodName(this Period period)
		{
			switch (period.Interval) {
				case Interval.January:
					return "январе";
				case Interval.February:
					return "феврале";
				case Interval.March:
					return "марте";
				case Interval.April:
					return "апреле";
				case Interval.August:
					return "августе";
				case Interval.December:
					return "декабре";
				case Interval.July:
					return "июле";
				case Interval.June:
					return "июне";
				case Interval.May:
					return "мае";
				case Interval.November:
					return "ноябре";
				case Interval.October:
					return "октябре";
				case Interval.September:
					return "сентябре";
				case Interval.FirstQuarter:
					return "первом квартале";
				case Interval.SecondQuarter:
					return "втором квартале";
				case Interval.ThirdQuarter:
					return "третьем квартале";
				case Interval.FourthQuarter:
					return "четвертом квартале";
				default:
					throw new Exception(String.Format("не знаю что за период такой {0}", period));
			}
		}

		public static InvoicePeriod GetInvoicePeriod(this Period period)
		{
			if (period.Interval == Interval.FirstQuarter ||
				period.Interval == Interval.SecondQuarter ||
				period.Interval == Interval.ThirdQuarter ||
				period.Interval == Interval.FourthQuarter)
				return InvoicePeriod.Quarter;
			return InvoicePeriod.Month;
		}

		private static Dictionary<Interval, Interval[]> quaterMap = new Dictionary<Interval, Interval[]> {
			{ Interval.FirstQuarter, new[] { Interval.January, Interval.February, Interval.March } },
			{ Interval.SecondQuarter, new[] { Interval.April, Interval.May, Interval.June } },
			{ Interval.ThirdQuarter, new[] { Interval.July, Interval.August, Interval.September } },
			{ Interval.FourthQuarter, new[] { Interval.October, Interval.November, Interval.December } }
		};

		public static Period[] Months(this Period period)
		{
			if (!quaterMap.ContainsKey(period.Interval))
				return new[] { period };
			return quaterMap[period.Interval].Select(i => new Period(period.Year, i)).ToArray();
		}
	}
}