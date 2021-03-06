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
		[OneTimeSetUp]
		public void Setup()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			Global.Config.DocsPath = "../../../AdminInterface/Docs/";
			Global.Config.RegisterListEmail = "kvasovtest@analit.net";

			ForTest.InitialzeAR();
			IntegrationFixture2.Factory = ActiveRecordMediator.GetSessionFactoryHolder().GetSessionFactory(typeof(ActiveRecordBase));
		}
	}
}