using System.Configuration;
using System.IO;
using AdminInterface.Test.ForTesting;
using Microsoft.VisualStudio.WebHost;
using NUnit.Framework;

namespace Functional
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[SetUp]
		public void SetupFixture()
		{
			ForTest.InitialzeAR();

			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
		}

		[TearDown]
		public void TeardownFixture()
		{
			_webServer.Stop();
		}
	}
}
