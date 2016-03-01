using AdminInterface.Models.Security;
using AdminInterface.Security;
using NUnit.Framework;

namespace Unit
{
	[SetUpFixture]
	public class Setup
	{
		[OneTimeSetUp]
		public void SetupFixture()
		{
			SecurityContext.GetAdministrator = () => new Administrator { UserName = "test" };
		}
	}
}