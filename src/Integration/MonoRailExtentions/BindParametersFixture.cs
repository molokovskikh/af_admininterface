using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;

namespace Integration.MonoRailExtentions
{
	[TestFixture]
	public class BindParametersFixture
	{
		public class BindObject
		{
			public string Name { get; set; }
		}

		public class TestController : AdminInterfaceController
		{
			public BindObject @object;

			public void Action(BindObject @object)
			{
				@object = @object;
			}
		}

		private StubEngineContext engineContext;
		private StubMonoRailServices services;
		private StubResponse response;
		private StubRequest request;

		[SetUp]
		public void Init()
		{
			request = new StubRequest();
			response = new StubResponse();
			services = new StubMonoRailServices();
			engineContext = new StubEngineContext(request, response, services, null);
		}

		[Test, Ignore("нереализованно")]
		public void Bind_method_parameters()
		{
			var controller = new TestController();

			var context = services.ControllerContextFactory
				.Create("", "test", "action", services.ControllerDescriptorProvider.BuildDescriptor(controller));
			request.QueryString["Name"] = "test";

			controller.Process(engineContext, context);
			Assert.That(controller.@object, Is.Not.Null);
			Assert.That(controller.@object.Name, Is.EqualTo("Test"));

/*
			Assert.IsTrue(controller.parameters.Count != 0);
			Assert.IsNull(controller.parameters[0]);
*/
		}
	}
}