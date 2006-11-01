using System;
using System.Web.UI;

namespace AddUser
{
	partial class Error : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			
		}
		protected void BackButton_Click(object sender, EventArgs e)
		{
			Response.Redirect(Request["aspxerrorpath"]);
		}
}
}