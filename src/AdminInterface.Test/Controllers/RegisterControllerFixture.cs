using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
using log4net.Config;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Controllers
{
	[TestFixture]
	public class RegisterControllerFixture : BaseControllerTest
	{
		private RegisterController _controller;

		[SetUp]
		public void Setup()
		{
			_controller = new RegisterController();
			PrepareController(_controller, "Registered");

			XmlConfigurator.Configure();
			ActiveRecordStarter.Initialize(new[]
			                                {
			                                    Assembly.Load("AdminInterface"),
			                                    Assembly.Load("Common.Web.Ui")
			                                },
										   ActiveRecordSectionHandler.Instance);
		}

		[Test]
		public void Set_to_payer_juridical_name_client_full_name_if_juridical_name_not_set()
		{
			Payer payer;
			Client client;
			using (new TransactionScope())
			{
				client = new Client();
				client.ShortName = "Test short name";
				client.FullName = "Test full name";
				client.RegistrationDate = DateTime.Now;
				client.SaveAndFlush();

				payer = GetTestPayer();
				payer.SaveAndFlush();
			}

			_controller.Registered(payer, client.Id, false);

			Assert.That(Payer.Find(payer.PayerID).JuridicalName, Is.EqualTo(client.FullName));
		}

		private Payer GetTestPayer()
		{
			return new Payer
			       	{
			       		ShortName = "Test",
			       		JuridicalName = "",
			       		JuridicalAddress = "",
			       		KPP = "",
			       		INN = "",
						ActualAddressHouse = "",
						ActualAddressIndex = "",
						ActualAddressOffice = "",
						ActualAddressCountry = "",
                        ActualAddressProvince = "",
						ActualAddressRegion = "",
						ActualAddressStreet = "",
						ActualAddressTown = "",
						BeforeNamePrefix = "",
						AfterNamePrefix = "",
			       	};
		}
	}
}
