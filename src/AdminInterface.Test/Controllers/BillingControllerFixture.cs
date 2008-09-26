/*
using System.Linq;
using System.Collections;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Controllers
{
	[TestFixture]
	public class BillingControllerFixture : BaseControllerTest
	{
		private BillingController _controller;

		[SetUp]
		public void SetUp()
		{
			_controller = new BillingController();
			var admin = new Administrator
			            	{
			            		UserName = "TestAdmin",
			            	};
			PrepareController(_controller);
			SecurityContext.GetAdministrator = () => admin;
			ForTest.InitialzeAR();
		}

		[Test]
		public void Show_services_shold_display_all_avaliable_services()
		{
			Service.DeleteAll();
			new Service {Name = "test2"}.Save();
			new Service {Name = "test1"}.Save();

			_controller.ShowServices();
			Assert.That(new ListMapper((ICollection) ControllerContext.PropertyBag["services"]).Property("Name"),
			            Is.EquivalentTo(new[] {"test1", "test2"}));
		}

		[Test]
		public void On_update_services_delete_not_exists_save_new_end_update()
		{
			Service.DeleteAll();
			var service1 = new Service { Name = "test2" };
			service1.Save();

			new Service { Name = "test1" }.Save();

			_controller.UpdateServices(new[]
			                           	{
			                           		new Service
			                           			{
			                           				Id = service1.Id,
			                           				Name = service1.Name,
			                           				Cost = 50
			                           			},
			                           		new Service
			                           			{
			                           				Name = "test3",
			                           				Cost = 10
			                           			},
			                           	});
			var services = Service.FindAll();
			Assert.That(services.Length, Is.EqualTo(2));
			Assert.That(services.FirstOrDefault(s => s.Name == "test3" && s.Cost == 10), Is.Not.Null);
			Assert.That(services.FirstOrDefault(s => s.Name == service1.Name
			                                         && s.Cost == 50
			                                         && s.Id == service1.Id), Is.Not.Null);
		}
	}
}
*/
