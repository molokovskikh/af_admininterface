using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;
using Common.Web.Ui.Models;
using NHibernate.Criterion;

public partial class EditAdministrator : Page
{
	protected Administrator _current { get; set; }

	protected void Page_Load(object sender, EventArgs e)
    {
		SecurityContext.Administrator.CheckPermisions(PermissionType.ManageAdministrators);

    	if (IsPostBack)
			return;

		if (Convert.ToInt32(Request["id"]) > 0)
			_current = Administrator.GetById(Convert.ToUInt32(Request["id"]));
		else
			_current = new Administrator { AllowedPermissions = new List<Permission>() };

    	var regions = Region.FindAll(Order.Asc("Name"));
    	var permissions = Permission.FindAll();

    	RegionSelector.DataSource = regions;
    	PermissionSelector.DataSource = permissions;

		DataBind();

		foreach (ListItem item in RegionSelector.Items)
			if ((Convert.ToUInt64(item.Value) & _current.RegionMask) > 0)
				item.Selected = true;

		foreach (ListItem item in PermissionSelector.Items)
			if (_current.HavePermisions((PermissionType) Enum.Parse(typeof(PermissionType), item.Value)))
				item.Selected = true;
    }

	protected void Save_Click(object sender, EventArgs e)
	{
		if (!IsValid)
			return;

		var permissions = Permission.FindAll();
		var id = Convert.ToUInt32(Request["id"]);
		Administrator admin;
		if (id == 0)
			admin = new Administrator { AllowedPermissions = new List<Permission>() };
		else
			admin = Administrator.GetById(id);

		admin.UserName = Login.Text;
		admin.ManagerName = FIO.Text;
		admin.PhoneSupport = Phone.Text;
		admin.Email = Email.Text;

		foreach (ListItem item in RegionSelector.Items)
		{
			if (item.Selected)
				admin.RegionMask |= Convert.ToUInt64(item.Value);
			else
				admin.RegionMask &= ~Convert.ToUInt64(item.Value);
		}

		foreach (ListItem item in PermissionSelector.Items)
		{
			var permissionType = (PermissionType) Enum.Parse(typeof(PermissionType), item.Value);
			var permission = FindPermission(permissionType, admin.AllowedPermissions);
			if (item.Selected)
			{
				if (permission == null)
					admin.AllowedPermissions.Add(FindPermission(permissionType, permissions));
			}
			else
			{
				if (permission != null)
					admin.AllowedPermissions.Remove(permission);
			}
		}

		if (admin.Id == 0)
		{
			admin.Save();
			CreateUserInAD(admin);
			Response.Redirect("OfficeUserRegistrationReport.aspx");
		}
		else
		{
			admin.Update();
			Response.Redirect("ViewAdministrators.aspx");
		}
	}

	private Permission FindPermission(PermissionType type, IEnumerable<Permission> permissions)
	{
		return (from userPermission in permissions
		        where type == userPermission.Type
		        select userPermission).FirstOrDefault();
	}

	private void CreateUserInAD(Administrator administrator)
	{
		var password = Func.GeneratePassword();
		var isLoginExists = ADHelper.IsLoginExists(administrator.UserName);

		if (!isLoginExists)
			ADHelper.CreateAdministratorInAd(administrator, password);

		Session["IsLoginCreate"] = !isLoginExists;
		Session["Password"] = password;
		Session["FIO"] = administrator.ManagerName;
		Session["Login"] = administrator.UserName;
	}

	protected void Cancel_Click(object sender, EventArgs e)
	{
		Response.Redirect("ViewAdministrators.aspx");
	}
}
