using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Helpers
{
	[TestFixture]
	public class DbLogHelperFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void Set_up_transaction_parameters()
		{
			using (var scope = new SessionScope())
			{
				var session = ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof (ClientWithStatus));
				DbLogHelper.SetupParametersForTriggerLogging("test",
				                                             "localhost",
				                                             session);

				Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(), Is.EqualTo("test"));
				Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(), Is.EqualTo("localhost"));
			}
		}

		[Test]
		public void Set_up_transaction_parameters_from_ananymous_object()
		{
			using (var scope = new SessionScope())
			{
				var session = ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof(ClientWithStatus));
				DbLogHelper.SetupParametersForTriggerLogging(new
				                                             	{
				                                             		InUser = "test",
				                                             		InHost = "localhost"
				                                             	},
				                                             session);

				Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(), Is.EqualTo("test"));
				Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(), Is.EqualTo("localhost"));
			}
		}
	}
}
