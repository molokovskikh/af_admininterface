using AdminInterface.Models;
using Castle.MonoRail.Framework;

namespace AdminInterface.Filters
{
	public class AuthorizeFilter : IFilter
	{
		public bool Perform(ExecuteEnum exec, IRailsEngineContext context, Controller controller)
		{
			if (controller.Context.Session["Admin"] == null)
			{
				var admin = Administrator.GetByName(context.CurrentUser.Identity.Name);
				if (admin == null)
				{
					controller.Response.StatusCode = 403;
					return false;
				}
				controller.Context.Session["Admin"] = admin;
			}
			return true;
		}
	}
}
