using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class PaymentsControllerFixture : ControllerFixture
	{
		private PaymentsController controller;

		[SetUp]
		public new void Setup()
		{
			controller = new PaymentsController();
			Prepare(controller);
		}

		//люди нетерпиливы и два раза нажимаю на кнопку сохранить
		//в этом случае мы не должны два раза зачислять платежи
		[Test]
		public void Double_saved_payments_processing()
		{
			var payers = ActiveRecordLinqBase<Payer>.Queryable.Where(p => p.INN == "361911638854").ToList();
			payers.Each(p => {
				p.INN = null;
				p.Save();
			});

			var payer = DataMother.CreatePayerForBillingDocumentTest();
			payer.INN = "361911638854";
			payer.SaveAndFlush();

			var file = "../../../TestData/1c.txt";
			using (var stream = File.OpenRead(file))
				Context.Session["payments"] = Payment.Parse(file, stream);

			Reopen();
			controller.SavePayments();

			try {
				Reopen();
				controller.SavePayments();
			}
			catch (SessionExpiredException) {
			}

			Reopen();
			payer = Payer.Find(payer.Id);
			Assert.That(payer.Balance, Is.EqualTo(3000));
		}
	}
}