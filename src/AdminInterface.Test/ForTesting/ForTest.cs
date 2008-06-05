using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Common.Web.Ui.Models;
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
												Assembly.Load("AdminInterface.Test"),
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
			return new Client
			       	{
			       		ShortName = "Test short name",
			       		FullName = "Test full name",
			       		RegistrationDate = DateTime.Now
			       	};
		}

		public static void InitialzeAR()
		{
		}
	}
}
