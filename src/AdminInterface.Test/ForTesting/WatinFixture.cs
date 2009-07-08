using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.WebHost;
using NUnit.Framework;
using WatiN.Core;

namespace AdminInterface.Test.ForTesting
{
	[TestFixture]
	public class WatinFixture
	{
		private static Server _webServer;

		static WatinFixture()
		{
			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
		}

/*		[TestFixtureSetUp]
		public void SetupFixture()
		{
			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
		}*/


/*
		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			_webServer.Stop();
		}
*/

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
	}
}
