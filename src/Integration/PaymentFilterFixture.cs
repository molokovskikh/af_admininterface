using System;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using Integration.Models;
using log4net.Config;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class PaymentFilterFixture : IntegrationFixture
	{
		[Test]
		public void Search_by_payer_name()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			payer.Name = "Тестовый плательщик " + payer.Id;
			var payment = new Payment(payer, DateTime.Now, 800);
			payment.SaveAndFlush();
			var filter = new PaymentFilter {
				SearchText = "Тестовый плательщик " + payer.Id
			};
			var payments = filter.Find();
			Assert.That(payments.Count, Is.EqualTo(1));
			Assert.That(payments[0].Id, Is.EqualTo(payment.Id));
		}
	}
}