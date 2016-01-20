using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Tools.Threading;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Jobs;
using log4net;
using log4net.Config;
using NDesk.Options;

namespace AdminInterface.Background
{
	public class Program
	{
		private static ILog log = LogManager.GetLogger(typeof(Program));

		public static int Main(string[] args)
		{
			try {
				var help = false;
				string task = null;
				var options = new OptionSet {
					{ "help", x => help = x != null },
					{ "task=", "Выполнить указанную задачу и выйти", x => task = x },
				};
				try {
					options.Parse(args);
				} catch(Exception e) {
					Console.WriteLine(e.Message);
					options.WriteOptionDescriptions(Console.Out);
				}
				if (help) {
					options.WriteOptionDescriptions(Console.Out);
					return 0;
				}
				XmlConfigurator.Configure();
				var assembly = typeof(Program).Assembly;
				StandaloneInitializer.Init(assembly);

				var jobs = new List<ActiveRecordJob>();
				using (new SessionScope()) {
					var job = new ActiveRecordJob(new SendPaymentNotification());
					jobs.Add(job);
				}
				var mailer = new MonorailMailer {
					SiteRoot = ConfigurationManager.AppSettings["SiteRoot"]
				};

				var tasks = new List<Task> {
					new SendInvoiceTask(mailer)
				};
				tasks = tasks.Concat(assembly.GetTypes().Except(tasks.Select(x => x.GetType()).ToArray())
					.Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && typeof(Task).IsAssignableFrom(t))
					.Select(t => Activator.CreateInstance(t))
					.OfType<Task>())
					.ToList();

				if (!String.IsNullOrEmpty(task)) {
					var toRun = tasks.Where(x => x.GetType().Name.Match(task)).ToList();
					if (toRun.Count == 0) {
						Console.WriteLine($"Не удалось найти задачу {task}, доступные задачи {tasks.Implode(x => x.GetType().Name)}");
						return 1;
					}
					toRun.Each(x => x.Execute());
					return 0;
				}

				var actions = tasks.Select(x => new Action(x.Execute))
					.Concat(jobs.Select(x => {
						return new Action(() => {
							using (new SessionScope())
								x.Run();
						});
					})).ToList();
				var runner = new RepeatableCommand(30.Minute(), () => actions.Each(t => {
					try {
						t();
					}
					catch(Exception e) {
						log.Error($"Выполнение задачи {t} завершилось ошибкой", e);
					}
				}));
				tasks.Each(t => t.Cancellation = runner.Cancellation);
				jobs.Each(x => x.Job.Cacellation = runner.Cancellation);
				return CommandService.Start(args, runner);
			}
			catch(Exception e) {
				log.Error("Ошибка при запуске приложения", e);
				return 1;
			}
		}
	}
}