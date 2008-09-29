using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models.Billing
{
	[TestFixture]
	public class PaymentFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void Debit_is_a_sum_of_charge()
		{
			Payment.DeleteAll();
			var payer1 = new Payer{ ShortName = "test1" };
			var payer2 = new Payer { ShortName = "test1" };
			payer1.Save();
			payer2.Save();

			new Payment
				{
					Name = "Оплата",
					PaymentType = PaymentType.ChargeOff,
					PayedOn = DateTime.Today.AddDays(-10),
					Sum = 500,
					Payer = payer2,
				}.Save();

			new Payment
				{
					Name = "Оплата",
					PaymentType = PaymentType.ChargeOff,
					PayedOn = DateTime.Today.AddDays(-10),
					Sum = 500,
					Payer = payer1,
				}.Save();

			new Payment
			{
				Name = "Списание",
				PaymentType = PaymentType.Charge,
				PayedOn = DateTime.Today.AddDays(-5),
				Sum = 100,
				Payer = payer1,
			}.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.ChargeOff,
				PayedOn = DateTime.Today.AddDays(-5),
				Sum = 100,
				Payer = payer1,
			}.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.ChargeOff,
				PayedOn = DateTime.Today.AddDays(1),
				Sum = 100,
				Payer = payer1,
			}.Save();

			Assert.That(payer1.DebitOn(DateTime.Today), Is.EqualTo(600));

			payer1.Delete();
			payer2.Delete();
		}

		[Test]
		public void Credit_is_a_sum_of_charge_offs()
		{
			Payment.DeleteAll();
			var payer1 = new Payer { ShortName = "test1" };
			var payer2 = new Payer { ShortName = "test1" };
			payer1.Save();
			payer2.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.Charge,
				PayedOn = DateTime.Today.AddDays(-10),
				Sum = 500,
				Payer = payer2,
			}.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.Charge,
				PayedOn = DateTime.Today.AddDays(-10),
				Sum = 500,
				Payer = payer1,
			}.Save();

			new Payment
			{
				Name = "Списание",
				PaymentType = PaymentType.ChargeOff,
				PayedOn = DateTime.Today.AddDays(-5),
				Sum = 100,
				Payer = payer1,
			}.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.Charge,
				PayedOn = DateTime.Today.AddDays(-5),
				Sum = 100,
				Payer = payer1,
			}.Save();

			new Payment
			{
				Name = "Оплата",
				PaymentType = PaymentType.Charge,
				PayedOn = DateTime.Today.AddDays(1),
				Sum = 100,
				Payer = payer1,
			}.Save();

			Assert.That(payer1.CreditOn(DateTime.Today), Is.EqualTo(600));

			payer1.Delete();
			payer2.Delete();
		}
	}
}
