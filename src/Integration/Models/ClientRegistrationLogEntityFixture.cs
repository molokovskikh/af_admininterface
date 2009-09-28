using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ClientRegistrationLogEntityFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
			SecurityContext.GetAdministrator = () => new Administrator{ RegionMask = ulong.MaxValue };
		}

		[Test]
		public void Get_not_updated_clients()
		{
			ClientRegistrationLogEntity.NotUpdated(8, 0, 0, "shortname", "asc");
		}

		[Test]
		public void Get_not_ordered_clients()
		{
			ClientRegistrationLogEntity.NotOrdered(8, 0, 0, "shortname", "asc");
		}
	}
}
