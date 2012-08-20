using System.Collections;
using System.Reflection;
using AdminInterface.Models.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Security
{
	public class SecurityFilter : IFilter, IFilterAttributeAware
	{
		private SecureAttribute _attribute;

		public bool Perform(ExecuteWhen exec,
			IEngineContext context,
			IController controller,
			IControllerContext controllerContext)
		{
			var administrator = SecurityContext.Administrator;
			if (administrator == null) {
				context.Response.RedirectToUrl("~/Rescue/NotAuthorized.aspx");
				return false;
			}

			bool isPermissionGranted;

			if (_attribute.Required == Required.All)
				isPermissionGranted = administrator.HavePermisions(_attribute.PermissionTypes);
			else
				isPermissionGranted = administrator.HaveAnyOfPermissions(_attribute.PermissionTypes);

			if (!isPermissionGranted) {
				context.Response.RedirectToUrl("~/Rescue/NotAllowed.aspx");
				return false;
			}

			var mayBeActions = controllerContext.ControllerDescriptor.Actions[controllerContext.Action];
			MethodInfo action;
			if (mayBeActions is ArrayList)
				action = (MethodInfo)((ArrayList)mayBeActions)[0];
			else
				action = (MethodInfo)mayBeActions;

			if (action == null)
				return true;

			if (!Permission.CheckPermissionByAttribute(administrator, action)) {
				context.Response.RedirectToUrl("~/Rescue/NotAllowed.aspx");
				return false;
			}

			return true;
		}

		public FilterAttribute Filter
		{
			set { _attribute = (SecureAttribute)value; }
		}
	}
}