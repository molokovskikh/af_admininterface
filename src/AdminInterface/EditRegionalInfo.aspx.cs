using System;
using System.Data;
using System.Web.UI;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;
using MySql.Data.MySqlClient;


public partial class EditRegionalInfo : Page
{
	private int _regionalSettingsCode
	{
		get { return Convert.ToInt32(Session["RegionalSettingsCode"]); }
		set { Session["RegionalSettingsCode"] = value; }
	}
	
	private int _clientCode
	{
		get { return Convert.ToInt32(Session["ClientCode"]); }
		set { Session["ClientCode"] = value; }
	}
	
    protected void Page_Load(object sender, EventArgs e)
    {
		StateHelper.CheckSession(this, ViewState);
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
		var commandText = @"
SELECT Region, rd.RegionCode, ShortName, ContactInfo, OperativeInfo, rd.FirmCode
FROM usersettings.regionaldata rd
	INNER JOIN usersettings.clientsdata cd ON cd.FirmCode = rd.FirmCode
		INNER JOIN farm.regions r on r.RegionCode = rd.RegionCode
WHERE RowID = ?Id";

		using (var connection = new MySqlConnection(Literals.GetConnectionString()))
		{
			connection.Open();
			var command = new MySqlCommand(commandText);
			command.Parameters.AddWithValue("?Id", _regionalSettingsCode);
			using (var reader = command.ExecuteReader())
			{
				reader.Read();

				ContactInfoText.Text = reader.GetString("ContactInfo");
				OperativeInfoText.Text = reader.GetString("OperativeInfo");
				_clientCode = reader.GetInt32("FirmCode");
				ClientInfoLabel.Text = String.Format("Информация клиента \"{0}\"", reader.GetString("ShortName"));
				RegionInfoLabel.Text = String.Format("В регионе: {0}", reader.GetString("Region"));

				var regionCode = reader.GetUInt64("RegionCode");
				SecurityContext.Administrator.CheckClientHomeRegion(regionCode);
			}
		}
	}
	
	protected void SaveButton_Click(object sender, EventArgs e)
	{
		var commandText = @"
UPDATE usersettings.regionaldata
SET ContactInfo = ?ContactInformation, 
	OperativeInfo = ?Information
WHERE RowId = ?Id;";
		var command = new MySqlCommand(commandText);
		command.Parameters.AddWithValue("?Id", _regionalSettingsCode);
		command.Parameters.AddWithValue("?ContactInformation", ContactInfoText.Text);
		command.Parameters.AddWithValue("?Information", OperativeInfoText.Text);
		command.ExecuteNonQuery();
		Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
	}
	
	protected void CancelButton_Click(object sender, EventArgs e)
	{
		Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
	}
}
