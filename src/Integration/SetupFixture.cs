using System;
using System.Linq;
using AdminInterface;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	[SetUpFixture]
	public class SetupFixture
	{
		[SetUp]
		public void Setup()
		{
			IntegrationFixture.DoNotUserTransaction = true;
			Global.Config.DocsPath = "../../../AdminInterface/Docs/";
			Global.Config.RegisterListEmail = "kvasovtest@analit.net";

			ForTest.InitialzeAR();
		}
	}
}