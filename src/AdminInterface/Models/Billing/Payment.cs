using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

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
				case Period.November:
					return 10.MonthEnd();
				case Period.October:
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

			return new DateTime(year, 9, calendar.GetDaysInMonth(year, 9));
		}
	}

	public enum PaymentType
	{
		Charge = 0,
		ChargeOff = 1,
	}

	[ActiveRecord("Payments", Schema = "Billing")]
	public class Payment : ActiveRecordLinqBase<Payment>
	{
		public Payment()
		{}

		[PrimaryKey]
		public uint Id { get; set; }

		//фактическа дата платежа когда он прошел через банк
		[Property]
		public DateTime PayedOn { get; set; }

		//дата занесения платежа
		[Property]
		public DateTime RegistredOn { get; set; }

		[Property]
		public decimal Sum { get; set; }

		[Property]
		public string Comment { get; set; }

		[BelongsTo(Column = "PayerId", Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }

		[BelongsTo(Column = "RecipientId")]
		public Recipient Recipient { get; set; }

		public void RegisterPayment()
		{
			Payer.Balance += Sum;
			RegistredOn = DateTime.Now;
		}

		public static List<Payment> ParsePayment(string file)
		{
			using(var stream = File.OpenRead(file))
				return ParsePayment(stream);
		}

		public static List<Payment> ParsePayment(Stream file)
		{
			var doc = XDocument.Load(file);
			var accountNumber = doc.XPathSelectElement("XMLExport/accDescr/AccountCode").Value;
			var recipient = Recipient.Queryable.FirstOrDefault(r => r.BankAccountNumber == accountNumber);
			var list = new List<Payment>();
			foreach (var node in doc.XPathSelectElements("//payment"))
			{
				var date = node.XPathSelectElement("DatePorucheniya").Value;
				var sum = node.XPathSelectElement("Summa").Value;
				var comment = node.XPathSelectElement("AssignPayment").Value;
				var inn = node.XPathSelectElement("Payer/INN").Value;
				var payer = ActiveRecordLinq.AsQueryable<Payer>().FirstOrDefault(p => p.INN == inn);

				list.Add(new Payment {
					PayedOn = DateTime.Parse(date, CultureInfo.GetCultureInfo("ru-RU")),
					RegistredOn = DateTime.Now,
					Sum = Decimal.Parse(sum, CultureInfo.InvariantCulture),
					Comment = comment,
					Payer = payer,
					Recipient = recipient
				});
			}

			return list;
		}
	}
}
