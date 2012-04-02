using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class OrderFilterFixture : IntegrationFixture
	{
		[Test]
		public void Find_order_for_user()
		{
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			var user = client.Users.First();
			new OrderFilter {
				User = user,
				Client = client
			}.Find();
		}
	}
}