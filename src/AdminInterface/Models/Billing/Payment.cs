﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using AdminInterface.MonoRailExtentions;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Payments", Schema = "Billing")]
	public class Payment : BalanceUpdater<Payment>
	{
		private decimal _sum;

		public Payment(Payer payer, DateTime payedOn, decimal sum) : this(payer)
		{
			Sum = sum;
			PayedOn = payedOn;
		}

		public Payment(Payer payer)
			: this()
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
		//фактическая дата платежа когда он прошел через банк
		[Property, ValidateNonEmpty]
		public DateTime PayedOn { get; set; }

		[Property, ValidateGreaterThanZero]
		public virtual decimal Sum
		{
			get { return _sum; }
			set
			{
				_sum = value;
				BalanceAmount = value;
			}
		}

		[Property]
		public override decimal BalanceAmount { get; protected set; }

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
		public override Payer Payer { get; set; }

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
			RegistredOn = DateTime.Now;
		}

		public string GetWarning()
		{
			if (Payer != null
				|| PayerClient == null)
				return "";

			var payers = ActiveRecordLinq.AsQueryable<Payer>().Where(p => p.INN == PayerClient.Inn).ToList();
			if (payers.Count == 0) {
				return String.Format("Не удалось найти ни одного плательщика с ИНН {0}", PayerClient.Inn);
			}
			else if (payers.Count == 1) {
				Payer = payers.Single();
				return "";
			}
			else {
				return String.Format("Найдено более одного плательщика с ИНН {0}, плательщики с таким ИНН {1}",
					PayerClient.Inn,
					payers.Implode(p => p.Name));
			}
		}

		public static List<Payment> Parse(string file)
		{
			using (var stream = File.OpenRead(file)) {
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

		public static IEnumerable<Payment> Identify(IEnumerable<Payment> payments)
		{
			var recipients = ActiveRecordLinqBase<Recipient>.Queryable.ToList();
			var ignoredInns = IgnoredInn.Queryable.ToList();
			foreach (var payment in payments) {
				payment.Recipient = recipients.FirstOrDefault(r => r.BankAccountNumber == payment.RecipientClient.AccountCode);
				if (payment.Recipient == null)
					continue;

				var inn = payment.PayerClient.Inn;
				if (!ignoredInns.Any(i => String.Equals(i.Name, inn, StringComparison.InvariantCultureIgnoreCase))) {
					var payer = ActiveRecordLinq.AsQueryable<Payer>().FirstOrDefault(p => p.INN == inn);
					payment.Payer = payer;
				}

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
			while ((line = reader.ReadLine()) != null) {
				if (line.Equals("СекцияДокумент=Платежное поручение", StringComparison.CurrentCultureIgnoreCase)
					|| line.Equals("СекцияДокумент=Прочее", StringComparison.CurrentCultureIgnoreCase)) {
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
			while ((line = reader.ReadLine()) != null) {
				if (line.Equals("КонецДокумента", StringComparison.CurrentCultureIgnoreCase))
					break;

				if (!line.Contains("="))
					continue;
				var parts = line.Split('=');
				if (parts.Length < 2)
					continue;
				var label = parts[0];
				var value = parts[1];
				if (label.Equals("ДатаПоступило", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayedOn = DateTime.ParseExact(value, "dd.MM.yyyy", CultureInfo.CurrentCulture);
				}
				else if (label.Equals("Сумма", StringComparison.CurrentCultureIgnoreCase)) {
					payment.Sum = decimal.Parse(value, CultureInfo.InvariantCulture);
				}
				else if (label.Equals("НазначениеПлатежа", StringComparison.CurrentCultureIgnoreCase)) {
					payment.Comment = value;
				}
				else if (label.Equals("Номер", StringComparison.CurrentCultureIgnoreCase)) {
					payment.DocumentNumber = value;
				}
				else if (label.Equals("ПлательщикСчет", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerClient.AccountCode = value;
				}
				else if (label.Equals("Плательщик1", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerClient.Name = value;
				}
				else if (label.Equals("ПлательщикИНН", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerClient.Inn = value;
				}
				else if (label.Equals("ПлательщикБИК", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerBank.Bic = value;
				}
				else if (label.Equals("ПлательщикБанк1", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerBank.Description = value;
				}
				else if (label.Equals("ПлательщикКорсчет", StringComparison.CurrentCultureIgnoreCase)) {
					payment.PayerBank.AccountCode = value;
				}
				else if (label.Equals("ПолучательСчет", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientClient.AccountCode = value;
				}
				else if (label.Equals("Получатель1", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientClient.Name = value;
				}
				else if (label.Equals("ПолучательИНН", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientClient.Inn = value;
				}
				else if (label.Equals("ПолучательБИК", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientBank.Bic = value;
				}
				else if (label.Equals("ПолучательБанк1", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientBank.Description = value;
				}
				else if (label.Equals("ПолучательКорсчет", StringComparison.CurrentCultureIgnoreCase)) {
					payment.RecipientBank.AccountCode = value;
				}
			}
			return payment;
		}

		public static List<Payment> ParseXml(Stream file)
		{
			var doc = XDocument.Load(file);
			var payments = new List<Payment>();
			foreach (var node in doc.XPathSelectElements("//payment")) {
				var documentNumber = node.XPathSelectElement("NDoc").Value;
				var dateNode = node.XPathSelectElement("SendDate");
				if (dateNode == null)
					continue;
				var sum = node.XPathSelectElement("Summa").Value;
				var comment = node.XPathSelectElement("AssignPayment").Value;

				//если платеж из банка россии (ЦБ) то у него нет корсчета
				var payment = new Payment {
					DocumentNumber = documentNumber,
					PayedOn = DateTime.Parse(dateNode.Value, CultureInfo.GetCultureInfo("ru-RU")),
					RegistredOn = DateTime.Now,
					Sum = Decimal.Parse(sum, CultureInfo.InvariantCulture),
					Comment = comment,
					PayerBank = new BankInfo(
						(string)node.XPathSelectElement("BankPayer/Description"),
						(string)node.XPathSelectElement("BankPayer/BIC"),
						(string)node.XPathSelectElement("BankPayer/AccountCode")),
					PayerClient = new BankClient(
						(string)node.XPathSelectElement("Payer/Name"),
						(string)node.XPathSelectElement("Payer/INN"),
						(string)node.XPathSelectElement("Payer/AccountCode")),
					RecipientBank = new BankInfo(
						(string)node.XPathSelectElement("BankRecipient/Description"),
						(string)node.XPathSelectElement("BankRecipient/BIC"),
						(string)node.XPathSelectElement("BankRecipient/AccountCode")),
					RecipientClient = new BankClient(
						(string)node.XPathSelectElement("Recepient/Client/Name"),
						(string)node.XPathSelectElement("Recepient/Client/INN"),
						(string)node.XPathSelectElement("Recepient/Client/AccountCode")),
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
			{
			}

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
			{
			}

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
			if (Recipient == null
				|| (Payer != null && Payer.Recipient == null))
				summary.RegisterErrorMessage(
					"Recipient",
					"Получатель платежа не установлен");

			if (Recipient != null && Payer != null && Payer.Recipient != null
				&& Recipient.Id != Payer.Recipient.Id)
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

		public void DoUpdate()
		{
			UpdateInn();
		}

		public void UpdateInn()
		{
			if (!UpdatePayerInn)
				return;

			if (Payer != null
				&& PayerClient != null
				&& !String.IsNullOrEmpty(PayerClient.Inn)) {
				Payer.INN = PayerClient.Inn;
				Payer.Save();
			}
		}
	}
}