using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AdminInterface.Controllers;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Tools;

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
		{
			UpdatePayerInn = true;
		}

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

		[Property]
		public string DocumentNumber { get; set; }

		public bool UpdatePayerInn { get; set; }

		[BelongsTo(Column = "PayerId", Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }

		[BelongsTo(Column = "RecipientId")]
		public Recipient Recipient { get; set; }

		[Nested(ColumnPrefix = "Payer")]
		public BankClient PayerClient { get; set; }

		[Nested(ColumnPrefix = "PayerBank")]
		public BankInfo PayerBank { get; set; }

		[Nested(ColumnPrefix = "Recipient")]
		public BankClient RecipientClient { get; set; }

		[Nested(ColumnPrefix = "RecipientBank")]
		public BankInfo RecipientBank { get; set; }

		public void RegisterPayment()
		{
			Payer.Balance += Sum;
			RegistredOn = DateTime.Now;
		}

		public string GetWarning()
		{
			if (Payer != null
				|| PayerClient == null)
				return "";

			var payers = ActiveRecordLinq.AsQueryable<Payer>().Where(p => p.INN == PayerClient.Inn).ToList();
			if (payers.Count == 0)
			{
				return String.Format("Не удалось найти ни одного платильщика с ИНН {0}", PayerClient.Inn);
			}
			else if (payers.Count == 1)
			{
				Payer = payers.Single();
				return "";
			}
			else
			{
				return String.Format("Найдено более одного плательщика с ИНН {0}, плательщики с таким ИНН {1}",
					PayerClient.Inn,
					payers.Implode(p => p.Name));
			}
		}

		public static List<Payment> ParsePayment(string file)
		{
			using(var stream = File.OpenRead(file))
				return ParsePayment(stream);
		}

		public static List<Payment> ParsePayment(Stream file)
		{
			var recipients = Recipient.Queryable.ToList();
			var doc = XDocument.Load(file);
			var list = new List<Payment>();
			foreach (var node in doc.XPathSelectElements("//payment"))
			{
				var documentNumber = node.XPathSelectElement("NDoc").Value;
				var dateNode = node.XPathSelectElement("SendDate");
				if (dateNode == null)
					continue;
				var sum = node.XPathSelectElement("Summa").Value;
				var comment = node.XPathSelectElement("AssignPayment").Value;

				var payment = new Payment {
					DocumentNumber = documentNumber,
					PayedOn = DateTime.Parse(dateNode.Value, CultureInfo.GetCultureInfo("ru-RU")),
					RegistredOn = DateTime.Now,
					Sum = Decimal.Parse(sum, CultureInfo.InvariantCulture),
					Comment = comment,

					PayerBank = new BankInfo(
						node.XPathSelectElement("BankPayer/Description").Value,
						node.XPathSelectElement("BankPayer/BIC").Value,
						node.XPathSelectElement("BankPayer/AccountCode").Value
					),
					PayerClient = new BankClient(
						node.XPathSelectElement("Payer/Name").Value,
						node.XPathSelectElement("Payer/INN").Value,
						node.XPathSelectElement("Payer/AccountCode").Value
					),
					RecipientBank = new BankInfo(
						node.XPathSelectElement("BankRecipient/Description").Value,
						node.XPathSelectElement("BankRecipient/BIC").Value,
						node.XPathSelectElement("BankRecipient/AccountCode").Value
					),
					RecipientClient = new BankClient(
						node.XPathSelectElement("Recepient/Client/Name").Value,
						node.XPathSelectElement("Recepient/Client/INN").Value,
						node.XPathSelectElement("Recepient/Client/AccountCode").Value
					),
				};

				payment.Recipient = recipients.FirstOrDefault(r => r.BankAccountNumber == payment.RecipientClient.AccountCode);
				if (payment.Recipient == null)
					continue;

				var inn = payment.PayerClient.Inn;
				var payer = ActiveRecordLinq.AsQueryable<Payer>().FirstOrDefault(p => p.INN == inn);
				payment.Payer = payer;

				if (payment.IsDuplicate())
					continue;

				list.Add(payment);
			}

			return list;
		}

		public class BankInfo
		{
			[Property]
			public string Description { get; set; }
			[Property]
			public string Bic { get; set; }
			[Property]
			public string AccountCode { get; set; }

			public BankInfo()
			{}

			public BankInfo(string description, string bic, string accountCode)
			{
				Description = description;
				Bic = bic;
				AccountCode = accountCode;
			}
		}

		public class BankClient
		{
			[Property]
			public string Inn { get; set; }
			[Property]
			public string Name { get; set; }
			[Property]
			public string AccountCode { get; set; }

			public BankClient()
			{}

			public BankClient(string name, string inn, string accountCode)
			{
				Name = name;
				Inn = inn;
				AccountCode = accountCode;
			}
		}

		[ValidateSelf]
		public void Validate(ErrorSummary summary)
		{
			if (Recipient.Id != Payer.Recipient.Id)
				summary.RegisterErrorMessage(
					"Recipient",
					"Получатель платежей плательщика должен соответствовать получателю платежей выбранном в платеже");
		}

		private bool IsDuplicate()
		{
			if (Payer == null)
				return false;

			return Queryable.FirstOrDefault(p => p.Payer == Payer
				&& p.PayedOn == PayedOn
				&& p.Sum == Sum
				&& p.DocumentNumber == DocumentNumber) != null;
		}

		public void Cancel()
		{
			if (Payer != null)
				Payer.Balance -= Sum;

			Payer.Update();
			Delete();
		}

		public void DoUpdate()
		{
			UpdateInn();

			var oldPayer = this.OldValue(p => p.Payer);
			var oldSum = this.OldValue(p => p.Sum);
			if (this.IsChanged(p => p.Payer) || this.IsChanged(p => p.Sum))
			{
				if (oldPayer != null)
				{
					oldPayer.Balance -= oldSum;
					oldPayer.Save();
				}
				if (Payer != null)
				{
					Payer.Balance += Sum;
					Payer.Update();
				}
			}
		}

		public void UpdateInn()
		{
			if (!UpdatePayerInn)
				return;

			if (Payer != null
				&& PayerClient != null
				&& !String.IsNullOrEmpty(PayerClient.Inn))
			{
				Payer.INN = PayerClient.Inn;
				Payer.Save();
			}
		}
	}
}
