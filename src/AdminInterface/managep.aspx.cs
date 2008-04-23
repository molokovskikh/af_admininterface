using System;
using System.Collections.Generic;
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
		private readonly Dictionary<object, string> _configuratedCostTypes
			= new Dictionary<object, string>
			  	{
			  		{0, "Мультиколоночный"},
			  		{1, "Многофайловый"},
			  	};

		private readonly Dictionary<object, string> _unconfiguratedCostTypes
			= new Dictionary<object, string>
			  	{
			  		{0, "Мультиколоночный"},
			  		{1, "Многофайловый"},
			  		{DBNull.Value, "Не настроенный"},
			  	};

		private readonly MySqlConnection _connection = new MySqlConnection();

		private string _userName
		{
			get { return (string) Session["UserName"]; }
			set { Session["UserName"] = value; }
		}

		private DataSet Data
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
			PricesGrid.DataSource = Data;
			RegionalSettingsGrid.DataSource = Data;
			WorkRegionList.DataSource = Data;
			HomeRegion.DataSource = Data;
			ShowClientsGrid.DataSource = Data;
		}

		private void GetData()
		{
			_homeRegion = GetHomeRegion();
			if (Data == null)
				Data = new DataSet();
			else 
				Data.Clear();
			var pricesCommandText =
@"
SELECT  cd.firmcode, 
        ShortName, 
        pricesdata.PriceCode, 
        PriceName, 
        pricesdata.AgencyEnabled, 
        pricesdata.Enabled, 
        AlowInt,  
        pi.PriceDate, 
		UpCost,
		PriceType,
		CostType
FROM (clientsdata as cd, farm.regions, accessright.regionaladmins, pricesdata)
    JOIN usersettings.pricescosts pc on pricesdata.PriceCode = pc.PriceCode and pc.BaseCost = 1
        JOIN usersettings.PriceItems pi on pi.Id = pc.PriceItemId
WHERE   regions.regioncode                            =cd.regioncode  
        AND pricesdata.firmcode                       =cd.firmcode 
        AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName                   =?UserName  
        AND if(UseRegistrant                          =1, Registrant=@UserName, 1=1)  
        AND AlowManage                                =1  
        AND AlowCreateVendor                          =1  
        AND cd.firmcode                               =?ClientCode 
GROUP BY pricesdata.PriceCode;
";
			var regionSettingsCommnadText =
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
			var regionsCommandText =
@"
SELECT  RegionCode,   
        Region
FROM Farm.Regions
ORDER BY region;
";
			var enableRegionsCommandText =
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

			var getShowClient = @"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName     
FROM    (accessright.regionaladmins, clientsdata as cd) 
	JOIN ShowRegulation sr ON sr.ShowClientCode = cd.FirmCode
WHERE   sr.PrimaryClientCode				 = ?ClientCode
		AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName               = ?UserName 
        AND FirmType                         = if(ShowRetail+ShowVendor = 2, FirmType, if(ShowRetail = 1, 1, 0)) 
        AND if(UseRegistrant                 = 1, Registrant = ?UserName, 1 = 1)   
        AND FirmStatus    = 1 
        AND billingstatus = 1 
ORDER BY cd.shortname;
";

			var dataAdapter = new MySqlDataAdapter(pricesCommandText, _connection);
			dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
			dataAdapter.SelectCommand.Parameters.AddWithValue("?UserName", _userName);
			dataAdapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", _homeRegion);

            try
            {
                _connection.Open();

                dataAdapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                dataAdapter.Fill(Data, "Prices");

                dataAdapter.SelectCommand.CommandText = regionSettingsCommnadText;
                dataAdapter.Fill(Data, "RegionSettings");

                dataAdapter.SelectCommand.CommandText = regionsCommandText;
                dataAdapter.Fill(Data, "Regions");

                dataAdapter.SelectCommand.CommandText = enableRegionsCommandText;
                dataAdapter.Fill(Data, "EnableRegions");

            	dataAdapter.SelectCommand.CommandText = getShowClient;
            	dataAdapter.Fill(Data, "ShowClients");

                dataAdapter.SelectCommand.Transaction.Commit();
            }
            finally
            {
                _connection.Close();
            }

			HeaderLabel.Text = String.Format("Конфигурация клиента \"{0}\"", Data.Tables["Prices"].DefaultView[0]["ShortName"].ToString());
		}


		private void SetRegions()
		{
			HomeRegion.SelectedValue = _homeRegion.ToString();
			for (var i = 0; i < Data.Tables["EnableRegions"].Rows.Count; i++)
				WorkRegionList.Items[i].Selected = Convert.ToBoolean(Data.Tables["EnableRegions"].Rows[i]["Enable"]);
		}

		protected void PricesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					var row = Data.Tables["Prices"].NewRow();
					row["UpCost"] = 0;
					row["Enabled"] = false;
					row["AgencyEnabled"] = false;
					row["AlowInt"] = false;
					row["PriceType"] = 0;
					Data.Tables["Prices"].Rows.Add(row);
					((GridView)sender).DataBind();
					break;
			}

		}
		protected void PricesGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			Data.Tables["Prices"].DefaultView[e.RowIndex].Delete();
			((GridView)sender).DataBind();
		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			ShowAllRegionsCheck.Checked = false;
			UpdateHomeRegion();
			UpdateMaskRegion();
			ProcessChanges();
			var pricesDataAdapter = new MySqlDataAdapter("", _connection);
			pricesDataAdapter.DeleteCommand = new MySqlCommand(
@"
Set @InHost = ?UserHost;
Set @InUser = ?UserName;

DELETE FROM PricesData
WHERE PriceCode = ?PriceCode;

DELETE FROM Intersection
WHERE PriceCode = ?PriceCode;

DELETE FROM intersection_update_info
WHERE PriceCode = ?PriceCode;

DELETE FROM PricesRegionalData
WHERE PriceCode = ?PriceCode;
", _connection);

			pricesDataAdapter.DeleteCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.DeleteCommand.Parameters.AddWithValue("?UserName", _userName);
			pricesDataAdapter.DeleteCommand.Parameters.Add("?PriceCode", MySqlDbType.Int32, 0, "PriceCode");

			pricesDataAdapter.InsertCommand = new MySqlCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

INSERT INTO PricesData
SET UpCost = ?UpCost,
	PriceType = ?PriceType,
    CostType = ?CostType,
	Enabled = ?Enabled,
	AgencyEnabled = ?AgencyEnabled,
	AlowInt = ?AlowInt,
	FirmCode = ?ClientCode;
SET @InsertedPriceCode = Last_Insert_ID();

INSERT INTO farm.formrules() VALUES();
SET @NewFormRulesId = Last_Insert_ID();

INSERT INTO farm.sources() VALUES();	
SET @NewSourceId = Last_Insert_ID();

INSERT INTO usersettings.PriceItems(FormRuleId, SourceId) VALUES(@NewFormRulesId, @NewSourceId);
SET @NewPriceItemId = Last_Insert_ID();

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT @InsertedPriceCode, 1, @NewPriceItemId;
SET @NewPriceCostId:=Last_Insert_ID(); 

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId; 

call UpdateCostType(@InsertedPriceCode, ?CostType);

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
        AND (r.RegionCode & cd.MaskRegion > 0)  
        AND not exists
        (
			SELECT * 
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
        if(pricesdata.PriceType = 0, 0, 1) as invisibleonclient,
        a.invisibleonfirm,
        (
          SELECT costcode
          FROM    pricescosts pcc
          WHERE   basecost
                  AND pcc.PriceCode = pricesdata.PriceCode
        ) as CostCode
FROM pricesdata 
	JOIN clientsdata ON pricesdata.firmcode = clientsdata.firmcode
		JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
			JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND pricesdata.PriceCode = @InsertedPriceCode
		AND clientsdata2.firmtype = 1;

INSERT 
INTO    intersection_update_info
        (
                ClientCode, 
                regioncode, 
                pricecode
        )    
SELECT  DISTINCT clientsdata2.firmcode,
        regions.regioncode, 
        pricesdata.pricecode
FROM pricesdata 
	JOIN clientsdata ON pricesdata.firmcode = clientsdata.firmcode
		JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND pricesdata.PriceCode = @InsertedPriceCode
		AND clientsdata2.firmtype = 1;
", _connection);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserName", _userName);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
			pricesDataAdapter.InsertCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.InsertCommand.Parameters.Add("?PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.InsertCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("?AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("?AlowInt", MySqlDbType.Bit, 0, "AlowInt");
			pricesDataAdapter.InsertCommand.Parameters.Add("?PriceCode", MySqlDbType.Int32, 0, "PriceCode");
			pricesDataAdapter.InsertCommand.Parameters.Add("?CostType", MySqlDbType.Int32, 0, "CostType");

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

call UpdateCostType(?PriceCode, ?CostType);
", _connection);
			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", _userName);
			pricesDataAdapter.UpdateCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?AlowInt", MySqlDbType.Bit, 0, "AlowInt");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?PriceCode", MySqlDbType.Int32, 0, "PriceCode");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?CostType", MySqlDbType.Int32, 0, "CostType");

			var regionalSettingsDataAdapter = new MySqlDataAdapter("", _connection);
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
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?AdminMail", MySqlDbType.VarString, 0, "AdminMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?TmpMail", MySqlDbType.VarString, 0, "TmpMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?SupportPhone", MySqlDbType.VarString, 0, "SupportPhone");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Storage", MySqlDbType.Bit, 0, "Storage");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "RowID");

			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", _userName);

			var showClientsAdapter = new MySqlDataAdapter();
			showClientsAdapter.DeleteCommand = new MySqlCommand(@"
DELETE FROM showregulation 
WHERE PrimaryClientCode = ?PrimaryClientCode AND ShowClientCode = ?ShowClientCode;
", _connection);
			showClientsAdapter.DeleteCommand.Parameters.AddWithValue("?PrimaryClientCode", _clientCode);
			showClientsAdapter.DeleteCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");

			showClientsAdapter.InsertCommand = new MySqlCommand(@"
INSERT INTO showregulation 
SET PrimaryClientCode = ?PrimaryClientCode,
	ShowClientCode = ?ShowClientCode;
", _connection);
			showClientsAdapter.InsertCommand.Parameters.AddWithValue("?PrimaryClientCode", _clientCode);
			showClientsAdapter.InsertCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");

			showClientsAdapter.Update(Data, "ShowClients");

			MySqlTransaction transaction = null;
			try
			{
				_connection.Open();
                transaction = _connection.BeginTransaction(IsolationLevel.RepeatableRead);

				pricesDataAdapter.InsertCommand.Transaction = transaction;
				pricesDataAdapter.UpdateCommand.Transaction = transaction;
				pricesDataAdapter.DeleteCommand.Transaction = transaction;
				regionalSettingsDataAdapter.UpdateCommand.Transaction = transaction;
				showClientsAdapter.DeleteCommand.Transaction = transaction;
				showClientsAdapter.InsertCommand.Transaction = transaction;

				pricesDataAdapter.Update(Data.Tables["Prices"]);
				regionalSettingsDataAdapter.Update(Data.Tables["RegionSettings"]);
				showClientsAdapter.Update(Data.Tables["ShowClients"]);

				transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw new Exception("Ошибка на странице Managep.aspx", ex);
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

		private void ProcessChanges()
		{
			for (var i = 0; i < RegionalSettingsGrid.Rows.Count; i++)
			{
				if (Convert.ToBoolean(Data.Tables["RegionSettings"].Rows[i]["Enabled"]) != ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("EnabledCheck")).Checked)
					Data.Tables["RegionSettings"].Rows[i]["Enabled"] = ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("EnabledCheck")).Checked;
				if (Convert.ToBoolean(Data.Tables["RegionSettings"].Rows[i]["Storage"]) != ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("StorageCheck")).Checked)
					Data.Tables["RegionSettings"].Rows[i]["Storage"] = ((CheckBox)RegionalSettingsGrid.Rows[i].FindControl("StorageCheck")).Checked;
				if (Convert.ToString(Data.Tables["RegionSettings"].Rows[i]["AdminMail"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("AdministratorEmailText")).Text)
					Data.Tables["RegionSettings"].Rows[i]["AdminMail"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("AdministratorEmailText")).Text;
				if (Convert.ToString(Data.Tables["RegionSettings"].Rows[i]["TmpMail"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("RegionalEmailText")).Text)
					Data.Tables["RegionSettings"].Rows[i]["TmpMail"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("RegionalEmailText")).Text;
				if (Convert.ToString(Data.Tables["RegionSettings"].Rows[i]["SupportPhone"]) != ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("SupportPhoneText")).Text)
					Data.Tables["RegionSettings"].Rows[i]["SupportPhone"] = ((TextBox)RegionalSettingsGrid.Rows[i].FindControl("SupportPhoneText")).Text;
			}
			for (var i = 0; i < PricesGrid.Rows.Count; i++)
			{
				if (Convert.ToString(Data.Tables["Prices"].DefaultView[i]["UpCost"]) != ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text)
					Data.Tables["Prices"].DefaultView[i]["UpCost"] = ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text;
				if (Convert.ToString(Data.Tables["Prices"].DefaultView[i]["PriceType"]) != ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue)
					Data.Tables["Prices"].DefaultView[i]["PriceType"] = ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["AgencyEnabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked)
					Data.Tables["Prices"].DefaultView[i]["AgencyEnabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["Enabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked)
					Data.Tables["Prices"].DefaultView[i]["Enabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["AlowInt"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("IntegratedCheck")).Checked)
					Data.Tables["Prices"].DefaultView[i]["AlowInt"] = ((CheckBox)PricesGrid.Rows[i].FindControl("IntegratedCheck")).Checked;
				if (Data.Tables["Prices"].DefaultView[i]["CostType"].ToString() != ((DropDownList)PricesGrid.Rows[i].FindControl("CostType")).SelectedValue)
				{
					var value = ((DropDownList) PricesGrid.Rows[i].FindControl("CostType")).SelectedValue;
					if (value == DBNull.Value.ToString())
						Data.Tables["Prices"].DefaultView[i]["CostType"] = DBNull.Value;
					else
						Data.Tables["Prices"].DefaultView[i]["CostType"] = value;
				}
			}

			foreach (GridViewRow row in ShowClientsGrid.Rows)
			{
				if (Data.Tables["ShowClients"].DefaultView[row.RowIndex]["ShortName"].ToString() != ((DropDownList)row.FindControl("ShowClientsList")).SelectedItem.Text)
				{
					Data.Tables["ShowClients"].DefaultView[row.RowIndex]["ShortName"] = ((DropDownList)row.FindControl("ShowClientsList")).SelectedItem.Text;
					Data.Tables["ShowClients"].DefaultView[row.RowIndex]["FirmCode"] = ((DropDownList)row.FindControl("ShowClientsList")).SelectedValue;
				}
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
            try
            {
                var adapter = new MySqlDataAdapter(commandText, _connection);
                _connection.Open();
                adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);

                adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
                adapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", _homeRegion);
                adapter.SelectCommand.Parameters.AddWithValue("?UserName", _userName);
                Data.Tables["EnableRegions"].Clear();
                adapter.Fill(Data, "EnableRegions");

                adapter.SelectCommand.Transaction.Commit();
            }
            finally
            {
                _connection.Close();
            }
			WorkRegionList.DataBind();
			SetRegions();
		}
		
		private void UpdateMaskRegion()
		{
			var maskRegionCommand = new ScalarCommand(
@"
SELECT MaskRegion FROM ClientsData WHERE FirmCode = ?ClientCode;
", IsolationLevel.ReadCommitted);
			maskRegionCommand.Parameters.Add("?ClientCode", _clientCode);
			maskRegionCommand.Execute();
			var oldMaskRegion = Convert.ToUInt64(maskRegionCommand.Result);
			var newMaskRegion = oldMaskRegion;
			foreach (ListItem item in WorkRegionList.Items)
				newMaskRegion = item.Selected ? newMaskRegion | Convert.ToUInt64(item.Value) : newMaskRegion & ~Convert.ToUInt64(item.Value);

			if(oldMaskRegion != newMaskRegion)
			{
				var updateCommand = new ParametericCommand(
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
        AND (r.RegionCode & cd.MaskRegion > 0)  
        AND not exists
        (SELECT * 
        FROM    pricesregionaldata prd 
        WHERE   prd.PriceCode      = p.PriceCode 
                AND prd.RegionCode = r.RegionCode
        );

INSERT INTO regionaldata(FirmCode, RegionCode)
SELECT  cd.FirmCode, 
        r.RegionCode 
FROM    ClientsData cd, 
        Farm.Regions r 
WHERE   cd.FirmCode = ?ClientCode 
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
        if(pricesdata.PriceType = 0, 0, 1) as invisibleonclient,
        a.invisibleonfirm,
        (
          SELECT costcode
          FROM    pricescosts pcc
          WHERE   basecost
                  AND pcc.PriceCode = pricesdata.PriceCode
        ) as CostCode
FROM clientsdata
	JOIN pricesdata on pricesdata.firmcode = clientsdata.firmcode
	JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
		JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND clientsdata.firmcode = ?ClientCode
		AND clientsdata2.FirmType = 1;

INSERT 
INTO    intersection_update_info
        (
                ClientCode, 
                regioncode, 
                pricecode
        )    
SELECT  DISTINCT clientsdata2.firmcode,
        regions.regioncode, 
        pricesdata.pricecode
FROM clientsdata
	JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
		JOIN pricesdata on pricesdata.firmcode = clientsdata.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND clientsdata.firmcode = ?ClientCode
		AND clientsdata2.FirmType = 1;
", IsolationLevel.RepeatableRead);
				updateCommand.Parameters.Add("?MaskRegion", newMaskRegion);
				updateCommand.Parameters.Add("?ClientCode", _clientCode);
				updateCommand.Parameters.Add("?UserHost", HttpContext.Current.Request.UserHostAddress);
				updateCommand.Parameters.Add("?UserName", _userName);
				updateCommand.Execute();
			}
		}
		
		private void UpdateHomeRegion()
		{
			var currentHomeRegion = Convert.ToUInt64(HomeRegion.SelectedValue);
			if (_homeRegion != currentHomeRegion)
			{
				var command = new ParametericCommand(
@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

UPDATE ClientsData 
SET RegionCode = ?RegionCode
WHERE FirmCode = ?ClientCode;
", IsolationLevel.RepeatableRead);
				command.Parameters.Add("?RegionCode", currentHomeRegion);
				command.Parameters.Add("?ClientCode", _clientCode);
				command.Parameters.Add("?UserHost", HttpContext.Current.Request.UserHostAddress);
				command.Parameters.Add("?UserName", _userName);
				command.Execute();
			}
			
		}
		
		private ulong GetHomeRegion()
		{
			var homeRegionCommand = new ScalarCommand(
@"
SELECT RegionCode FROM ClientsData WHERE FirmCode = ?ClientCode;
", IsolationLevel.ReadCommitted);
			homeRegionCommand.Parameters.Add("?ClientCode", _clientCode);
			homeRegionCommand.Execute();

			return Convert.ToUInt64(homeRegionCommand.Result);
		}

		protected void RegionalSettingsGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{			
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				var rows = Data.Tables["EnableRegions"].Select(String.Format("RegionCode = {0}", Data.Tables["RegionSettings"].Rows[e.Row.DataItemIndex]["RegionCode"]));
				if (rows.Length > 0)
				{
					if (Convert.ToBoolean(rows[0]["Enable"]) == false)
						e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5"); ;
				}
				else
				{
					e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5");
				}
			}
		}

		protected void ShowClientsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				var rowView = (DataRowView)e.Row.DataItem;
				if (rowView.Row.RowState == DataRowState.Added)
				{
					((Button)e.Row.FindControl("SearchButton")).CommandArgument = e.Row.RowIndex.ToString();
					e.Row.FindControl("SearchButton").Visible = true;
					e.Row.FindControl("SearchText").Visible = true;
				}
				else
				{
					e.Row.FindControl("SearchButton").Visible = false;
					e.Row.FindControl("SearchText").Visible = false;
				}
				var list = ((DropDownList)e.Row.FindControl("ShowClientsList"));
				list.Items.Add(new ListItem(rowView["ShortName"].ToString(), rowView["FirmCode"].ToString()));
				list.Width = new Unit(90, UnitType.Percentage);
			}
		}

		protected void ShowClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					ProcessChanges();
					Data.Tables["ShowClients"].Rows.Add(Data.Tables["ShowClients"].NewRow());
					ShowClientsGrid.DataSource = Data.Tables["ShowClients"].DefaultView;
					ShowClientsGrid.DataBind();
					break;
				case "Search":
					var adapter = new MySqlDataAdapter
						(@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName
FROM    (accessright.regionaladmins, clientsdata as cd)  
LEFT JOIN showregulation sr
        ON sr.ShowClientCode	             =cd.firmcode  
WHERE   cd.regioncode & regionaladmins.regionmask > 0  
        AND regionaladmins.UserName               =?UserName  
        AND FirmType                         =if(ShowRetail+ShowVendor=2, FirmType, if(ShowRetail=1, 1, 0)) 
        AND if(UseRegistrant                 =1, Registrant=?UserName, 1=1)  
        AND cd.ShortName like ?SearchText
        AND FirmStatus   =1  
        AND billingstatus=1  
ORDER BY cd.shortname;
", Literals.GetConnectionString());
					adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
                    adapter.SelectCommand.Parameters.AddWithValue("?SearchText", string.Format("%{0}%", ((TextBox)ShowClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText")).Text));

					var data = new DataSet();
					try
					{
						_connection.Open();
						adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
						adapter.Fill(data);
						adapter.SelectCommand.Transaction.Commit();
					}
					finally
					{
						_connection.Close();
					}

					var ShowList = ((DropDownList)ShowClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ShowClientsList"));
					ShowList.DataSource = data;
					ShowList.DataBind();
					ShowList.Visible = data.Tables[0].Rows.Count > 0;
					break;
			}
		}

		protected void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ProcessChanges();
			Data.Tables["ShowClients"].DefaultView[e.RowIndex].Delete();
			ShowClientsGrid.DataSource = Data.Tables["ShowClients"].DefaultView;
			ShowClientsGrid.DataBind();
		}

		protected void ParentValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = !String.IsNullOrEmpty(args.Value);
		}

		public Dictionary<object, string> GetCostTypeSource(object costType)
		{
			if (costType.Equals(DBNull.Value))
				return _unconfiguratedCostTypes;
			return _configuratedCostTypes;				
		}
	}
}
