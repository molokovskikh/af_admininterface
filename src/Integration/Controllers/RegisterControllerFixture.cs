using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using Functional.ForTesting;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Common.Web.Ui.Models;
using System.Configuration;


namespace Integration.Controllers
{
	[TestFixture]
	public class RegisterControllerFixture : BaseControllerTest
	{
		private RegisterController controller;

		[SetUp]
		public void Setup()
		{
			controller = new RegisterController();
			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";
		}

		[Test, Ignore("Чинить. Нужно вместо null передавать объект JuridicalOrganisation")]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			var workerRequest = new SimpleWorkerRequest("", "", "", "http://test", new StreamWriter(new MemoryStream()));
			var context = new HttpContext(workerRequest);
			HttpContext.Current = context;
			
			Client client = DataMother.CreateTestClient();
			Payer payer = client.Payer;
			payer.Comment = "ata";
			payer.Update();
			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };
			controller.Registered(payer, null, paymentOptions, client.Id, false);

			Assert.That(Payer.Find(payer.PayerID).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}

		[Test]
		public void Create_LegalEntity()
		{
			string legalName = null;
			string legalFullName = null;
			string legalAddress = null;
			var client1 = DataMother.CreateTestClientWithPayer();
			client1.MaskRegion = 1;
			client1.Settings = new DrugstoreSettings
			{
				Id = client1.Id,
				WorkRegionMask = client1.MaskRegion,
				OrderRegionMask = 111,
				ParseWaybills = true,
				ShowAdvertising = true,
				ShowNewDefecture = true,
			};
			client1.Settings.WorkRegionMask = 1;
			RegionSettings[] regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}};
			AdditionalSettings addsettings = new AdditionalSettings();
			addsettings.PayerExists = true;
			Contact[] clientContacts = new[] {
				new Contact{Id = 1, Type = 0, ContactText = "11@33.ru"}};
			Person[] person = new[] {new Person()};

			controller.RegisterClient(client1, 1, regionSettings, null, addsettings, "address", client1.Payer, 
				client1.Payer.PayerID, null, clientContacts, null, new Contact[0], person, "11@ff.ru", "");
			var CommandText = String.Format("select id, payerId, name, fullname, address from billing.LegalEntities where payerid = {0}", client1.Payer.PayerID);
			using (var connection1 = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection1.Open();
				var command = new MySqlCommand(CommandText, connection1);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						legalAddress = reader.GetString("Address");
						legalFullName = reader.GetString("FullName");
						legalName = reader.GetString("Name");
					}
					reader.Close();
				}
				connection1.Close();
			}
			
			Assert.That(legalName, Is.EqualTo(client1.Payer.ShortName));
			Assert.That(legalFullName, Is.EqualTo(client1.Payer.JuridicalName));
			Assert.That(legalAddress, Is.EqualTo(client1.Payer.JuridicalAddress));
		}

		[Test]
		public void Create_client_with_smart_order()
		{
			using (new SessionScope())
			{
				var client = DataMother.CreateTestClient();
				client.MaskRegion = 1;
				client.Settings = new DrugstoreSettings
				                  	{
				                  		Id = client.Id,
				                  		WorkRegionMask = client.MaskRegion,
				                  		OrderRegionMask = 111,
				                  		ParseWaybills = true,
				                  		ShowAdvertising = true,
				                  		ShowNewDefecture = true,
				                  	};
				client.Settings.WorkRegionMask = 1;
				RegionSettings[] regionSettings = new[]
				                                  	{
				                                  		new RegionSettings
				                                  			{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}
				                                  	};
				AdditionalSettings addsettings = new AdditionalSettings();
				addsettings.PayerExists = true;
				Contact[] clientContacts = new[]
				                           	{
				                           		new Contact {Id = 1, Type = 0, ContactText = "11@ww.ru"}
				                           	};
				Person[] person = new[] {new Person()};

				controller.RegisterClient(client, 1, regionSettings, null, addsettings, "address", client.Payer,
				                          client.Payer.PayerID, null, clientContacts, null, new Contact[0], person, "", "");
				var client1 = Client.Find(client.Id + 1);
				Assert.That(client1.Settings.SmartOrderRules.AssortimentPriceCode, Is.EqualTo(4662));
				Assert.That(client1.Settings.SmartOrderRules.ParseAlgorithm, Is.EqualTo("TestSource"));
				Assert.That(client1.Settings.EnableSmartOrder, Is.EqualTo(true));
			}
		}
	}
}
