using AdminInterface.Models.Security;
using AdminInterface.Security;
using Integration.ForTesting;
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