using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class OrdersViewFixture
	{
		[SetUp]
		public void Setup()
		{
			SecurityContext.GetAdministrator = () => new Administrator { RegionMask = ulong.MaxValue };
		}

		[Test]
		public void Find_not_sended_orders()
		{
			OrderView.FindNotSendedOrders();
		}
	}
}
