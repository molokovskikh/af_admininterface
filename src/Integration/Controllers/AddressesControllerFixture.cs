using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class AddressesControllerFixture : ControllerFixture
	{
		private DeliveriesController controller;
		private Client client;
		private User user;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			user = client.Users.First();
			controller = new DeliveriesController();
			PrepareController(controller, "Addresses", "Add");
		}

		[Test]
		public void Add()
		{
			var address = new Address {Value = "тестовый адрес доставки"};
			address.AvaliableForUsers.Add(user);
			controller.Add(address, new Contact[0], client.Id, "тестовое сообщение для биллинга");

			var messages = ClientInfoLogEntity.Queryable.Where(m => m.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}
	}
}