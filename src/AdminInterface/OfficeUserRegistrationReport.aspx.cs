using System;
using System.Web.UI;

namespace AdminInterface
{
	public partial class OfficeUserRegistrationReport : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			LBShortName.Text = Session["FIO"].ToString();
			LBPassword.Text = Session["Password"].ToString();
			LBLogin.Text = Session["Login"].ToString();
			RegDate.Text = DateTime.Now.ToString();
		}
	}
}
