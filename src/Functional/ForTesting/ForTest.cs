using System;
using System.Collections.Generic;
using System.Reflection;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using log4net.Config;

namespace AdminInterface.Test.ForTesting
{
	public class ForTest
	{
		static ForTest()
		{
			XmlConfigurator.Configure();
			ActiveRecordStarter.Initialize(new[]
			                                {
			                                    Assembly.Load("AdminInterface"),
			                                    Assembly.Load("Common.Web.Ui"),
												Assembly.Load("Functional"),
			                                },
										   ActiveRecordSectionHandler.Instance);
		}

		public static Payer CreatePayer()
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

		public static Client CreateClient()
		{
			var client = new Client {
				Name = "Test short name",
				FullName = "Test full name",
				RegistrationDate = DateTime.Now,
				BillingInstance = CreatePayer(),
			};
			client.Users = new List<User> {
				new User {
					Login = "test" + new Random().Next(),
					Name = "test"
				}
			};
			client.Users[0].Client = client;
			return client;
		}

		public static void InitialzeAR()
		{}
	}
}
