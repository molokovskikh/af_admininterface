using System;
using System.Configuration;
using System.Threading;
using Castle.ActiveRecord;
using NUnit.Framework;
using SHDocVw;
using WatiN.Core;

namespace Functional.ForTesting
{
	[TestFixture]
	public class WatinFixture
	{
		protected SessionScope scope;
		protected bool UseTestScope;

		[SetUp]
		public void Setup()
		{
			if (UseTestScope)
				scope = new SessionScope(FlushAction.Never);
		}

		[TearDown]
		public void Teardown()
		{
			if (scope == null)
				return;
			scope.Dispose();
			scope = null;
		}

		public static string BuildTestUrl(string urlPart)
		{
			return String.Format("http://localhost:{0}/{1}",
								 ConfigurationManager.AppSettings["webPort"],
								 urlPart);
		}

		protected static void CheckForError(IE browser)
		{
			if (browser.ContainsText("Error"))
			{
				Console.WriteLine(browser.Text);
			}
		}

		protected IE Open(object item)
		{
			return null;
		}

		protected IE Open(string uri)
		{
			var browser = new IE(BuildTestUrl(uri));
/*
			((InternetExplorerClass)browser.InternetExplorer).DocumentComplete += (object disp, ref object url) => {
				Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
				//Console.WriteLine(browser.Text);
				//Assert.That(browser.Text, Is.Not.ContainsSubstring("exception"));
			};
*/
			return browser;
		}

		protected IE Open(string uri, params object[] args)
		{
			return Open(String.Format(uri, args));
		}
	}
}
