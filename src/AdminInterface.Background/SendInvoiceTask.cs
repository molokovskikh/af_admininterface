using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;

namespace AdminInterface.Background
{
	public class SendInvoiceTask : Task
	{
		private MonorailMailer _mailer;

		public SendInvoiceTask(MonorailMailer mailer)
		{
			_mailer = mailer;
		}

		protected override void Process()
		{
			var invoices = Session.Query<Invoice>()
				.Where(i => ((i.SendToEmail && i.Payer.InvoiceSettings.EmailInvoice) || (i.SendToMinimail && i.Payer.InvoiceSettings.SendToMinimail)) && i.Date <= DateTime.Today);
			foreach (var invoice in invoices) {
				_mailer.Clear();
				if (invoice.SendToEmail && invoice.Payer.InvoiceSettings.EmailInvoice) {
					_mailer.InvoiceToEmail(invoice, false);
				}
				else if (invoice.SendToMinimail && invoice.Payer.InvoiceSettings.SendToMinimail) {
					_mailer.SendInvoiceToMinimail(invoice);
					invoice.SendToMinimail = false;
				}
				_mailer.Send();

				Session.Save(invoice);
			}
		}
	}
}