using System;
using System.Collections.Generic;
using System.Reflection;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using log4net.Config;

namespace Integration.ForTesting
{
	public class ForTest
	{
		public static void InitialzeAR()
		{
			XmlConfigurator.Configure();
			if (!ActiveRecordStarter.IsInitialized)
				ActiveRecordStarter.Initialize(new[] {
					Assembly.Load("AdminInterface"),
					Assembly.Load("Common.Web.Ui"),
				}, ActiveRecordSectionHandler.Instance);
		}
	}
}
