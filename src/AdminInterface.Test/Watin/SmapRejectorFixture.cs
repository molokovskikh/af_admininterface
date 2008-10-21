using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.WebHost;
using NUnit.Framework;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class SmapRejectorFixture
	{
		private Server _webServer;

		[TestFixtureSetUp]
		public void SetupFixture()
		{
			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
		}

		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			_webServer.Stop();
		}

		private static string BuildTestUrl(string urlPart)
		{
			return String.Format("http://localhost:{0}/{1}",
								 ConfigurationManager.AppSettings["webPort"],
								 urlPart);
		}

		[Test]
		public void Main_form_should_have_link_to_rejected_emails_where_should_be_emails_for_current_day()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{

			}
		}
	}
}
