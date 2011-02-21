using System;
using System.Configuration;
using AdminInterface.Helpers;
using Castle.ActiveRecord;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.ForTesting
{
	[TestFixture]
	public class WatinFixture
	{
		protected SessionScope scope;
		protected bool UseTestScope;

		protected Browser browser;

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

			if (browser != null)
			{
				browser.Dispose();
				browser = null;
			}
		}

		public static string BuildTestUrl(string urlPart)
		{
			if (!urlPart.StartsWith("/"))
				urlPart = "/" + urlPart;
			return String.Format("http://localhost:{0}{1}",
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

		protected IE Open(object item, string action = null)
		{
			return new IE(BuildTestUrl(AppHelper.GetUrl(item, action)));
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
