using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NUnit.Framework;


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
		    using (new SessionScope())
		    {
		        DbLogHelper.SetupParametersForTriggerLogging<ClientWithStatus>("test",
		                                                                       "localhost");

		        ArHelper.WithSession<ClientWithStatus>(
		            session => {
		                Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(),
		                            Is.EqualTo("test"));
		                Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(),
		                            Is.EqualTo("localhost"));
		            });

		    }
		}

	    [Test]
		public void Set_up_transaction_parameters_from_ananymous_object()
		{
		    using (new SessionScope())
		    {
		        DbLogHelper.SetupParametersForTriggerLogging<ClientWithStatus>(new
		                                                                           {
		                                                                               InUser = "test",
		                                                                               InHost = "localhost"
		                                                                           });

		        ArHelper.WithSession<ClientStatus>(
		            session => {
		                Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(),
		                            Is.EqualTo("test"));
		                Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(),
		                            Is.EqualTo("localhost"));
		            });
		    }
		}
	}
}
