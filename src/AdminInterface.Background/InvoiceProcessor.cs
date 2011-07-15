using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using log4net;

namespace AdminInterface.Background
{
	public class InvoiceProcessor
	{
		private static ILog _log = LogManager.GetLogger(typeof(InvoiceProcessor));

		public void Process()
		{
			using (new SessionScope(FlushAction.Never))
			{
				var invoices = Invoice.Queryable.Where(i => i.SendToEmail && i.Date <= DateTime.Today);
				foreach (var invoice in invoices)
				{
					using (var transaction = new TransactionScope(OnDispose.Rollback))
					{
						try
						{
							invoice.Send();
							if (_log.IsDebugEnabled)
								_log.DebugFormat("Счет {3} для плательщика {2} за {0} отправлен на адреса {1}",
									BindingHelper.GetDescription(invoice.Period),
									invoice.Payer.GetInvocesAddress(),
									invoice.Payer.Name,
									invoice.Id);
						}
						catch (DoNotHaveContacts)
						{
							if (_log.IsDebugEnabled)
								_log.DebugFormat("Счет {0} не отправлен тк не задана контактная информация для плательщика {1} - {2}",
									invoice.Id,
									invoice.Payer.Id,
									invoice.Payer.Name);

							if (invoice.ShouldNotify())
							{
								invoice.LastErrorNotification = DateTime.Now;
								MonorailMailer.Deliver(m => m.DoNotHaveInvoiceContactGroup(invoice));
							}
						}

						invoice.Update();
						transaction.VoteCommit();
					}
				}
			}
		}
	}
}