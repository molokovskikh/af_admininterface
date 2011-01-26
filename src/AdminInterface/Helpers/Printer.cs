using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
using Microsoft.Win32;

namespace AdminInterface.Helpers
{
	public class Printer
	{
		private TaskScheduler Sta = new StaTaskScheduler(1);

		public void PrintHtml(string htmlPath)
		{
			Task.Factory.StartNew(() => PrintOnStaThread(htmlPath), CancellationToken.None, TaskCreationOptions.None, Sta).Wait();
		}

		public static void SetPrinter(string printer)
		{
			var path = "win32_printer.DeviceId='" + printer + "'";
			using (var printerObject = new ManagementObject(path))
			{
				printerObject.InvokeMethod("SetDefaultPrinter", null, null);
			}

			const string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
			using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
			{
				if (key == null)
					return;

				key.SetValue("footer", "");
				key.SetValue("header", "");
			}
		}

		public void PrintView(StandaloneBooViewEngine brail, string name, Dictionary<string, object> parameters)
		{
			var file = Path.GetTempFileName();
			try
			{
				using (var writer = new StreamWriter(file))
				{
					brail.Process(name,
						writer,
						parameters);
				}
				PrintHtml(file);
			}
			finally
			{
				RemoteServiceHelper.Try(() => File.Delete(file));
			}
		}

		private void PrintOnStaThread(string htmlPath)
		{
			const short PRINT_WAITFORCOMPLETION = 2;
			const int OLECMDID_PRINT = 6;
			const int OLECMDEXECOPT_DONTPROMPTUSER = 2;
			using(var browser = new WebBrowser())
			{
				browser.Navigate(htmlPath);
				while(browser.ReadyState != WebBrowserReadyState.Complete)
					Application.DoEvents();

				dynamic ie = browser.ActiveXInstance;
				ie.ExecWB(OLECMDID_PRINT, OLECMDEXECOPT_DONTPROMPTUSER, PRINT_WAITFORCOMPLETION);
			}
		}
	}

	/// <summary>Provides a scheduler that uses STA threads.</summary>
	public sealed class StaTaskScheduler : TaskScheduler, IDisposable
	{
		/// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
		private BlockingCollection<Task> _tasks;
		/// <summary>The STA threads used by the scheduler.</summary>
		private readonly List<Thread> _threads;

		/// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
		/// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
		public StaTaskScheduler(int numberOfThreads)
		{
			// Validate arguments
			if (numberOfThreads < 1) throw new ArgumentOutOfRangeException("concurrencyLevel");

			// Initialize the tasks collection
			_tasks = new BlockingCollection<Task>();

			// Create the threads to be used by this scheduler
			_threads = Enumerable.Range(0, numberOfThreads).Select(i =>
			{
				var thread = new Thread(() =>
				{
					// Continually get the next task and try to execute it.
					// This will continue until the scheduler is disposed and no more tasks remain.
					foreach (var t in _tasks.GetConsumingEnumerable())
					{
						TryExecuteTask(t);
					}
				});
				thread.IsBackground = true;
				thread.SetApartmentState(ApartmentState.STA);
				return thread;
			}).ToList();

			// Start all of the threads
			_threads.ForEach(t => t.Start());
		}

		/// <summary>Queues a Task to be executed by this scheduler.</summary>
		/// <param name="task">The task to be executed.</param>
		protected override void QueueTask(Task task)
		{
			// Push it into the blocking collection of tasks
			_tasks.Add(task);
		}

		/// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
		/// <returns>An enumerable of all tasks currently scheduled.</returns>
		protected override IEnumerable<Task> GetScheduledTasks()
		{
			// Serialize the contents of the blocking collection of tasks for the debugger
			return _tasks.ToArray();
		}

		/// <summary>Determines whether a Task may be inlined.</summary>
		/// <param name="task">The task to be executed.</param>
		/// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
		/// <returns>true if the task was successfully inlined; otherwise, false.</returns>
		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			// Try to inline if the current thread is STA
			return
				Thread.CurrentThread.GetApartmentState() == ApartmentState.STA &&
				TryExecuteTask(task);
		}

		/// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
		public override int MaximumConcurrencyLevel
		{
			get { return _threads.Count; }
		}

		/// <summary>
		/// Cleans up the scheduler by indicating that no more tasks will be queued.
		/// This method blocks until all threads successfully shutdown.
		/// </summary>
		public void Dispose()
		{
			if (_tasks != null)
			{
				// Indicate that no new tasks will be coming in
				_tasks.CompleteAdding();

				// Wait for all threads to finish processing tasks
				foreach (var thread in _threads) thread.Join();

				// Cleanup
				_tasks.Dispose();
				_tasks = null;
			}
		}
	}
}