﻿using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;
using log4net.Config;

namespace Integration.Models
{
	[TestFixture]
	public class PayerFilterFixture : IntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			var recipient = Recipient.Queryable.First();
			payer.Recipient = recipient;
			payer.SaveAndFlush();

			var items = new PayerFilter {
				SearchBy = SearchBy.PayerId,
				SearchText = payer.Id.ToString(),
			}.Find();
			Assert.That(items.Count, Is.EqualTo(1));
			var result = items[0];
			Assert.That(result.PayerId, Is.EqualTo(payer.Id));
			Assert.That(result.Recipient, Is.EqualTo(recipient.Name));
		}

		[Test]
		public void Status_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			supplier.Save();
			var items = new PayerFilter{
				SearchBy = SearchBy.PayerId,
				SearchText = payer.Id.ToString(),
			}.Find();
			Assert.That(items.Count, Is.EqualTo(1));
			var result = items[0];
			Assert.That(result.PayerId, Is.EqualTo(payer.Id));
			Assert.That(result.IsDisabled, Is.False);
		}

		[Test]
		public void Search_by_type()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			supplier.Save();

			var items = new PayerFilter {
				SearchBy = SearchBy.Name,
				ClientType = SearchClientType.Supplier
			}.Find();
			Assert.That(items.Count, Is.GreaterThan(0));
			var result = items.FirstOrDefault(i => i.PayerId == payer.Id);
			Assert.That(result, Is.Not.Null, "не нашли плательщика {0}", payer.Id);
		}

		[Test]
		public void Search_by_recipient_id()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			var recipient = Recipient.Queryable.First();
			payer.Recipient = recipient;
			payer.Save();

			var items = new PayerFilter{
				Recipient = Recipient.Queryable.First()
			}.Find();
			Assert.That(items.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Search_payer_by_invoice_type()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			payer.AutoInvoice = InvoiceType.Auto;
			payer.SaveAndFlush();

			var items = new PayerFilter{InvoiceType = InvoiceType.Manual}.Find();
			Assert.That(items.Any(i => i.PayerId == payer.Id), Is.False,
				"не должны были найти плательщика {0}, есть {1}", payer.Id, items.Implode(i => i.PayerId.ToString()));
		}

		[Test]
		public void Search_by_inn()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			payer.INN = DataMother.RandomInn();
			payer.SaveAndFlush();

			var items = new PayerFilter{SearchText = payer.INN, SearchBy = SearchBy.Inn}.Find();
			Assert.That(items.Count, Is.EqualTo(1));
			Assert.That(items[0].PayerId, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Search_by_address()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var payer = client.Payers.First();
			var address = client.Addresses.First();
			address.Value = address.Value + " " + address.Id;
			address.SaveAndFlush();
			client.SaveAndFlush();

			var items = new PayerFilter{SearchText = address.Value, SearchBy = SearchBy.Address}.Find();
			Assert.That(items.Count, Is.EqualTo(1));
			Assert.That(items[0].PayerId, Is.EqualTo(payer.Id));
		}
	}
}