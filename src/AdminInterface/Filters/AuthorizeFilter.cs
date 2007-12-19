using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
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
				Administrator admin = Administrator.GetByName(context.CurrentUser.Identity.Name);
				if (admin == null)
					return false;
				controller.Context.Session["Admin"] = admin;
			}
			return true;
		}
	}
}
