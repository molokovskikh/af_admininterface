using System;
using System.Web.UI;
using AdminInterface.Security;

namespace AddUser
{
	partial class Error : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var exception = Server.GetLastError();
			if (exception is NotAuthorizedException)
				Response.Redirect("NotAuthorized.aspx");
			else if (exception is NotHavePermissionException)
				Response.Redirect("NotAllowed.aspx");
		}

		protected void BackButton_Click(object sender, EventArgs e)
		{
			if (Request["aspxerrorpath"] == null)
				Response.Redirect("~/default.aspx");
			else
				Response.Redirect(Request["aspxerrorpath"]);
		}
}
}