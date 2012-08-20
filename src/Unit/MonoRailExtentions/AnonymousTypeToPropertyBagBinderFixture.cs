using System;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;

namespace Unit.MonoRailExtentions
{
	[TestFixture]
	public class AnonymousTypeToPropertyBagBinderFixture : BaseControllerTest
	{
		private TestController _controller;

		private class TestController : SmartDispatcherController
		{
			[return: AnonymousTypeToPropertyBagBinder]
			public object Test()
			{
				return new { i1 = 1, s1 = "123" };
			}
		}

		[SetUp]
		public void SetUp()
		{
			_controller = new TestController();
			PrepareController(_controller, "TestController", "Test");
		}

		[Test]
		public void Property_from_anonymous_type_should_bind_to_property_bag()
		{
			_controller.Process(Context, ControllerContext);
			Assert.That(ControllerContext.PropertyBag["i1"], Is.EqualTo(1));
			Assert.That(ControllerContext.PropertyBag["s1"], Is.EqualTo("123"));
		}
	}
}