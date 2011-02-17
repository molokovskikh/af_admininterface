using System.Collections.Generic;
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
			var admin = new Administrator{
				UserName = "test",
				Email = "kvasovtest@analit.net",
				PhoneSupport = "112",
				RegionMask = ulong.MaxValue,
				ManagerName = "test",
				AllowedPermissions = new List<Permission> {
					Permission.Find(PermissionType.Billing),
					Permission.Find(PermissionType.ViewDrugstore),
				}
			};
			admin.Save();
			SecurityContext.GetAdministrator = () => admin;
			Administrator.GetHost = () => "localhost";
		}
	}
}