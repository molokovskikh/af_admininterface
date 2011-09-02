using System;
using System.Configuration;
using AdminInterface.Helpers;
using Castle.ActiveRecord;
using Common.Tools.Calendar;
using NUnit.Framework;
using WatiN.Core;
using WatiN.Core.UtilityClasses;

namespace Functional.ForTesting
{
	public class WatinFixture2 : WatinFixture
	{
		public WatinFixture2()
		{
			UseTestScope = true;
			SaveBrowser = true;
		}

		protected void Refresh()
		{
			if (scope != null)
				scope.Flush();
			browser.Refresh();
		}

		protected void AssertText(string text)
		{
			Assert.That(browser.Text, Is.StringContaining(text));
		}

		protected void Save(object entity)
		{
			ActiveRecordMediator.Save(entity);
		}
	}

	[TestFixture]
	public class WatinFixture
	{
		protected bool UseTestScope;
		protected bool SaveBrowser;

		protected ISessionScope scope;
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
			if (scope != null)
			{
				scope.Dispose();
				scope = null;
			}

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
			return Open(AppHelper.GetShortUrl(item, action));
		}

		protected IE Open(string uri = "/")
		{
			if (scope != null)
				scope.Flush();

			var browser = new IE(BuildTestUrl(uri));
/*
			((InternetExplorerClass)browser.InternetExplorer).DocumentComplete += (object disp, ref object url) => {
				Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
				//Console.WriteLine(browser.Text);
				//Assert.That(browser.Text, Is.Not.ContainsSubstring("exception"));
			};
*/
			if (SaveBrowser)
			{
				if (this.browser != null)
					this.browser.Dispose();

				this.browser = browser;

				new TryFuncUntilTimeOut(2.Second()) {
						SleepTime = TimeSpan.FromMilliseconds(50.0),
						ExceptionMessage = () => string.Format("waiting {0} seconds for document text not null.", 2)
					}.Try<bool>(() => browser.Text != null);
			}
			return browser;
		}

		protected dynamic Css(string selector)
		{
			return browser.Css(selector);
		}

		protected void Click(string name)
		{
			browser.Click(name);
		}

		protected IE Open(string uri, params object[] args)
		{
			return Open(String.Format(uri, args));
		}
	}
}
