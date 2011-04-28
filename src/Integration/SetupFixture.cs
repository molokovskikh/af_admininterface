using System;
using System.Linq;
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
				UserName = Environment.UserName,
				Email = "kvasovtest@analit.net",
				PhoneSupport = "112",
				RegionMask = ulong.MaxValue,
				ManagerName = "test",
			};
			admin.AllowedPermissions = Enum.GetValues(typeof(PermissionType))
				.Cast<PermissionType>()
				.Select(t => Permission.Find(t))
				.ToList();
			admin.Save();
			SecurityContext.GetAdministrator = () => admin;
			Administrator.GetHost = () => "localhost";
		}
	}
}