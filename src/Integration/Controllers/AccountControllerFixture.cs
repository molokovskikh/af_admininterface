using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class AccountControllerFixture : ControllerFixture
	{
		private AccountController controller;
		private User user;

		[SetUp]
		public void Setup()
		{
			user = DataMother.CreateSupplierUser();
			controller = new AccountController();
			PrepareController(controller, "Account", "SetUserStatus");
		}

		[Test]
		public void Disable_supplier_user()
		{
			Assert.That(user.Enabled, Is.True);
			controller.SetUserStatus(user.Id, false);
			user.Refresh();
			Assert.That(user.Enabled, Is.False);
		}
	}
}