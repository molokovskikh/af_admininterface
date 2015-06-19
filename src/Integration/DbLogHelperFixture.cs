using AdminInterface.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	[TestFixture]
	public class DbLogHelperFixture : AdmIntegrationFixture
	{
		[Test]
		public void Set_up_transaction_parameters()
		{
			DbLogHelper.SetupParametersForTriggerLogging(
				"test",
				"localhost");

			Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(),
				Is.EqualTo("test"));
			Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(),
				Is.EqualTo("localhost"));
		}

		[Test]
		public void Set_up_transaction_parameters_from_ananymous_object()
		{
			DbLogHelper.SetupParametersForTriggerLogging(new {
				InUser = "test",
				InHost = "localhost"
			});


			Assert.That(session.CreateSQLQuery("select @InUser;").UniqueResult<string>(),
				Is.EqualTo("test"));
			Assert.That(session.CreateSQLQuery("select @InHost;").UniqueResult<string>(),
				Is.EqualTo("localhost"));
		}
	}
}