using AdminInterface.Models;
using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class UserSearchItemFixture
	{
		[Test]
		public void Supplier_user_can_not_be_old()
		{
			var item = new UserSearchItem {
				ClientType = SearchClientType.Supplier
			};
			Assert.That(item.IsOldUserUpdate, Is.False);
		}
	}
}