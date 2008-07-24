using System.Collections;
using System.Reflection;
using AdminInterface.Models;
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
			if (SecurityContext.Administrator == null)
			{
				context.Response.RedirectToUrl("../Rescue/NotAuthorized.aspx");
				return false;
			}

			bool isPermissionGranted;

			if (_attribute.Required == Required.All)
				isPermissionGranted = SecurityContext.Administrator.HavePermisions(_attribute.PermissionTypes);
			else
				isPermissionGranted = SecurityContext.Administrator.HaveAnyOfPermissions(_attribute.PermissionTypes);

			if (!isPermissionGranted)
			{
				context.Response.RedirectToUrl("../Rescue/NotAllowed.aspx");
				return false;
			}

			var mayBeActions = controllerContext.ControllerDescriptor.Actions[controllerContext.Action];
			MethodInfo action;
			if (mayBeActions is ArrayList)
				action = (MethodInfo) ((ArrayList) mayBeActions)[0];
			else
				action = (MethodInfo) mayBeActions;

			var attributes = action.GetCustomAttributes(typeof (RequiredPermissionAttribute), true);

			foreach (RequiredPermissionAttribute attribute in attributes)
			{
				if (attribute.Required == Required.All)
					isPermissionGranted = SecurityContext.Administrator.HavePermisions(attribute.PermissionTypes);
				else
					isPermissionGranted = SecurityContext.Administrator.HaveAnyOfPermissions(attribute.PermissionTypes);

				if (!isPermissionGranted)
				{
					context.Response.RedirectToUrl("../Rescue/NotAllowed.aspx");
					return false;
				}				
			}

			return true;			
		}

		public FilterAttribute Filter
		{
			set { _attribute = (SecureAttribute) value; }
		}
	}
}