using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using log4net;
using log4net.Config;
using Topshelf.Configuration.Dsl;
using Topshelf.Shelving;

namespace AdminInterface.Background
{
	public class Bootstrapper : Bootstrapper<Waiter>
	{
		public void InitializeHostedService(IServiceConfigurator<Waiter> cfg)
		{
			XmlConfigurator.Configure();
			cfg.HowToBuildService(n => new Waiter());
			cfg.WhenStarted(s => s.DoStart());
			cfg.WhenStopped(s => s.Stop());
		}
	}

	public class Waiter : RepeatableCommand
	{
		private static ILog log = LogManager.GetLogger(typeof(Waiter));

		public Waiter()
		{
			Delay = (int)TimeSpan.FromHours(1).TotalMilliseconds;
			Action = () => {
				new InvoiceProcessor().Process();
			};
		}

		public void DoStart()
		{
			StandaloneInitializer.Init(typeof(InvoiceProcessor).Assembly);
			Start();
		}

		public override void Error(Exception e)
		{
			log.Error(e);
		}
	}

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