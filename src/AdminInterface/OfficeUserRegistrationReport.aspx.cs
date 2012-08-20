using System;
using System.Web.UI;
using AdminInterface.Models.Security;
using AdminInterface.Security;

namespace AdminInterface
{
	public partial class OfficeUserRegistrationReport : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.Administrator.CheckPermisions(PermissionType.ManageAdministrators);

			LBShortName.Text = Session["FIO"].ToString();
			LBPassword.Text = Session["Password"].ToString();
			LBLogin.Text = Session["Login"].ToString();
			RegDate.Text = DateTime.Now.ToString();
		}
	}
}