using System;
using System.Configuration;
using NUnit.Framework;
using WatiN.Core;

namespace AdminInterface.Test.ForTesting
{
	[TestFixture]
	public class WatinFixture
	{
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
	}
}
