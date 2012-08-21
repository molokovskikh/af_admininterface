using System;
using System.Data;
using System.Web.UI;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using MySql.Data.MySqlClient;


public partial class EditRegionalInfo : Page
{
	private int _regionalSettingsCode
	{
		get { return Convert.ToInt32(Session["RegionalSettingsCode"]); }
		set { Session["RegionalSettingsCode"] = value; }
	}

	private uint _clientCode
	{
		get { return Convert.ToUInt32(Session["ClientCode"]); }
		set { Session["ClientCode"] = value; }
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		SecurityContext.Administrator.CheckPermisions(PermissionType.ManageSuppliers, PermissionType.ViewSuppliers);

		if (IsPostBack)
			return;
		int regionalSettingsCode;
		if (!int.TryParse(Request["id"], out regionalSettingsCode))
			throw new ArgumentException(string.Format("Не верный аргумент id = {0}", Request["id"]), "id");

		_regionalSettingsCode = regionalSettingsCode;
		BindData();
	}

	private void BindData()
	{
		var regionalData = ActiveRecordMediator<RegionalData>.FindByPrimaryKey(_regionalSettingsCode);
		ContactInfoText.Text = regionalData.ContactInfo;
		OperativeInfoText.Text = regionalData.OperativeInfo;
		_clientCode = regionalData.Supplier.Id;
		ClientInfoLabel.Text = String.Format("Информация клиента \"{0}\"", regionalData.Supplier.Name);
		RegionInfoLabel.Text = String.Format("В регионе: {0}", regionalData.Region.Name);
		SecurityContext.Administrator.CheckRegion(regionalData.Region.Id);
	}

	protected void SaveButton_Click(object sender, EventArgs e)
	{
		using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
			connection.Open();

			var commandText = @"
UPDATE usersettings.regionaldata
SET ContactInfo = ?ContactInformation, 
	OperativeInfo = ?Information
WHERE RowId = ?Id;";
			var command = new MySqlCommand(commandText, connection);
			command.Parameters.AddWithValue("?Id", _regionalSettingsCode);
			command.Parameters.AddWithValue("?ContactInformation", ContactInfoText.Text);
			command.Parameters.AddWithValue("?Information", OperativeInfoText.Text);
			command.ExecuteNonQuery();
			Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
		}
	}

	protected void CancelButton_Click(object sender, EventArgs e)
	{
		Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
	}
}