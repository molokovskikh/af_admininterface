using System;
using System.Web.UI;

namespace AddUser
{
	partial class _error : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			Label1.Text = Convert.ToString(Application["strError"]);
		}
	}
}