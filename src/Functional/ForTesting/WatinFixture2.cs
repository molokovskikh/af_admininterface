using System;
using System.Configuration;
using AdminInterface.Helpers;
using Castle.ActiveRecord;
using Common.Tools.Calendar;
using Integration.ForTesting;
using NUnit.Framework;
using SHDocVw;
using WatiN.Core;
using WatiN.Core.Native.InternetExplorer;
using WatiN.Core.UtilityClasses;
using WatiN.CssSelectorExtensions;

namespace Functional.ForTesting
{
	public class WatinFixture2 : IntegrationFixture
	{
		protected Browser browser;
		private object httpStatusCode;

		public WatinFixture2()
		{}

		[TearDown]
		public void TearDown()
		{
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
			var internetExplorerClass = ((InternetExplorerClass)((IEBrowser)browser.NativeBrowser).WebBrowser);
			httpStatusCode = null;
			internetExplorerClass.NavigateError += (object disp, ref object url, ref object frame, ref object code, ref bool cancel) => {
				httpStatusCode = code;
			};
			internetExplorerClass.BeforeNavigate2 += (object disp, ref object url, ref object flags, ref object name, ref object data, ref object headers, ref bool cancel) => {
				httpStatusCode = null;
			};

/*			//ie ������������ ��� ����������
			internetExplorerClass.NavigateComplete2 += (object disp, ref object url) => {
				if (httpStatusCode == null)
					return;
				if (Convert.ToInt32(httpStatusCode) == 500)
				{
					throw new Exception(browser.Html);
				}
			};
*/
			if (this.browser != null)
				this.browser.Dispose();

			this.browser = browser;

			new TryFuncUntilTimeOut(2.Second()) {
				SleepTime = TimeSpan.FromMilliseconds(50.0),
				ExceptionMessage = () => string.Format("waiting {0} seconds for document text not null.", 2)
			}.Try<bool>(() => browser.Text != null);
			CheckStatus();
			return browser;
		}

		private void CheckStatus()
		{
			if (httpStatusCode != null && Convert.ToInt32(httpStatusCode) == 500)
				throw new Exception(browser.Text);
		}

		protected dynamic Css(string selector)
		{
			return browser.Css(selector);
		}

		protected void Click(string selector, string name)
		{
			var container = (IElementContainer)browser.CssSelect(selector);
			container.Click(name);
			CheckStatus();
		}

		protected void Click(string name)
		{
			browser.Click(name);
			CheckStatus();
		}

		protected IE Open(string uri, params object[] args)
		{
			return Open(String.Format(uri, args));
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
}