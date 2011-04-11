using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
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


	public class SuppliersController : Controller
	{
		public void Show(uint id)
		{
			PropertyBag["supplier"] = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
		}
	}
}