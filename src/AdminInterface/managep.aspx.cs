using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class managep : Page
	{
		private MySqlConnection _connection = new MySqlConnection();
		
		private string _userName
		{
			get { return (string) Session["UserName"]; }
			set { Session["UserName"] = value; }
		}

		private DataSet _data
		{
			get { return (DataSet)Session["RegionalSettingsData"]; }
			set { Session["RegionalSettingsData"] = value; }
		}

		private int _clientCode
		{
			get { return Convert.ToInt32(Session["ClientCode"]); }
			set { Session["ClientCode"] = value; }
		}
		
		private ulong _homeRegion
		{
			get { return (ulong) Session["RegionalSettingsHomeRegion"]; }
			set { Session["RegionalSettingsHomeRegion"] = value;}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");
		
			_connection.ConnectionString = Literals.GetConnectionString();

			if (!IsPostBack)
			{
				int clientCode;
				if (Int32.TryParse(Request["cc"], out clientCode))
					_clientCode = clientCode;
				else
					throw new ArgumentException(String.Format("Не верное значение ClientCode = {0}", clientCode), "ClientCode");

				GetData();
				ConnectDataSource();
				DataBind();
				SetRegions();
			}
			else
				ConnectDataSource();

		}

		private void ConnectDataSource()
		{
			PricesGrid.DataSource = _data;
			RegionalSettingsGrid.DataSource = _data;
			WorkRegionList.DataSource = _data;
			HomeRegion.DataSource = _data;
		}

		private void GetData()
		{
			_homeRegion = GetHomeRegion();
			if (_data == null)
				_data = new DataSet();
			else 
				_data.Clear();
			string pricesCommandText =
@"
SELECT  cd.firmcode, 
        ShortName, 
        pricesdata.PriceCode, 
        PriceName, 
        pricesdata.AgencyEnabled, 
        pricesdata.Enabled, 
        AlowInt,  
        DateCurPrice, 
        DateLastForm,
		UpCost,
		PriceType
FROM    (clientsdata as cd, farm.regions, accessright.regionaladmins, pricesdata, farm.formrules fr,  pricescosts pc)
WHERE   regions.regioncode                            =cd.regioncode  
        AND pricesdata.firmcode                       =cd.firmcode 
        AND pricesdata.pricecode                      =fr.firmcode 
        AND pc.showpricecode                          =pricesdata.pricecode 
        AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName                   =?UserName  
        AND if(UseRegistrant                          =1, Registrant=@UserName, 1=1)  
        AND AlowManage                                =1  
        AND AlowCreateVendor                          =1  
        AND cd.firmcode                               =?ClientCode 
GROUP BY 3;
";
			string regionSettingsCommnadText =
@"
SELECT  RowID, 
		r.RegionCode,
        Region,
        Enabled, 
        `Storage`, 
        AdminMail, 
        TmpMail, 
        SupportPhone, 
        ContactInfo, 
        OperativeInfo  
FROM    usersettings.regionaldata rd  
INNER JOIN farm.regions r 
        ON rd.regioncode = r.regioncode  
WHERE   rd.FirmCode      = ?ClientCode;
";
			string regionsCommandText =
@"
SELECT  RegionCode,   
        Region
FROM Farm.Regions
ORDER BY region;
";
			string enableRegionsCommandText =
@"
SELECT  a.RegionCode,
        a.Region, 
        MaskRegion & a.regioncode     >0 as Enable
FROM    farm.regions                     as a, 
        farm.regions                     as b, 
        clientsdata, 
        accessright.regionaladmins
WHERE   b.regioncode                                             =?HomeRegion
        AND  clientsdata.firmcode                                =?ClientCode
        AND a.regioncode & (b.defaultshowregionmask | MaskRegion)>0
        AND regionaladmins.username                              =?UserName
        AND a.regioncode & regionaladmins.RegionMask             > 0
GROUP BY regioncode
ORDER BY region;
";

			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(pricesCommandText, _connection);
			dataAdapter.SelectCommand.Parameters.Add("ClientCode", _clientCode);
			dataAdapter.SelectCommand.Parameters.Add("UserName", _userName);
			dataAdapter.SelectCommand.Parameters.Add("HomeRegion", _homeRegion);
			
			dataAdapter.Fill(_data, "Prices");

			dataAdapter.SelectCommand.CommandText = regionSettingsCommnadText;
			dataAdapter.Fill(_data, "RegionSettings");

			dataAdapter.SelectCommand.CommandText = regionsCommandText;
			dataAdapter.Fill(_data, "Regions");

			dataAdapter.SelectCommand.CommandText = enableRegionsCommandText;
			dataAdapter.Fill(_data, "EnableRegions");


			HeaderLabel.Text = String.Format("Конфигурация клиента \"{0}\"", _data.Tables["Prices"].Rows[0]["ShortName"].ToString());
		}


		private void SetRegions()
		{
			HomeRegion.SelectedValue = _homeRegion.ToString();
			for (int i = 0; i < _data.Tables["EnableRegions"].Rows.Count; i++)
				WorkRegionList.Items[i].Selected = Convert.ToBoolean(_data.Tables["EnableRegions"].Rows[i]["Enable"]);
		}

		protected void PricesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					DataRow row = _data.Tables["Prices"].NewRow();
					row["UpCost"] = 0;
					row["Enabled"] = false;
					row["AgencyEnabled"] = false;
					row["AlowInt"] = false;
					_data.Tables["Prices"].Rows.Add(row);
					((GridView)sender).DataBind();
					break;
			}

		}
		protected void PricesGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			_data.Tables["Prices"].Rows[e.RowIndex].Delete();
			((GridView)sender).DataBind();
		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			ShowAllRegionsCheck.Checked = false;
			UpdateHomeRegion();
			UpdateMaskRegion();
			ChangesToDataSet();
			MySqlDataAdapter pricesDataAdapter = new MySqlDataAdapter("", _connection);
			pricesDataAdapter.DeleteCommand = new MySqlCommand(
@"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;

DELETE FROM PricesData
WHERE PriceCode = ?PriceCode;

DELETE FROM PricesData
WHERE PriceCode in (select PriceCode from PricesCosts where ShowPriceCode = ?PriceCode);

DELETE FROM Intersection
WHERE PriceCode = ?PriceCode;

DELETE FROM PricesRegionalData
WHERE PriceCode = ?PriceCode;
", _connection);

			pricesDataAdapter.DeleteCommand.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.DeleteCommand.Parameters.Add("UserName", _userName);
			pricesDataAdapter.DeleteCommand.Parameters.Add("PriceCode", MySqlDbType.Int32, 0, "PriceCode");

			pricesDataAdapter.InsertCommand = new MySqlCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

INSERT INTO PricesData
SET UpCost = ?UpCost,
	PriceType = ?PriceType,
	Enabled = ?Enabled,
	AgencyEnabled = ?AgencyEnabled,
	AlowInt = ?AlowInt,
	FirmCode = ?ClientCode;

SET @InsertedPriceCode = Last_Insert_ID();
INSERT INTO farm.formrules(firmcode) VALUES(@InsertedPriceCode); 
INSERT INTO farm.sources(FirmCode) VALUES(@InsertedPriceCode);  

INSERT INTO pricesdata(Firmcode, PriceCode) VALUES(?ClientCode , null);  
set @NewPriceCode:=Last_Insert_ID(); 
INSERT INTO farm.formrules(firmcode) VALUES(@NewPriceCode); 
INSERT INTO farm.sources(FirmCode) VALUES(@NewPriceCode);  
			
INSERT 
INTO    PricesCosts
        (
                CostCode, 
                PriceCode, 
                BaseCost, 
                ShowPriceCode
        ) 
SELECT  @NewPriceCode, 
        @NewPriceCode, 
        1, 
        @InsertedPriceCode;  
INSERT INTO farm.costformrules(PC_CostCode, FR_ID) SELECT @NewPriceCode, @NewPriceCode;

INSERT 
INTO    pricesregionaldata
        (
                regioncode, 
                pricecode, 
                enabled
        )    
SELECT  r.RegionCode,
		p.PriceCode, 
        if(p.pricetype<>1, 1, 0) 
FROM    pricesdata p,   
        clientsdata cd,   
        farm.regions r  
WHERE   p.PriceCode  = @InsertedPriceCode
        AND p.FirmCode = cd.FirmCode  
        AND exists
        (SELECT * 
        FROM    pricescosts pc 
        WHERE   pc.ShowPriceCode = p.PriceCode
        )  
        AND (r.RegionCode & cd.MaskRegion > 0)  
        AND not exists
        (SELECT * 
        FROM    pricesregionaldata prd 
        WHERE   prd.PriceCode      = p.PriceCode 
                AND prd.RegionCode = r.RegionCode
        );

INSERT 
INTO    intersection
        (
                ClientCode, 
                regioncode, 
                pricecode, 
                invisibleonclient, 
                InvisibleonFirm, 
                costcode
        )    
SELECT  DISTINCT clientsdata2.firmcode, 
        regions.regioncode, 
        pricesdata.pricecode,    
        pricesdata.PriceType=2 as invisibleonclient, 
        a.invisibleonfirm, 
        (SELECT costcode 
        FROM    pricescosts pcc 
        WHERE   basecost    
                AND showpricecode=pc.showpricecode
        )    
FROM    (clientsdata, 
        farm.regions, 
        pricesdata, 
        pricesregionaldata, 
        clientsdata as clientsdata2, 
        pricescosts pc)
LEFT JOIN intersection 
        ON intersection.pricecode  =pricesdata.pricecode 
        AND intersection.regioncode=regions.regioncode 
        AND intersection.clientcode=clientsdata2.firmcode    
LEFT JOIN retclientsset as a 
        ON a.clientcode=clientsdata2.firmcode    
WHERE   PricesData.PriceCode							  = @InsertedPriceCode
		AND intersection.pricecode IS NULL    
        AND clientsdata.firmsegment                       =clientsdata2.firmsegment    
        AND clientsdata.firmtype                          =0    
        AND pricesdata.firmcode                           =clientsdata.firmcode    
        AND pricesregionaldata.pricecode                  =pricesdata.pricecode    
        AND pricesregionaldata.regioncode                 =regions.regioncode    
        AND pricesdata.pricetype                         <>1    
        AND pricesdata.pricecode                          =pc.showpricecode    
        AND (clientsdata.maskregion & regions.regioncode) >0    
        AND (clientsdata2.maskregion & regions.regioncode)>0    
        AND clientsdata2.firmtype                         =1;
", _connection);
			pricesDataAdapter.InsertCommand.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.InsertCommand.Parameters.Add("UserName", _userName);
			pricesDataAdapter.InsertCommand.Parameters.Add("ClientCode", _clientCode);
			pricesDataAdapter.InsertCommand.Parameters.Add("UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.InsertCommand.Parameters.Add("PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.InsertCommand.Parameters.Add("Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("AlowInt", MySqlDbType.Bit, 0, "AlowInt");
			pricesDataAdapter.InsertCommand.Parameters.Add("PriceCode", MySqlDbType.Int32, 0, "PriceCode");

			pricesDataAdapter.UpdateCommand = new MySqlCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

UPDATE pricesdata
SET UpCost = ?UpCost,
	PriceType = ?PriceType,
	Enabled = ?Enabled,
	AgencyEnabled = ?AgencyEnabled,
	AlowInt = ?AlowInt
WHERE PriceCode = ?PriceCode;
", _connection);
			pricesDataAdapter.UpdateCommand.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.UpdateCommand.Parameters.Add("UserName", _userName);
			pricesDataAdapter.UpdateCommand.Parameters.Add("UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.UpdateCommand.Parameters.Add("PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.UpdateCommand.Parameters.Add("Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("AlowInt", MySqlDbType.Bit, 0, "AlowInt");
			pricesDataAdapter.UpdateCommand.Parameters.Add("PriceCode", MySqlDbType.Int32, 0, "PriceCode");

			MySqlDataAdapter regionalSettingsDataAdapter = new MySqlDataAdapter("", _connection);
			regionalSettingsDataAdapter.UpdateCommand = new MySqlCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;
UPDATE usersettings.regionaldata
SET AdminMail = ?AdminMail,
	TmpMail = ?TmpMail,
	SupportPhone = ?SupportPhone,
	Enabled = ?Enabled,
	`Storage` = ?Storage
WHERE RowId = ?Id;
", _connection);
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("AdminMail", MySqlDbType.VarString, 0, "AdminMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("TmpMail", MySqlDbType.VarString, 0, "TmpMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("SupportPhone", MySqlDbType.VarString, 0, "SupportPhone");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("Enabled", MySqlDbType.Bit, 0, "Enabled");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("Storage", MySqlDbType.Bit, 0, "Storage");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("Id", MySqlDbType.Int32, 0, "RowID");

			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("UserName", _userName);
			MySqlTransaction transaction = null;
			try
			{
				_connection.Open();
				transaction = _connection.BeginTransaction();

				pricesDataAdapter.InsertCommand.Transaction = transaction;
				pricesDataAdapter.UpdateCommand.Transaction = transaction;
				pricesDataAdapter.DeleteCommand.Transaction = transaction;
				regionalSettingsDataAdapter.UpdateCommand.Transaction = transaction;

				pricesDataAdapter.Update(_data.Tables["Prices"]);
				regionalSettingsDataAdapter.Update(_data.Tables["RegionSettings"]);

				transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw;
			}
			finally
			{
				_connection.Close();
			}
			
			GetData();
			ConnectDataSource();
			DataBind();
			SetRegions();
		}

		private void ChangesToDataSet()
		{
			for (int i = 0; i < RegionalSettingsGrid.Rows.Count; i++)
			{
				if (Convert.ToBoolean(_data.Tables["RegionSettings"].Rows[i]["Enabled"]) != ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("EnabledCheck")).Checked)
					_data.Tables["RegionSettings"].Rows[i]["Enabled"] = ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("EnabledCheck")).Checked;
				if (Convert.ToBoolean(_data.Tables["RegionSettings"].Rows[i]["Storage"]) != ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("StorageCheck")).Checked)
					_data.Tables["RegionSettings"].Rows[i]["Storage"] = ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("StorageCheck")).Checked;
				if (Convert.ToString(_data.Tables["RegionSettings"].Rows[i]["AdminMail"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("AdministratorEmailText")).Text)
					_data.Tables["RegionSettings"].Rows[i]["AdminMail"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("AdministratorEmailText")).Text;
				if (Convert.ToString(_data.Tables["RegionSettings"].Rows[i]["TmpMail"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("RegionalEmailText")).Text)
					_data.Tables["RegionSettings"].Rows[i]["TmpMail"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("RegionalEmailText")).Text;
				if (Convert.ToString(_data.Tables["RegionSettings"].Rows[i]["SupportPhone"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("SupportPhoneText")).Text)
					_data.Tables["RegionSettings"].Rows[i]["SupportPhone"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("SupportPhoneText")).Text;
			}
			for (int i = 0; i < PricesGrid.Rows.Count; i++)
			{
				if (Convert.ToString(_data.Tables["Prices"].Rows[i]["UpCost"]) != ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text)
					_data.Tables["Prices"].Rows[i]["UpCost"] = ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text;
				if (Convert.ToString(_data.Tables["Prices"].Rows[i]["PriceType"]) != ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue)
					_data.Tables["Prices"].Rows[i]["PriceType"] = ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue;
				if (Convert.ToBoolean(_data.Tables["Prices"].Rows[i]["Enabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked)
					_data.Tables["Prices"].Rows[i]["Enabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked;
				if (Convert.ToBoolean(_data.Tables["Prices"].Rows[i]["AgencyEnabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked)
					_data.Tables["Prices"].Rows[i]["AgencyEnabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked;
				if (Convert.ToBoolean(_data.Tables["Prices"].Rows[i]["AlowInt"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("IntegratedCheck")).Checked)
					_data.Tables["Prices"].Rows[i]["AlowInt"] = ((CheckBox)PricesGrid.Rows[i].FindControl("IntegratedCheck")).Checked;
			}
		}

		protected void ShowAllRegionsCheck_CheckedChanged(object sender, EventArgs e)
		{
			string commandText;
			if (((CheckBox)sender).Checked)
			{
				commandText =
@"
SELECT  a.RegionCode,   
        a.Region,   
        MaskRegion & a.regioncode >0 as `Enable`  
FROM    farm.regions                 as a,   
        clientsdata,   
        accessright.regionaladmins  
WHERE   clientsdata.firmcode                         =?ClientCode
        AND regionaladmins.username                  =?UserName   
        AND a.regioncode & regionaladmins.RegionMask > 0  
ORDER BY region;
";
			}
			else
			{
				commandText =
@"
SELECT  a.RegionCode,
        a.Region, 
        MaskRegion & a.regioncode     >0 as Enable
FROM    farm.regions                     as a, 
        farm.regions                     as b, 
        clientsdata, 
        accessright.regionaladmins
WHERE   b.regioncode                                             =?HomeRegion
        AND  clientsdata.firmcode                                =?ClientCode
        AND a.regioncode & (b.defaultshowregionmask | MaskRegion)>0
        AND regionaladmins.username                              =?UserName
        AND a.regioncode & regionaladmins.RegionMask             > 0
GROUP BY regioncode
ORDER BY region;
";
			}
			MySqlDataAdapter adapter = new MySqlDataAdapter(commandText, _connection);
			adapter.SelectCommand.Parameters.Add("ClientCode", _clientCode);
			adapter.SelectCommand.Parameters.Add("HomeRegion", _homeRegion);
			adapter.SelectCommand.Parameters.Add("UserName", _userName);
			_data.Tables["EnableRegions"].Clear();
			adapter.Fill(_data, "EnableRegions");
			WorkRegionList.DataBind();
			SetRegions();
		}
		
		private void UpdateMaskRegion()
		{
			ScalarCommand maskRegionCommand = new ScalarCommand(
@"
SELECT MaskRegion FROM ClientsData WHERE FirmCode = ?ClientCode;
");
			maskRegionCommand.Parameters.Add("ClientCode", _clientCode);
			maskRegionCommand.Execute();
			ulong oldMaskRegion = Convert.ToUInt64(maskRegionCommand.Result);
			ulong newMaskRegion = oldMaskRegion;
			foreach (ListItem item in WorkRegionList.Items)
				newMaskRegion = item.Selected ? newMaskRegion | Convert.ToUInt64(item.Value) : newMaskRegion & ~Convert.ToUInt64(item.Value);

			if(oldMaskRegion != newMaskRegion)
			{
				ParametericCommand updateCommand = new ParametericCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

UPDATE ClientsData
SET MaskRegion = ?MaskRegion
WHERE FirmCode = ?ClientCode;

INSERT 
INTO    pricesregionaldata
        (
                regioncode, 
                pricecode, 
                enabled
        )    
SELECT  r.RegionCode,
		p.PriceCode, 
        if(p.pricetype<>1, 1, 0) 
FROM    pricesdata p,   
        clientsdata cd,   
        farm.regions r  
WHERE     cd.FirmCode  = ?ClientCode  
        AND p.FirmCode = cd.FirmCode  
        AND exists
        (SELECT * 
        FROM    pricescosts pc 
        WHERE   pc.ShowPriceCode = p.PriceCode
        )  
        AND (r.RegionCode & cd.MaskRegion > 0)  
        AND not exists
        (SELECT * 
        FROM    pricesregionaldata prd 
        WHERE   prd.PriceCode      = p.PriceCode 
                AND prd.RegionCode = r.RegionCode
        );

INSERT INTO regionaldata  (   FirmCode,   RegionCode  )  
SELECT  cd.FirmCode, 
        r.RegionCode 
FROM    ClientsData cd, 
        Farm.Regions r 
WHERE   cd.FirmCode                       = ?ClientCode 
        AND(r.RegionCode & cd.MaskRegion) > 0 
        AND NOT exists 
        (SELECT * 
        FROM    regionaldata rd 
        WHERE   rd.FirmCode       = cd.FirmCode 
                AND rd.RegionCode = r.RegionCode
);

INSERT 
INTO    intersection
        (
                ClientCode, 
                regioncode, 
                pricecode, 
                invisibleonclient, 
                InvisibleonFirm, 
                costcode
        )    
SELECT  DISTINCT clientsdata2.firmcode, 
        regions.regioncode, 
        pricesdata.pricecode,    
        pricesdata.PriceType=2 as invisibleonclient, 
        a.invisibleonfirm, 
        (SELECT costcode 
        FROM    pricescosts pcc 
        WHERE   basecost    
                AND showpricecode=pc.showpricecode
        )    
FROM    (clientsdata, 
        farm.regions, 
        pricesdata, 
        pricesregionaldata, 
        clientsdata as clientsdata2, 
        pricescosts pc)
LEFT JOIN intersection 
        ON intersection.pricecode  =pricesdata.pricecode 
        AND intersection.regioncode=regions.regioncode 
        AND intersection.clientcode=clientsdata2.firmcode    
LEFT JOIN retclientsset as a 
        ON a.clientcode=clientsdata2.firmcode    
WHERE   clientsdata.FirmCode							  = ?ClientCode
		AND intersection.pricecode IS NULL    
        AND clientsdata.firmsegment                       =clientsdata2.firmsegment    
        AND clientsdata.firmtype                          =0    
        AND pricesdata.firmcode                           =clientsdata.firmcode    
        AND pricesregionaldata.pricecode                  =pricesdata.pricecode    
        AND pricesregionaldata.regioncode                 =regions.regioncode    
        AND pricesdata.pricetype                         <>1    
        AND pricesdata.pricecode                          =pc.showpricecode    
        AND (clientsdata.maskregion & regions.regioncode) >0    
        AND (clientsdata2.maskregion & regions.regioncode)>0    
        AND clientsdata2.firmtype                         =1;
"
				);
				updateCommand.Parameters.Add("MaskRegion", newMaskRegion);
				updateCommand.Parameters.Add("ClientCode", _clientCode);
				updateCommand.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
				updateCommand.Parameters.Add("UserName", _userName);
				updateCommand.Execute();
			}
		}
		
		private void UpdateHomeRegion()
		{
			ulong currentHomeRegion = Convert.ToUInt64(HomeRegion.SelectedValue);
			if (_homeRegion != currentHomeRegion)
			{
				ParametericCommand command = new ParametericCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

UPDATE ClientsData 
SET RegionCode = ?RegionCode
WHERE FirmCode = ?ClientCode;
");
				command.Parameters.Add("RegionCode", currentHomeRegion);
				command.Parameters.Add("ClientCode", _clientCode);
				command.Parameters.Add("UserHost", HttpContext.Current.Request.UserHostAddress);
				command.Parameters.Add("UserName", _userName);
				command.Execute();
			}
			
		}
		
		private ulong GetHomeRegion()
		{
			ScalarCommand homeRegionCommand = new ScalarCommand(
@"
SELECT RegionCode FROM ClientsData WHERE FirmCode = ?ClientCode;
");
			homeRegionCommand.Parameters.Add("ClientCode", _clientCode);
			homeRegionCommand.Execute();

			return Convert.ToUInt64(homeRegionCommand.Result);
		}

		protected void RegionalSettingsGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				DataRow[] rows = _data.Tables["EnableRegions"].Select(String.Format("RegionCode = {0}", _data.Tables["RegionSettings"].Rows[e.Row.DataItemIndex]["RegionCode"]));
				if (rows.Length > 0)
				{
					if (Convert.ToBoolean(rows[0]["Enable"]) == false)
						e.Row.BackColor = Color.FromArgb(239, 236, 201);
				}
				else
				{
					e.Row.BackColor = Color.FromArgb(239, 236, 201);
				}
			}
		}
}
}