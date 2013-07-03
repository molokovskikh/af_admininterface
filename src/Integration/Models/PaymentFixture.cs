using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PaymentFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Parse_payments()
		{
			var existsPayment = Payment.Queryable.FirstOrDefault(p => p.Comment == "Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС).");
			if (existsPayment != null)
				existsPayment.DeleteAndFlush();

			var file = @"..\..\..\TestData\20110114104609.xml";
			var payments = Payment.ParseXml(File.OpenRead(file));
			Assert.That(payments.Count, Is.GreaterThan(0));
			var payment = payments.First();
			Assert.That(payment.Sum, Is.EqualTo(800));
			Assert.That(payment.PayedOn, Is.EqualTo(DateTime.Parse("11.01.2011")));
			Assert.That(payment.Comment, Is.EqualTo("Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС)."));

			Assert.That(payment.PayerClient.Name, Is.EqualTo("ЗАО ТРИОМЕД"));
			Assert.That(payment.PayerBank.Description, Is.EqualTo("ФИЛИАЛ ОРУ ОАО \"МИНБ\""));

			Assert.That(payment.RecipientClient.Name, Is.EqualTo("\\366601001 ООО\"Аналитический центр\""));
			Assert.That(payment.RecipientBank.Description, Is.EqualTo("ВОРОНЕЖСКИЙ Ф-Л ОАО \"ПРОМСВЯЗЬБАНК\" г ВОРОНЕЖ"));
			Assert.That(payment.Sum, Is.EqualTo(800));
			Assert.That(payment.PayedOn, Is.EqualTo(DateTime.Parse("11.01.2011")));
			Assert.That(payment.Comment, Is.EqualTo("Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС)."));
		}

		[Test]
		public void Parse_payments_without_bank_account_code()
		{
			var payments = Payment.ParseXml(File.OpenRead(@"..\..\..\TestData\201102_21.xml"));
			Assert.That(payments.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Parse_payment_without_recipient_inn()
		{
			var payments = Payment.ParseXml(File.OpenRead(@"..\..\..\TestData\20110113.xml"));
			Assert.That(payments.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Parse_raiffeisen_payments()
		{
			var payments = Payment.ParseText(File.OpenRead(@"..\..\..\TestData\1c.txt"));
			Assert.That(payments.Count, Is.GreaterThan(0));
			Assert.That(payments.Count, Is.EqualTo(4));
			var payment = payments.First();
			Assert.That(payment.Sum, Is.EqualTo(3000));
			Assert.That(payment.Comment, Is.EqualTo("Обеспечение доступа ИС услуги за 1 квартал 2011г. оплата по счету N 1815 от 11 января 2011 г. Без НДС"));
			Assert.That(payment.DocumentNumber, Is.EqualTo("18"));
			Assert.That(payment.PayedOn, Is.EqualTo(new DateTime(2011, 1, 27)));
		}

		[Test]
		public void Parse_over_document_type()
		{
			var payments = Payment.ParseText(File.OpenRead(@"..\..\..\TestData\201201_20.txt"));
			Assert.That(payments.Count, Is.EqualTo(5));
		}

		[Test]
		public void Parser_with_output_payments()
		{
			var file = @"..\..\..\TestData\20110113.xml";
			var payments = Payment.ParseXml(File.OpenRead(file));
			Assert.That(payments.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Before_save_new_paymen_for_ad_create_ad()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum = 800;
			payment.ForAd = true;
			payment.AdSum = 800;
			session.Save(payment);

			Assert.That(payment.Ad, Is.Not.Null);
			Assert.That(payment.Ad.Payer, Is.EqualTo(payer));
			Assert.That(payment.Ad.Payment, Is.EqualTo(payment));
			Assert.That(payment.Ad.Cost, Is.EqualTo(800));
		}

		[Test]
		public void Join_payment_for_exists_ad()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum = 800;
			session.Save(payment);
			var ad = new Advertising(payer, 600);
			payer.Ads.Add(ad);
			session.Save(payer);
			payment.ForAd = true;
			payment.AdSum = 800;
			session.Save(payment);
			Assert.That(payment.Ad.Id, Is.EqualTo(ad.Id));

			//хак для борьбы с хаком, строчка ниже что бы избежать ошибки
			//NHibernate.NonUniqueObjectException : a different object with the same identifier value was already associated with the session: 58, of entity: AdminInterface.Models.Billing.Advertising
			//происходит из-за открытия сессии в Payer.UpdateAd
			session.Evict(ad);
		}

		[Test]
		public void Update_payer_balance()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum = 800;
			session.Save(payment);
			Assert.That(payer.Balance, Is.EqualTo(800));
		}

		[Test]
		public void Parse_payment_without_inn()
		{
			var payments = Payment.ParseXml(File.OpenRead(@"..\..\..\TestData\\201103_04-16.03.xml"));
			Assert.That(payments.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Ignore_inn_from_black_list()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var recipient = session.Query<Recipient>().First();
			payer.INN = DataMother.RandomInn();
			new IgnoredInn(payer.INN).Save();
			session.Save(payer);

			var payments = new List<Payment> {
				new Payment {
					Sum = 800,
					RecipientClient = new Payment.BankClient(recipient.Name, recipient.INN, recipient.BankAccountNumber),
					PayerClient = new Payment.BankClient(payer.Name, payer.INN, "")
				}
			};

			var identifyPayments = Payment.Identify(payments);
			Assert.That(identifyPayments.First().Payer, Is.Null);
		}

		[Test]
		public void Identify_payment()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var recipient = session.Query<Recipient>().First();
			payer.INN = DataMother.RandomInn();
			session.Save(payer);

			var payments = new List<Payment> {
				new Payment {
					Sum = 800,
					RecipientClient = new Payment.BankClient(recipient.Name, recipient.INN, recipient.BankAccountNumber),
					PayerClient = new Payment.BankClient(payer.Name, payer.INN, "")
				}
			};

			var identifyPayments = Payment.Identify(payments);
			Assert.That(identifyPayments.First().Payer, Is.EqualTo(payer));
		}
	}
}