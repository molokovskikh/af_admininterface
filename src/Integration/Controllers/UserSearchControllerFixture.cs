using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class UserSearchControllerFixture : ControllerFixture
	{
		private UserSearchController controller;

		[SetUp]
		public void Setup()
		{
			controller = new UserSearchController();
			PrepareController(controller, "Search");
		}

		[Test]
		public void Redirect_to_supplier_page_if_single_supplier_found()
		{
			controller.AutoOpen(new List<UserSearchItem> {
				new UserSearchItem {
					ClientType = SearchClientType.Supplier
				}
			});
			Assert.That(Response.RedirectedTo, Is.EqualTo("/suppliers/show.castle"));
			Assert.That(Response.WasRedirected, Is.True);
		}
	}
}