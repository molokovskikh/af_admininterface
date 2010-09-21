using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class MailerFixture : BaseControllerTest
	{
		[Test]
		public void Enable_changed()
		{
			var _controller = new RegisterController();
			SecurityContext.GetAdministrator = () => new Administrator {UserName = "TestAdmin"};
			PrepareController(_controller, "Registered");
			_controller.Mail().EnableChanged(new Client(), false).Send();
		}
	}
}