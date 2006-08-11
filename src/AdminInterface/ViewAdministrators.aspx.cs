using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DAL;

public partial class ViewAdministrators : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			Response.Redirect("default.aspx");
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
		}
	}

	protected void Administrators_RowCreated(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType == DataControlRowType.DataRow)
		{ 
			e.Row.Attributes.Add("onmouseout","return SetClass(this, '');");
			e.Row.Attributes.Add("onmouseover", "return SetClass(this, 'SelectedRow');");
		}
	}
}
