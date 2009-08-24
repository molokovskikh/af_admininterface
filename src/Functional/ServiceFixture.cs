/*
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Test.ForTesting;
using Microsoft.VisualStudio.WebHost;
using NUnit.Framework;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class ServiceFixture
	{
		private Server _webServer;

		[TestFixtureSetUp]
		public void SetupFixture()
		{
			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
			ForTest.InitialzeAR();
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
		public void Main_form_should_contains_link_to_services_list()
		{
			Service.DeleteAll();
			new Service{ Cost = 50, Name = "test1"}.Save();
			new Service { Cost = 10, Name = "test2" }.Save();

			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{
				browser.Link(Find.ByText("Сервисы")).Click();
				Assert.That(browser.ContainsText("test1"));
				Assert.That(browser.ContainsText("50"));
				Assert.That(browser.ContainsText("test2"));
				Assert.That(browser.ContainsText("10"));
			}
		}

		[Test]
		public void Edit_payer_page_shold_contains_assigned_services()
		{
			Service.DeleteAll();
			var s1 = new Service { Cost = 50, Name = "test1" };
			s1.Save();

			var s2 = new Service { Cost = 10, Name = "test2" };
			s2.Save();

			var payer = new Payer
			            	{
			            		ShortName = "Test",
			            		Services =  new List<Service>{ s1, s2 },
								Clients = new List<Client> { new Client{  ShortName = "Test"} }
			            	};
			payer.Save();

			using (var browser = new IE(BuildTestUrl("billing/edit.rails?clientCode="+ payer.Clients[0].Id)))
			{
				Assert.That(browser.ContainsText("test1"));
				Assert.That(browser.ContainsText("50"));
				Assert.That(browser.ContainsText("test2"));
				Assert.That(browser.ContainsText("10"));
			}
		}
	}
}
*/
