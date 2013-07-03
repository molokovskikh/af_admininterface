using System;
using System.Linq;
using AddUser;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	[SetUpFixture]
	public class SetupFixture
	{
		[SetUp]
		public void Setup()
		{
			IntegrationFixture.DoNotUserTransaction = true;
			Global.Config.DocsPath = "../../../AdminInterface/Docs/";

			ForTest.InitialzeAR();
			var admin = new Administrator {
				UserName = Environment.UserName,
				Email = "kvasovtest@analit.net",
				PhoneSupport = "112",
				RegionMask = ulong.MaxValue,
				ManagerName = "test",
			};
			using(new SessionScope()) {
				ArHelper.WithSession(session => {
					admin.AllowedPermissions = Enum.GetValues(typeof(PermissionType))
						.Cast<PermissionType>()
						.Select(t => session.Load<Permission>(t))
						.ToList();
					ActiveRecordMediator.Save(admin);
					SecurityContext.GetAdministrator = () => admin;
					Administrator.GetHost = () => "localhost";
				});
			}
		}
	}
}