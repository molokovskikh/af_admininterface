using AdminInterface.Helpers;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class DbLogHelperFixture
	{
		[Test]
		public void Set_up_transaction_parameters()
		{
			using (new SessionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging(
					"test",
					"localhost");

				ArHelper.WithSession(
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
				DbLogHelper.SetupParametersForTriggerLogging(new {
					InUser = "test",
					InHost = "localhost"
				});

				ArHelper.WithSession(
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
