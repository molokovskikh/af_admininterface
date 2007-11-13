using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.DirectoryServices;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
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
			CheckUserGroup(_current.Login);
			CommandFactory.AddAdministrator(_current);
		}

		Response.Redirect("ViewAdministrators.aspx");
	}

	private void CheckUserGroup(string userName)
	{
		string groupName = "Региональные администраторы";
		bool finded = false;
		using (DirectorySearcher searcher = new DirectorySearcher())
		{
			searcher.Filter = String.Format("(&(objectClass=user)(name={0}))", userName);
			SearchResult searchResult = searcher.FindOne();

			searcher.Filter = "(&(objectClass=group)(name=Региональные администраторы))";
			SearchResult groupResult = searcher.FindOne();
			
			using(DirectoryEntry group = groupResult.GetDirectoryEntry())
			using (DirectoryEntry user  = searchResult.GetDirectoryEntry())
			{
				
				foreach (string value in user.Properties["MemberOf"])
				{
					if (value.IndexOf(groupName, StringComparison.CurrentCultureIgnoreCase) > 0)
					{
						finded = true;
						break;
					}
				}

				if (!finded)
					group.Invoke("Add", user.Path);

				user.CommitChanges();
			}
		}
	}

	protected void Cancel_Click(object sender, EventArgs e)
	{
		Response.Redirect("ViewAdministrators.aspx");
	}

	protected void LoginValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		using (DirectorySearcher searcher = new DirectorySearcher(String.Format("(&(objectClass=user)(name={0}))", args.Value)))
		{
			SearchResult searchResult = searcher.FindOne();
			if (searchResult == null)
			{
				args.IsValid = false;
				return;
			}
		}
		args.IsValid = true;
		return;
	}
}
