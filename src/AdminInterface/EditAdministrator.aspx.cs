using System;
using System.DirectoryServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using AddUser;
using AdminInterface.Helpers;
using DAL;

public partial class EditAdministrator : Page
{
	protected Administrator _current
	{
		get { return (Administrator)Session["Administrator"]; }
		set { Session["Administrator"] = value; }
	}

    protected void Page_Load(object sender, EventArgs e)
    {
		if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			Response.Redirect("default.aspx");

		if (Convert.ToInt32(Request["id"]) > 0)
			_current = CommandFactory.GetAdministrator(Convert.ToInt32(Request["id"]));
		else 
			_current = new Administrator();
		if (!IsPostBack)
		{
			DataBind();
			foreach (ListItem item in Regions.Items)
				if ((Convert.ToUInt64(item.Value) & _current.RegionMask) > 0)
					item.Selected = true;
		}
    }

	protected void Save_Click(object sender, EventArgs e)
	{
		if (!IsValid)
			return;

		_current.Login = Login.Text;
		_current.FIO = FIO.Text;
		_current.Phone = Phone.Text;
		_current.Email = Email.Text;
		_current.DefaultSegment = Convert.ToInt32(DefaultSegment.SelectedValue);

		_current.RegionMask = 0;
		foreach (ListItem item in Regions.Items)
			if (item.Selected)
				_current.RegionMask = _current.RegionMask | Convert.ToUInt64(item.Value);

		_current.AllowManageAdminAccounts = AllowManageAdminAccounts.Checked;
		_current.AllowCreateRetail = AllowCreateRetail.Checked;
		_current.AllowCreateVendor = AllowCreateVendor.Checked;
		_current.AllowRetailInterface = AllowRetailInterface.Checked;
		_current.AllowVendorInterface = AllowVendorInterface.Checked;
		_current.AllowChangeSegment = AllowChangeSegment.Checked;
		_current.AllowManage = AllowManage.Checked;
		_current.AllowClone = AllowClone.Checked;
		_current.AllowChangePassword = AllowChangePassword.Checked;
		_current.AllowCreateInvisible = AllowCreateInvisible.Checked;
		_current.SendAlert = SendAlert.Checked;
		_current.UseRegistrant = UseRegistrant.Checked;
		_current.ShowRetail = ShowRetail.Checked;
		_current.ShowVendor = ShowVendor.Checked;

		if (_current.ID != -1)
			CommandFactory.UpdateAdministrator(_current);
		else
		{
			CommandFactory.AddAdministrator(_current);
			CreateUserInAD(_current);
			Response.Redirect("OfficeUserRegistrationReport.aspx");
		}

		Response.Redirect("ViewAdministrators.aspx");
	}

	private void CreateUserInAD(Administrator administrator)
	{
		var password = Func.GeneratePassword();
		var isLoginExists = ADHelper.IsLoginExists(administrator.Login);
		if (!isLoginExists)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Региональные администраторы,OU=Управляющие,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Пользователи офиса,OU=Уровни доступа,OU=Офис,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + administrator.Login, "user");
			user.Properties["samAccountName"].Value = administrator.Login;
			if (!String.IsNullOrEmpty(administrator.FIO.Trim()))
				user.Properties["sn"].Value = administrator.FIO;
			user.Properties["logonHours"].Value = ADHelper.LogonHours();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
		}

		Session["IsLoginCreate"] = !isLoginExists;
		Session["Password"] = password;
		Session["FIO"] = administrator.FIO;
		Session["Login"] = administrator.Login;
	}

	protected void Cancel_Click(object sender, EventArgs e)
	{
		Response.Redirect("ViewAdministrators.aspx");
	}
}
