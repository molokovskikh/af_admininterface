using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Views.Brail;
using Common.Tools;
using Common.Web.Ui.Helpers;
using IgorO.ExposedObjectProject;
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
			InvoiceProcessor.Init();
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
						invoice.Send();
						if (_log.IsDebugEnabled)
							_log.DebugFormat("—чет {3} дл€ плательщика {2} за {0} отправлен на адреса {1}",
								BindingHelper.GetDescription(invoice.Period),
								invoice.Payer.GetInvocesAddress(),
								invoice.Payer.ShortName,
								invoice.Id);

						invoice.Update();
						transaction.VoteCommit();
					}
				}
			}
		}

		public static void Init()
		{
			ActiveRecordStarter.Initialize(
				new[] {
					Assembly.Load("AdminInterface"),
					Assembly.Load("Common.Web.Ui")
				},
				ActiveRecordSectionHandler.Instance);

			var config = new MonoRailConfiguration();
			config.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));

			var provider = new FakeServiceProvider();
			provider.Services.Add(typeof(IMonoRailConfiguration), config);
			var loader = new FileAssemblyViewSourceLoader("");
			var executingAssembly = Assembly.GetExecutingAssembly();
			loader.AddAssemblySource(new AssemblySourceInfo(executingAssembly, "AdminInterface.Background.Views"));
			provider.Services.Add(typeof(IViewSourceLoader), loader);

			var manager = new DefaultViewEngineManager();
			manager.Service(provider);
			manager.Initialize();
			var namespaces = ExposedObject.From(manager).viewEnginesFastLookup[0].Options.NamespacesToImport;
			namespaces.Add("Boo.Lang.Builtins");
			namespaces.Add("AdminInterface.Helpers");
			BaseMailer.ViewEngineManager = manager;
		}
	}
}