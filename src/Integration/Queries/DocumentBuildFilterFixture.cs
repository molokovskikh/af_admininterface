using System;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class DocumentBuildFilterFixture : AdmIntegrationFixture
	{
		private Payer payer;
		private DocumentBuilderFilter filter;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			payer.AutoInvoice = InvoiceType.Auto;
			session.Save(payer);
			Flush();
			filter = new DocumentBuilderFilter();
		}

		[Test]
		public void Do_not_build_documents_brefore_payer_registration_period()
		{
			filter.Period = DateTime.Now.AddMonths(-1).ToPeriod();
			var invoices = filter.BuildInvoices(DateTime.Now);
			Assert.That(invoices.Any(i => i.Payer == payer), Is.False);
		}

		/// <summary>
		/// если первый счет был сгенерирован за май то автоматически за
		/// март формировать его не надо
		/// </summary>
		[Test]
		public void Do_not_build_documents_before_first_document_build_period()
		{
			filter.Period = DateTime.Now.AddMonths(2).ToPeriod();
			var invoices = filter.BuildInvoices(DateTime.Now);
			Assert.That(invoices.Any(i => i.Payer == payer), Is.True);

			filter.Period = DateTime.Now.AddMonths(1).ToPeriod();
			invoices = filter.BuildInvoices(DateTime.Now);
			Assert.That(invoices.Any(i => i.Payer == payer), Is.False);
		}
	}
}