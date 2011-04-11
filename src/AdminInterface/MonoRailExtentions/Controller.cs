using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	public class Controller : SmartDispatcherController
	{
		public Controller()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Administrator;
			};
		}

		protected Administrator Administrator
		{
			get
			{
				return SecurityContext.Administrator;
			}
		}
	}

	public class ARController : ARSmartDispatcherController
	{
		public ARController()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Administrator;
			};
		}

		protected Administrator Administrator
		{
			get
			{
				return SecurityContext.Administrator;
			}
		}
	}
}