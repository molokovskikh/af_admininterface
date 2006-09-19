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
		if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			Response.Redirect("default.aspx");
    	
		
		if (!IsPostBack)
		{
			int regionalSettingsCode = -1;
			if (int.TryParse(Request["id"], out regionalSettingsCode))
			{
				_regionalSettingsCode = regionalSettingsCode;
				BindData();
			}
			else
				throw new ArgumentException(string.Format("Не верный аргумент id = {0}", Request["id"]), "id");
		}
    }

	private void BindData()
	{	
		string commandText =
@"
SELECT Region, ShortName, ContactInfo, OperativeInfo, rd.FirmCode
FROM usersettings.regionaldata rd
	INNER JOIN usersettings.clientsdata cd ON cd.FirmCode = rd.FirmCode
		INNER JOIN farm.regions r on r.RegionCode = rd.RegionCode
WHERE RowID = ?Id
";
		IDataCommand command = new DataCommand(commandText, IsolationLevel.ReadCommitted);
		command.Parameters.Add("Id", _regionalSettingsCode);
		command.Execute();
		
		if (command.Data.Tables[0].Rows.Count == 0)
			throw new ArgumentException(string.Format("Записи с id = {0}, не существует", _clientCode), "id");
		
		ContactInfoText.Text = command.Data.Tables[0].Rows[0]["ContactInfo"].ToString();
		OperativeInfoText.Text = command.Data.Tables[0].Rows[0]["OperativeInfo"].ToString();
		_clientCode = Convert.ToInt32(command.Data.Tables[0].Rows[0]["FirmCode"]);
		ClientInfoLabel.Text = String.Format("Информация клиента \"{0}\"", command.Data.Tables[0].Rows[0]["ShortName"]);
		RegionInfoLabel.Text = String.Format("В регионе: {0}", command.Data.Tables[0].Rows[0]["Region"]);
	}
	
	protected void SaveButton_Click(object sender, EventArgs e)
	{
		string commandText =
@"
UPDATE usersettings.regionaldata
SET ContactInfo = ?ContactInformation, 
	OperativeInfo = ?Information
WHERE RowId = ?Id;
";
		IParametericCommand command = new ParametericCommand(commandText, IsolationLevel.RepeatableRead);
		command.Parameters.Add("Id", _regionalSettingsCode);
		command.Parameters.Add("ContactInformation", ContactInfoText.Text);
		command.Parameters.Add("Information", OperativeInfoText.Text);
		command.Execute();
		Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
	}
	
	protected void CancelButton_Click(object sender, EventArgs e)
	{
		Response.Redirect(String.Format("managep.aspx?cc={0}", _clientCode));
	}
}
