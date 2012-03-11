using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;

namespace AdminInterface.Background
{
	public class InvoiceProcessor
	{
		public void Process()
		{
			var mailer = new MonorailMailer();
			mailer.SiteRoot = ConfigurationManager.AppSettings["SiteRoot"];
			using (new SessionScope(FlushAction.Never))
			{
				var invoices = Invoice.Queryable.Where(i => (i.SendToEmail || i.SendToMinimail) && i.Date <= DateTime.Today);
				foreach (var invoice in invoices)
				{
					using (var transaction = new TransactionScope(OnDispose.Rollback))
					{
						if (invoice.SendToEmail)
						{
							mailer.InvoiceToEmail(invoice, false);
						}
						else if (invoice.SendToMinimail)
						{
							mailer.SendInvoiceToMinimail(invoice);
							invoice.SendToMinimail = false;
						}
						mailer.Send();

						invoice.Update();
						transaction.VoteCommit();
					}
				}
			}
		}
	}
}