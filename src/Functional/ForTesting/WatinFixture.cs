using System;
using System.Configuration;
using AdminInterface.Helpers;
using NUnit.Framework;
using WatiN.Core;
using mshtml;

namespace Functional.ForTesting
{
	[TestFixture, Obsolete("Устарел используй WatinFixture2")]
	public class WatinFixture
	{
		protected Browser browser;

		[TearDown]
		public void Teardown()
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

		protected IE Open(object item, string action = null)
		{
			return Open(AppHelper.GetShortUrl(item, action));
		}

		protected IE Open(string uri = "/")
		{
			return new IE(BuildTestUrl(uri));
		}

		protected IE Open(string uri, params object[] args)
		{
			return Open(String.Format(uri, args));
		}
	}
}
