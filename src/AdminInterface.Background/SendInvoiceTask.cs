using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;

namespace AdminInterface.Background
{
	public class SendInvoiceTask
	{
		private MonorailMailer _mailer;

		public SendInvoiceTask(MonorailMailer mailer)
		{
			_mailer = mailer;
		}

		public void Process()
		{
			using (new SessionScope(FlushAction.Never))
			{
				var invoices = Invoice.Queryable.Where(i => (i.SendToEmail || i.SendToMinimail) && i.Date <= DateTime.Today);
				foreach (var invoice in invoices)
				{
					_mailer.Clear();
					using (var transaction = new TransactionScope(OnDispose.Rollback))
					{
						if (invoice.SendToEmail)
						{
							_mailer.InvoiceToEmail(invoice, false);
						}
						else if (invoice.SendToMinimail)
						{
							_mailer.SendInvoiceToMinimail(invoice);
							invoice.SendToMinimail = false;
						}
						_mailer.Send();

						invoice.Update();
						transaction.VoteCommit();
					}
				}
			}
		}
	}
}