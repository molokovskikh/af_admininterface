using System;
using System.Configuration;
using AdminInterface.Helpers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Common.Web.Ui.ActiveRecord;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.ForTesting
{
	public class WatinFixture2 : WatinFixture
	{
		public WatinFixture2()
		{
			UseTestScope = true;
			SaveBrowser = true;
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
			var browser = new IE(BuildTestUrl(uri));
/*
			((InternetExplorerClass)browser.InternetExplorer).DocumentComplete += (object disp, ref object url) => {
				Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
				//Console.WriteLine(browser.Text);
				//Assert.That(browser.Text, Is.Not.ContainsSubstring("exception"));
			};
*/
			if (SaveBrowser)
				this.browser = browser;
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
