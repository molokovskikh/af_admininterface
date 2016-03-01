using System;
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
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			SecurityContext.GetAdministrator = () => new Administrator { UserName = "test" };
		}
	}
}