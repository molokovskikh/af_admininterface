using System;
using System.Configuration;
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

		protected IE Open(string uri)
		{
			return new IE(BuildTestUrl(uri));
		}

		protected IE Open(string uri, params object[] args)
		{
			return new IE(BuildTestUrl(String.Format(uri, args)));
		}
	}
}
