using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;

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
			case "Block":
				{
					ADHelper.Block(e.CommandArgument.ToString());
					Response.Redirect("ViewAdministrators.aspx");
					break;
				}
			case "Unblock":
				{
					ADHelper.Unlock(e.CommandArgument.ToString());
					Response.Redirect("ViewAdministrators.aspx");
					break;
				}
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
		if (ADHelper.IsLocked(login))
			return "Unblock";
		return "Block";
	}

	protected bool GetDeleteBlockButtonVisibiliti(string login)
	{
		if (login == "Boss" || login == "michail")
			return false;
		return true;
	}
}
