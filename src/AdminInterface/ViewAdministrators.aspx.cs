using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;

public partial class ViewAdministrators : Page
{
	private IList<Permission> permissions;

    protected void Page_Load(object sender, EventArgs e)
    {
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		SecurityContext.Administrator.CheckPermisions(PermissionType.ManageAdministrators);

    	permissions = Permission.FindAll();

		if (IsPostBack)
			return;

    	var administrators = Administrator.FindAll();
    	Administrators.DataSource = administrators;
		DataBind();
    }

	protected void Administrators_RowCommand(object sender, GridViewCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "Create":
				{
					Response.Redirect("EditAdministrator.aspx");
					break;
				}
			case "Edit":
				{
					Response.Redirect(String.Format("EditAdministrator.aspx?id={0}", e.CommandArgument));
					break;
				}
			case "Disable":
				{
					ADHelper.Disable(e.CommandArgument.ToString());
					Response.Redirect("ViewAdministrators.aspx");
					break;
				}
			case "Enable":
				{
					ADHelper.Enable(e.CommandArgument.ToString());
					Response.Redirect("ViewAdministrators.aspx");
					break;
				}
			case "Del":
				var administrator = Administrator.GetById(Convert.ToUInt32(e.CommandArgument));
				administrator.Delete();
				Response.Redirect("ViewAdministrators.aspx");
				break;
		}
	}

	protected string GetButtonLabel(string login)
	{
		if (ADHelper.IsLocked(login))
			return "Разблокировать";
		return "Блокировать";
	}

	protected string GetButtonCommand(string login)
	{
		if (ADHelper.IsDisabled(login))
			return "Enable";
		return "Disable";
	}

	protected bool GetDeleteBlockButtonVisibiliti(string login)
	{
		if (login == "Boss" || login == "michail")
			return false;
		return true;
	}

	protected string GetPermissionShortcut(PermissionType permissionType)
	{
		return (from permission in permissions
		       where permissionType == permission.Type
		       select permission.Shortcut).First();
	}

	protected string GetPermissionName(PermissionType permissionType)
	{
		return (from permission in permissions
				where permissionType == permission.Type
				select permission.Name).First();
	}
}
