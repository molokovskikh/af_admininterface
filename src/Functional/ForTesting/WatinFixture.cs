﻿using System;
using System.Configuration;
using AdminInterface.Helpers;
using NHibernate;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;
using mshtml;

namespace Functional.ForTesting
{
	[TestFixture, Obsolete("Устарел используй WatinFixture2")]
	public class WatinFixture
	{
		protected ISession session;
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
			return Open(WatinFixture2.GetShortUrl(item, action));
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
