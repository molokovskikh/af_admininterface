using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminInterface.Controllers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration.Controllers
{
	public class BuilderFixture : IntegrationFixture2
	{
		public class StubRequest : HttpRequestBase
		{
			public string StubHttpMethod;
			public override string HttpMethod => StubHttpMethod;
		}

		public class StubContext : HttpContextBase
		{
			public StubRequest StubRequest = new StubRequest();

			public override HttpRequestBase Request => StubRequest;
		}

		private DataMother DataMother;
		private BuilderController controller;

		[SetUp]
		public void ControllerSetup()
		{
			DataMother = new DataMother(session);
			ForTest.InitializeMailer();
			controller = new BuilderController();
			controller.DbSession = session;
			var context = new StubContext();
			context.StubRequest.StubHttpMethod = "POST";
			controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
			controller.ValueProvider = new FormCollection().ToValueProvider();

		}

		[Test]
		public void Docs()
		{
			var client = DataMother.CreateTestClientWithUser();
			controller.Docs(client.Users[0].Id);
		}

		[Test]
		public void Inventory()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var supplier = DataMother.CreateSupplier();
			DataMother.CreateTestDocument(supplier, client);
			controller.Inventory(client.Users[0].Id);
		}
	}
}