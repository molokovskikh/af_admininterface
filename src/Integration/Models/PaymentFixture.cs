using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PaymentFixture
	{
		[Test]
		public void Parse_payments()
		{
			using (new SessionScope())
			{
				var file = @"..\..\..\TestData\20110114104609.xml";
				var payments = Payment.ParsePayment(file);
				Assert.That(payments.Count, Is.GreaterThan(0));
				var payment = payments.First();
				Assert.That(payment.Sum, Is.EqualTo(800));
				Assert.That(payment.PayedOn, Is.EqualTo(DateTime.Parse("11.01.2011")));
				Assert.That(payment.Recipient.FullName, Is.EqualTo("ООО \"Аналитический центр\""));
				Assert.That(payment.Comment, Is.EqualTo("Оплата за мониторинг оптового фармрынка за январь по счету №161 от 11.01..2011г Cумма 800-00,без налога (НДС)."));
			}
		}
	}
}
