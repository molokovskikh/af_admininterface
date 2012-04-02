using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class OrdersViewFixture
	{
		[Test]
		public void Find_not_sended_orders()
		{
			new OrderFilter {
				NotSent = true
			}.Find();
		}
	}
}
