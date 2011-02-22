﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using AdminInterface.Controllers;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Helpers;

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
		public Payment(Payer payer, DateTime payedOn, decimal sum) : this(payer)
		{
			Sum = sum;
			PayedOn = payedOn;
		}

		public Payment(Payer payer)
		{
			Payer = payer;
			Recipient = payer.Recipient;
		}

		public Payment()
		{
			UpdatePayerInn = true;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		//информация ниже получается из выписки
		//фактическа дата платежа когда он прошел через банк
		[Property, ValidateNonEmpty]
		public DateTime PayedOn { get; set; }

		[Property, ValidateGreaterThanZero]
		public decimal Sum { get; set; }

		[Property]
		public string Comment { get; set; }

		[Property]
		public string DocumentNumber { get; set; }

		[Nested(ColumnPrefix = "Payer")]
		public BankClient PayerClient { get; set; }

		[Nested(ColumnPrefix = "PayerBank")]
		public BankInfo PayerBank { get; set; }

		[Nested(ColumnPrefix = "Recipient")]
		public BankClient RecipientClient { get; set; }

		[Nested(ColumnPrefix = "RecipientBank")]
		public BankInfo RecipientBank { get; set; }

		//все что выше получается из выписки
		//дата занесения платежа
		[BelongsTo(Column = "PayerId", Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }

		[BelongsTo(Column = "RecipientId")]
		public Recipient Recipient { get; set; }

		[Property]
		public DateTime RegistredOn { get; set; }

		[Property]
		public string OperatorComment { get; set; }

		[BelongsTo(Cascade = CascadeEnum.All)]
		public Advertising Ad { get; set; }

		[Property]
		public bool ForAd { get; set; }

		[Property, ValidateDecimal]
		public decimal? AdSum { get; set; }

		public bool UpdatePayerInn { get; set; }

		public void RegisterPayment()
		{
			if (Payer != null)
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

		public static List<Payment> Parse(string file)
		{
			using(var stream = File.OpenRead(file))
			{
				return Parse(file, stream);
			}
		}

		public static List<Payment> Parse(string file, Stream stream)
		{
			List<Payment> payments;
			if (Path.GetExtension(file).ToLower() == ".txt")
				payments = ParseText(stream);
			else
				payments = ParseXml(stream);

			return Identify(payments).ToList();
		}

		private static IEnumerable<Payment> Identify(IEnumerable<Payment> payments)
		{
			var recipients = Recipient.Queryable.ToList();
			foreach (var payment in payments)
			{
				payment.Recipient = recipients.FirstOrDefault(r => r.BankAccountNumber == payment.RecipientClient.AccountCode);
				if (payment.Recipient == null)
					continue;

				var inn = payment.PayerClient.Inn;
				var payer = ActiveRecordLinq.AsQueryable<Payer>().FirstOrDefault(p => p.INN == inn);
				payment.Payer = payer;

				if (payment.IsDuplicate())
					continue;

				yield return payment;
			}
		}

		public static List<Payment> ParseText(Stream file)
		{
			var reader = new StreamReader(file, Encoding.GetEncoding(1251));
			string line;
			var payments = new List<Payment>();
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Equals("СекцияДокумент=Платежное поручение", StringComparison.CurrentCultureIgnoreCase))
				{
					var payment = ParsePayment(reader);
					payments.Add(payment);
				}
			}
			return payments;
		}

		public static Payment ParsePayment(StreamReader reader)
		{
			string line;
			var payment = new Payment();
			payment.PayerClient = new BankClient();
			payment.PayerBank = new BankInfo();
			payment.RecipientBank = new BankInfo();
			payment.RecipientClient = new BankClient();
			while((line = reader.ReadLine()) != null)
			{
				if (line.Equals("КонецДокумента", StringComparison.CurrentCultureIgnoreCase))
					break;

				if (!line.Contains("="))
					continue;
				var parts = line.Split('=');
				if (parts.Length < 2)
					continue;
				var label = parts[0];
				var value = parts[1];
				if (label.Equals("Дата", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayedOn = DateTime.ParseExact(value, "dd.MM.yyyy", CultureInfo.CurrentCulture);
				}
				else if (label.Equals("Сумма", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.Sum = decimal.Parse(value, CultureInfo.InvariantCulture);
				}
				else if (label.Equals("НазначениеПлатежа", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.Comment = value;
				}
				else if (label.Equals("Номер", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.DocumentNumber = value;
				}
				else if (label.Equals("ПлательщикСчет", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerClient.AccountCode = value;
				}
				else if (label.Equals("Плательщик1", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerClient.Name = value;
				}
				else if (label.Equals("ПлательщикИНН", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerClient.Inn = value;
				}
				else if (label.Equals("ПлательщикБИК", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerBank.Bic = value;
				}
				else if (label.Equals("ПлательщикБанк1", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerBank.Description = value;
				}
				else if (label.Equals("ПлательщикКорсчет", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.PayerBank.AccountCode = value;
				}
				else if (label.Equals("ПолучательСчет", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientClient.AccountCode = value;
				}
				else if (label.Equals("Получатель1", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientClient.Name = value;
				}
				else if (label.Equals("ПолучательИНН", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientClient.Inn = value;
				}
				else if (label.Equals("ПолучательБИК", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientBank.Bic = value;
				}
				else if (label.Equals("ПолучательБанк1", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientBank.Description = value;
				}
				else if (label.Equals("ПолучательКорсчет", StringComparison.CurrentCultureIgnoreCase))
				{
					payment.RecipientBank.AccountCode = value;
				}
			}
			return payment;
		}

		public static List<Payment> ParseXml(Stream file)
		{
			var doc = XDocument.Load(file);
			var payments = new List<Payment>();
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

				payments.Add(payment);
			}

			return payments;
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

		protected override void OnUpdate()
		{
			UpdateAd();
			base.OnUpdate();
		}

		protected override bool BeforeSave(IDictionary state)
		{
			UpdateAd();
			return base.BeforeSave(state);
		}

		private void UpdateAd()
		{
			if (Payer != null
				&& ForAd
				&& this.IsChanged(p => p.ForAd))
			{
				Advertising ad = null;
				ArHelper.WithSession(s => {
					ad = s.AsQueryable<Advertising>().FirstOrDefault(a => a.Payer == Payer && a.Payment == null);
				});
				//var ad = Advertising.Queryable.FirstOrDefault(a => a.Payer == Payer && a.Payment == null);
				if (ad == null)
				{
					ad = new Advertising(Payer);
					ad.Cost = AdSum.Value;
				}
				Ad = ad;
				ad.PayedSum = AdSum;
				ad.Payment = this;
			}
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
