using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class SendInvoiceTaskFixture : Test.Support.IntegrationFixture
	{
		private SendInvoiceTask task;

		List<MailMessage> messages;

		[SetUp]
		public void Setup()
		{
			messages = new List<MailMessage>();
			ForTest.InitializeMailer();
			var mailer = ForTest.TestMailer(m => messages.Add(m));

			task = new SendInvoiceTask(mailer);
		}

		[Test]
		public void Send_invoice_only_once()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			payer.ContactGroupOwner
				.AddContactGroup(ContactGroupType.Invoice)
				.AddContact(ContactType.Email, "test@analit.net");
			payer.InvoiceSettings.EmailInvoice = true;
			Save(payer);
			var invoice = payer.BuildInvoices(DateTime.Today, new Period(DateTime.Today)).Single();
			Save(invoice);

			Flush();
			Close();

			task.Process();

			var message = messages.FirstOrDefault(m => m.Body.Contains(String.Format("Счет №{0}", invoice.Id)));
			Assert.That(message, Is.Not.Null);
			messages.Clear();
			task.Process();

			message = messages.FirstOrDefault(m => m.Body.Contains(String.Format("Счет №{0}", invoice.Id)));
			Assert.That(message, Is.Null, "Отправили счет повторно");
		}
	}
}