using AdminInterface.Models;
using Castle.MonoRail.Framework;

namespace AdminInterface.Security
{
	public class AuthorizeFilter : IFilter
	{
		public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller,
		                    IControllerContext controllerContext)
		{
			if (context.Session["Admin"] == null)
			{
				var admin = Administrator.GetByName(context.CurrentUser.Identity.Name);
				if (admin == null)
				{
					context.Response.StatusCode = 403;
					return false;
				}
				context.Session["Admin"] = admin;
			}
			return true;			
		}
	}
}