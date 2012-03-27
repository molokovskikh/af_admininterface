using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.Web.Ui.Controllers;

namespace AdminInterface.MonoRailExtentions
{
	public class AdminInterfaceController : BaseController
	{
		public AdminInterfaceController()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Admin;
			};
		}

		protected Administrator Admin
		{
			get
			{
				return SecurityContext.Administrator;
			}
		}

		protected IUserStorage Storage
		{
			get
			{
				return ADHelper.Storage;
			}
		}

		protected AppConfig Config
		{
			get
			{
				return Global.Config;
			}
		}

		public void RedirectTo(object entity, string action = "Show")
		{
			var controller = AppHelper.GetControllerName(entity);
			var id = ((dynamic)entity).Id;
			RedirectUsingRoute(controller, action, new {id});
		}
	}
}