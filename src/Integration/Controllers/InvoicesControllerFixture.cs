using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class InvoicesControllerFixture : BaseControllerTest
	{
		private InvoicesController controller;

		[SetUp]
		public void Setup()
		{
			controller = new InvoicesController();
			PrepareController(controller);
			Services.UrlBuilder.UseExtensions = false;
		}

		[Test]
		public void After_build_redirect_to_index()
		{
			var recipient = Recipient.Queryable.First();
			var region = Region.Queryable.First();

			controller.Build(Period.April, region.Id, "test", DateTime.Now, recipient.Id);

			Assert.That(Response.RedirectedTo,
				Is.StringEnding(String.Format("Index?filter.Period={0}&filter.Region.Id={1}&filter.Recipient.Id={2}",
					(int)Period.April,
					region.Id,
					recipient.Id)));
		}
	}
}