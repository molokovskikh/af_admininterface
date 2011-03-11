using System;
using System.IO;
using System.Linq;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PaymentFixture : IntegrationFixture
	{
		[Test]
		public void Parse_payments()
		{
			var existsPayment = Payment.Queryable.FirstOrDefault(p => p.Comment == "Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС).");
			if (existsPayment != null)
				existsPayment.DeleteAndFlush();

			var file = @"..\..\..\TestData\20110114104609.xml";
			var payments = Payment.Parse(file);
			Assert.That(payments.Count, Is.GreaterThan(0));
			var payment = payments.First();
			Assert.That(payment.Sum, Is.EqualTo(800));
			Assert.That(payment.PayedOn, Is.EqualTo(DateTime.Parse("11.01.2011")));
			Assert.That(payment.Recipient.FullName, Is.EqualTo("ООО \"Аналитический центр\""));
			Assert.That(payment.Comment, Is.EqualTo("Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС)."));

			Assert.That(payment.PayerClient.Name, Is.EqualTo("ЗАО ТРИОМЕД"));
			Assert.That(payment.PayerBank.Description, Is.EqualTo("ФИЛИАЛ ОРУ ОАО \"МИНБ\""));

			Assert.That(payment.RecipientClient.Name, Is.EqualTo("\\366601001 ООО\"Аналитический центр\""));
			Assert.That(payment.RecipientBank.Description, Is.EqualTo("ВОРОНЕЖСКИЙ Ф-Л ОАО \"ПРОМСВЯЗЬБАНК\" г ВОРОНЕЖ"));
			Assert.That(payment.Payer, Is.Not.Null);
			Assert.That(payment.Sum, Is.EqualTo(800));
			Assert.That(payment.PayedOn, Is.EqualTo(DateTime.Parse("11.01.2011")));
			Assert.That(payment.Recipient.FullName, Is.EqualTo("ООО \"Аналитический центр\""));
			Assert.That(payment.Comment, Is.EqualTo("Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС)."));
		}

		[Test]
		public void Parse_payments_without_bank_account_code()
		{
			var payments = Payment.ParseXml(File.OpenRead(@"..\..\..\TestData\201102_21.xml"));
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
			//Assert.That(payment.Payer.Id, Is.EqualTo(2072));
			//Assert.That(payment.Recipient.Id, Is.EqualTo(4));
		}

		[Test]
		public void Parser_with_output_payments()
		{
			var file = @"..\..\..\TestData\20110113.xml";
			var payments = Payment.Parse(file);
			Assert.That(payments.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Before_save_new_paymen_for_ad_create_ad()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum  = 800;
			payment.ForAd = true;
			payment.AdSum = 800;
			payment.SaveAndFlush();

			Assert.That(payment.Ad, Is.Not.Null);
			Assert.That(payment.Ad.Payer, Is.EqualTo(payer));
			Assert.That(payment.Ad.Payment, Is.EqualTo(payment));
			Assert.That(payment.Ad.Cost, Is.EqualTo(800));
		}

		[Test]
		public void Join_payment_for_exists_ad()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum  = 800;
			payment.SaveAndFlush();
			var ad = new Advertising(payer, 600);
			payer.Ads.Add(ad);
			payer.SaveAndFlush();
			payment.ForAd = true;
			payment.AdSum = 800;
			payment.SaveAndFlush();
			Assert.That(payment.Ad.Id, Is.EqualTo(ad.Id));
		}

		[Test]
		public void Update_payer_balance()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var payment = new Payment(payer);
			payment.Sum  = 800;
			payment.SaveAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(800));
		}
	}
}
