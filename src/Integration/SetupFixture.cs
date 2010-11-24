using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[SetUpFixture]
	public class SetupFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
			SecurityContext.GetAdministrator = () => new Administrator{
				UserName = "test",
			};
			Administrator.GetHost = () => "localhost";
		}
	}
}