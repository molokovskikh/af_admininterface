﻿using System;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Models.Audit;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PayerFixture : AdmIntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			payer.ShortName = String.Format("Тестовый плательщик {0}", payer.Id);
			session.Save(payer);
			session.Flush();

			var payers = Payer.GetLikeAvaliable(session, payer.ShortName);
			Assert.That(payers.Count(), Is.EqualTo(1));
			Assert.That(payers.Single().Id, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Search_payet_with_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var payer = supplier.Payer;
			payer.ShortName = String.Format("Тестовый плательщик {0}", payer.Id);
			Save(payer);
			Flush();

			var payers = Payer.GetLikeAvaliable(session, payer.ShortName);
			Assert.That(payers.Count(), Is.EqualTo(1));
			Assert.That(payers.Single().Id, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Before_save_if_begin_balance_changed_update_balance()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();

			payer.BeginBalance = 1000;
			session.Save(payer);
			session.Flush();
			Assert.That(payer.Balance, Is.EqualTo(1000));
			payer.Balance = 2000;
			payer.BeginBalance = -500;
			session.Save(payer);
			session.Flush();
			Assert.That(payer.Balance, Is.EqualTo(500));
		}

		[Test]
		public void Log_payment_changes()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			user.Accounting.Payment = 200;
			session.Save(user);
			Flush();

			var payer = client.Payers[0];
			var records = PayerAuditRecord.Find(payer);
			Assert.That(records.Count, Is.EqualTo(1));
			var record = records[0];
			Assert.That(record.Message, Is.EqualTo("Изменено 'Платеж' было '800' стало '200'"));
			Assert.That(record.Payer, Is.EqualTo(payer));
			Assert.That(record.ObjectId, Is.EqualTo(user.Id));
			Assert.That(record.ObjectType, Is.EqualTo(LogObjectType.User));
			Assert.That(record.Name, Is.EqualTo("test"));
		}

		[Test]
		public void Include_report_into_balance_calculation()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			var report = DataMother.Report(payer);
			report.Payment = 1500;

			session.Save(client);
			session.Save(payer);
			session.Save(report);

			session.Refresh(payer);
			Assert.That(payer.TotalSum, Is.EqualTo(1500));
		}

		[Test]
		public void Delete_payer()
		{
			var client = DataMother.TestClient();
			client.Disabled = true;
			var payer = client.Payers.First();
			Assert.That(payer.CanDelete(session), Is.True);

			payer.Delete(session);
			Flush();
			Reopen();

			Assert.That(session.Get<Payer>(payer.Id), Is.Null);
		}
	}
}