﻿using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PayerFixture : IntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			payer.ShortName = String.Format("Тестовый плательщик {0}", payer.Id);
			payer.UpdateAndFlush();

			var payers = Payer.GetLikeAvaliable(payer.ShortName);
			Assert.That(payers.Count(), Is.EqualTo(1));
			Assert.That(payers.Single().Id, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Search_payet_with_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			payer.ShortName = String.Format("Тестовый плательщик {0}", payer.Id);
			supplier.Save();
			payer.UpdateAndFlush();

			var payers = Payer.GetLikeAvaliable(payer.ShortName);
			Assert.That(payers.Count(), Is.EqualTo(1));
			Assert.That(payers.Single().Id, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Before_save_if_begin_balance_changed_update_balance()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();

			payer.BeginBalance = 1000;
			payer.SaveAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(1000));
			payer.Balance = 2000;
			payer.BeginBalance = -500;
			payer.SaveAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(500));
		}
	}
}