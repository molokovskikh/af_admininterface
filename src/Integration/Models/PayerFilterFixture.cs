using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class PayerFilterFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			var recipient = session.Query<Recipient>().First();
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
			Save(supplier);
			Flush();

			var items = new PayerFilter {
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
			Save(supplier);
			Flush();

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
			var recipient = session.Query<Recipient>().First();
			payer.Recipient = recipient;
			payer.Save();

			var items = new PayerFilter {
				Recipient = session.Query<Recipient>().First()
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

			var items = new PayerFilter { InvoiceType = InvoiceType.Manual }.Find();
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

			var items = new PayerFilter { SearchText = payer.INN, SearchBy = SearchBy.Inn }.Find();
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
			session.SaveOrUpdate(client);

			var items = new PayerFilter { SearchText = address.Value, SearchBy = SearchBy.Address }.Find();
			Assert.That(items.Count, Is.EqualTo(1));
			Assert.That(items[0].PayerId, Is.EqualTo(payer.Id));
		}

		[Test]
		public void Search_payer_without_document()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();

			var period = DateTime.Now.ToPeriod();
			var filter = new PayerFilter { SearchWithoutDocuments = true, Period = period, DocumentType = DocumentType.Invoice };
			var items = filter.Find();

			Assert.Contains(payer.Id, items.Select(i => i.PayerId).ToArray());

			Save(payer.BuildInvoices(DateTime.Now, period));
			Flush();

			items = filter.Find();
			Assert.That(items.Select(i => i.PayerId).ToArray(), Is.Not.Contains(payer.Id));
		}

		[Test]
		public void Do_not_include_payer_that_was_not_registred()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();

			var period = DateTime.Now.AddMonths(-1).ToPeriod();
			var filter = new PayerFilter { SearchWithoutDocuments = true, Period = period, DocumentType = DocumentType.Invoice };
			var items = filter.Find();

			Assert.That(items.Select(i => i.PayerId).ToArray(), Is.Not.Contains(payer.Id));
		}

		[Test]
		public void If_payer_have_report_it_not_disabled()
		{
			var payer = DataMother.CreatePayer();
			var report = DataMother.Report(payer);

			Save(payer);
			Save(report);

			Flush();

			var filter = new PayerFilter();
			var items = filter.Find();

			var item = items.FirstOrDefault(i => payer.Id == i.PayerId);
			Assert.That(item, Is.Not.Null, "не удалось найти плателщика {0} нашли {1}", payer.Id, items.Implode());
			Assert.That(item.IsDisabled, Is.False);
		}
	}
}