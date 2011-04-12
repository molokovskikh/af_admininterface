using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Common.MySql;
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
			SecurityContext.Administrator.CheckPermisions(PermissionType.ViewSuppliers, PermissionType.ManageSuppliers);
		
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
			OrderSendRules.DataSource = Data;
		}

		private void GetData()
		{
			_homeRegion = GetHomeRegion();
			SecurityContext.Administrator.CheckClientHomeRegion(_homeRegion);
			if (Data == null)
				Data = new DataSet();
			else 
				Data.Clear();

			var supplier = Supplier.Find(Convert.ToUInt32(_clientCode));

			var pricesCommandText =
@"
SELECT  pd.PriceCode,
		pd.PriceName,
		pd.AgencyEnabled,
		pd.Enabled,
		pd.AlowInt,
		pi.PriceDate,
		pd.UpCost,
		pd.PriceType,
		pd.CostType
FROM pricesdata pd
	JOIN usersettings.pricescosts pc pd.PriceCode = pc.PriceCode and pc.BaseCost = 1
		JOIN usersettings.PriceItems pi on pi.Id = pc.PriceItemId
WHERE pd.firmcode = ?supplierId
GROUP BY pd.PriceCode;
";
			var regionSettingsCommnadText = @"
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
	JOIN farm.regions r ON rd.regioncode = r.regioncode  
WHERE rd.FirmCode      = ?ClientCode
	  and r.regionCode & ?AdminRegionMask;";

			var regionsCommandText = @"
SELECT  RegionCode,   
		Region
FROM Farm.Regions
WHERE regionCode & ?AdminRegionMask > 0
ORDER BY region;";

			var enableRegionsCommandText = @"
SELECT  a.RegionCode,
		a.Region,
		cd.MaskRegion & a.regioncode > 0 as Enable
FROM    farm.regions as a, 
		farm.regions as b, 
		clientsdata as cd
WHERE   b.regioncode = ?HomeRegion
		AND cd.firmcode = ?ClientCode
		AND a.regioncode & ?AdminRegionMask & (b.defaultshowregionmask | cd.MaskRegion) > 0
GROUP BY regioncode
ORDER BY region;";

			var orderSendConfig = @"
SELECT s.Id, s.SenderId, s.FormaterId, s.SendDebugMessage, s.ErrorNotificationDelay, r.RegionCode
FROM ordersendrules.order_send_rules s
	left join farm.regions r on s.regioncode = r.regioncode
	join usersettings.clientsdata cd on cd.firmcode = s.firmcode
where s.firmcode = ?ClientCode
	  and (cd.MaskRegion & ?AdminRegionMask & r.regioncode > 0 or s.regionCode is null)
order by s.RegionCode;";

			var senders = @"
SELECT id, classname FROM ordersendrules.order_handlers o
where o.type = 2
order by classname;";

			var formaters = @"
SELECT id, classname FROM ordersendrules.order_handlers o
where o.type = 1
order by className;";

			var sendRuleRegions = @"
SELECT r.regioncode, r.region
FROM (farm.regions r, usersettings.clientsdata cd)
where cd.firmcode = ?ClientCode
	  and cd.MaskRegion & ?AdminRegionMask & r.regioncode > 0
order by r.Region;";


			With.Connection(
				c => {
					var dataAdapter = new MySqlDataAdapter(pricesCommandText, c);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", _homeRegion);

					dataAdapter.Fill(Data, "Prices");

					dataAdapter.SelectCommand.CommandText = regionSettingsCommnadText;
					dataAdapter.Fill(Data, "RegionSettings");

					dataAdapter.SelectCommand.CommandText = regionsCommandText;
					dataAdapter.Fill(Data, "Regions");

					dataAdapter.SelectCommand.CommandText = enableRegionsCommandText;
					dataAdapter.Fill(Data, "EnableRegions");

					dataAdapter.SelectCommand.CommandText = orderSendConfig;
					dataAdapter.Fill(Data, "OrderSendConfig");

					dataAdapter.SelectCommand.CommandText = senders;
					dataAdapter.Fill(Data, "Senders");

					dataAdapter.SelectCommand.CommandText = formaters;
					dataAdapter.Fill(Data, "Formaters");

					dataAdapter.SelectCommand.CommandText = sendRuleRegions;
					dataAdapter.Fill(Data, "SenRuleRegions");
					var row = Data.Tables["SenRuleRegions"].NewRow();
					row["RegionCode"] = DBNull.Value;
					row["Region"] = "Любой регион";
					Data.Tables["SenRuleRegions"].Rows.InsertAt(row, 0);

					ShowRegulationHelper.Load(dataAdapter, Data);
				});

			HeaderLabel.Text = String.Format("Конфигурация клиента \"{0}\"", Data.Tables["Prices"].DefaultView[0]["ShortName"]);
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
			var pricesDataAdapter = new MySqlDataAdapter();
			pricesDataAdapter.DeleteCommand = new MySqlCommand(
@"
Set @InHost = ?UserHost;
Set @InUser = ?UserName;

DELETE FROM PricesData
WHERE PriceCode = ?PriceCode;

DELETE FROM Intersection
WHERE PriceCode = ?PriceCode;

DELETE FROM PricesRegionalData
WHERE PriceCode = ?PriceCode;
");

			pricesDataAdapter.DeleteCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.DeleteCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
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
				costcode,
				FirmClientCode,
				FirmClientCode2,
				FirmClientCode3
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
		) as CostCode,
		rootIntersection.FirmClientCode,
		rootIntersection.FirmClientCode2,
		rootIntersection.FirmClientCode3
FROM pricesdata 
	JOIN clientsdata ON pricesdata.firmcode = clientsdata.firmcode
		JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
			JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
	LEFT JOIN pricesdata as rootPrice on rootPrice.PriceCode = (select min(pricecode) from pricesdata as p where p.firmcode = clientsdata.FirmCode)
		LEFT JOIN intersection as rootIntersection on rootIntersection.PriceCode = rootPrice.PriceCode and rootIntersection.RegionCode = Regions.RegionCode and rootIntersection.ClientCode = clientsdata2.FirmCode
WHERE   intersection.pricecode IS NULL
		AND clientsdata.firmtype = 0
		AND pricesdata.PriceCode = @InsertedPriceCode
		AND clientsdata2.firmtype = 1;
");
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
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
");
			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			pricesDataAdapter.UpdateCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?AlowInt", MySqlDbType.Bit, 0, "AlowInt");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?PriceCode", MySqlDbType.Int32, 0, "PriceCode");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?CostType", MySqlDbType.Int32, 0, "CostType");

			var regionalSettingsDataAdapter = new MySqlDataAdapter();
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
");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?AdminMail", MySqlDbType.VarString, 0, "AdminMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?TmpMail", MySqlDbType.VarString, 0, "TmpMail");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?SupportPhone", MySqlDbType.VarString, 0, "SupportPhone");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Storage", MySqlDbType.Bit, 0, "Storage");
			regionalSettingsDataAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "RowID");

			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

			var orderSendRulesDataAdapter = new MySqlDataAdapter();
			orderSendRulesDataAdapter.UpdateCommand = new MySqlCommand(@"
update ordersendrules.order_send_rules
set SenderId = ?senderId, 
	FormaterId = ?formaterId, 
	RegionCode = ?RegionCode, 
	SendDebugMessage = ?SendDebugMessage, 
	ErrorNotificationDelay = ?ErrorNotificationDelay
where id = ?Id;");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?SenderId", MySqlDbType.Int32, 0, "SenderId");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?FormaterId", MySqlDbType.Int32, 0, "FormaterId");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?RegionCode", MySqlDbType.Int64, 0, "RegionCode");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?SendDebugMessage", MySqlDbType.Bit, 0, "SendDebugMessage");
			orderSendRulesDataAdapter.UpdateCommand.Parameters.Add("?ErrorNotificationDelay", MySqlDbType.Bit, 0, "ErrorNotificationDelay");

			orderSendRulesDataAdapter.InsertCommand = new MySqlCommand(@"
insert into ordersendrules.order_send_rules(FirmCode, SenderId, FormaterId, RegionCode, SendDebugMessage, ErrorNotificationDelay)
values(?FirmCode, ?senderId, ?formaterId, ?regionCode, ?SendDebugMessage, ?ErrorNotificationDelay);");
			orderSendRulesDataAdapter.InsertCommand.Parameters.Add("?SenderId", MySqlDbType.Int32, 0, "SenderId");
			orderSendRulesDataAdapter.InsertCommand.Parameters.Add("?FormaterId", MySqlDbType.Int32, 0, "FormaterId");
			orderSendRulesDataAdapter.InsertCommand.Parameters.Add("?RegionCode", MySqlDbType.Int64, 0, "RegionCode");
			orderSendRulesDataAdapter.InsertCommand.Parameters.Add("?SendDebugMessage", MySqlDbType.Bit, 0, "SendDebugMessage");
			orderSendRulesDataAdapter.InsertCommand.Parameters.Add("?ErrorNotificationDelay", MySqlDbType.Bit, 0, "ErrorNotificationDelay");
			orderSendRulesDataAdapter.InsertCommand.Parameters.AddWithValue("?FirmCode", _clientCode);

			orderSendRulesDataAdapter.DeleteCommand = new MySqlCommand(@"
delete FROM ordersendrules.order_send_rules
where id = ?Id;");
			orderSendRulesDataAdapter.DeleteCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");

			With.Transaction(
				(connection, transaction) => {
					pricesDataAdapter.InsertCommand.Connection = connection;
					pricesDataAdapter.InsertCommand.Transaction = transaction;
					pricesDataAdapter.UpdateCommand.Connection = connection;
					pricesDataAdapter.UpdateCommand.Transaction = transaction;
					pricesDataAdapter.DeleteCommand.Connection = connection;
					pricesDataAdapter.DeleteCommand.Transaction = transaction;
					regionalSettingsDataAdapter.UpdateCommand.Transaction = transaction;
					regionalSettingsDataAdapter.UpdateCommand.Connection = connection;

					orderSendRulesDataAdapter.InsertCommand.Transaction = transaction;
					orderSendRulesDataAdapter.InsertCommand.Connection = connection;
					orderSendRulesDataAdapter.UpdateCommand.Transaction = transaction;
					orderSendRulesDataAdapter.UpdateCommand.Connection = connection;
					orderSendRulesDataAdapter.DeleteCommand.Transaction = transaction;
					orderSendRulesDataAdapter.DeleteCommand.Connection = connection;

					pricesDataAdapter.Update(Data.Tables["Prices"]);
					regionalSettingsDataAdapter.Update(Data.Tables["RegionSettings"]);
					orderSendRulesDataAdapter.Update(Data.Tables["OrderSendConfig"]);
					ShowRegulationHelper.Update(connection, transaction, Data, _clientCode);
				});
			
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

			for (var i = 0; i < OrderSendRules.Rows.Count; i++)
			{
				var row = OrderSendRules.Rows[i];
				var dataRow = Data.Tables["OrderSendConfig"].DefaultView[i];

				if (Convert.ToUInt32(((DropDownList)row.FindControl("Sender")).SelectedValue) != Convert.ToUInt32(dataRow["SenderId"]))
					dataRow["Senderid"] = Convert.ToUInt32(((DropDownList) row.FindControl("Sender")).SelectedValue);

				if (Convert.ToUInt32(((DropDownList)row.FindControl("Formater")).SelectedValue) != Convert.ToUInt32(dataRow["FormaterId"]))
					dataRow["FormaterId"] = Convert.ToUInt32(((DropDownList)row.FindControl("Formater")).SelectedValue);

				if (((DropDownList)row.FindControl("Region")).SelectedValue != dataRow["RegionCode"].ToString())
				{
					var value = ((DropDownList) row.FindControl("Region")).SelectedValue;
					if (value == DBNull.Value.ToString())
						dataRow["RegionCode"] = DBNull.Value;
					else
						dataRow["RegionCode"] = Convert.ToUInt64(value);
				}

				if (((CheckBox)row.FindControl("SendDebugMessage")).Checked != Convert.ToBoolean(dataRow["SendDebugMessage"]))
					dataRow["SendDebugMessage"] = Convert.ToBoolean(((CheckBox)row.FindControl("SendDebugMessage")).Checked);

				if (Convert.ToUInt32(((TextBox)row.FindControl("SmsSendDelay")).Text) != Convert.ToUInt32(dataRow["ErrorNotificationDelay"]))
					dataRow["ErrorNotificationDelay"] = Convert.ToUInt32(((TextBox)row.FindControl("SmsSendDelay")).Text);
			}

			ShowRegulationHelper.ProcessChanges(ShowClientsGrid, Data);
		}

		protected void ShowAllRegionsCheck_CheckedChanged(object sender, EventArgs e)
		{
			string commandText;
			if (((CheckBox)sender).Checked)
			{
				commandText = @"
SELECT  a.RegionCode,   
		a.Region,
		cd.MaskRegion & a.regioncode > 0 as Enable
FROM farm.regions as a, clientsdata as cd
WHERE a.regionCode & ?AdminRegionMask > 0 and cd.firmcode = ?ClientCode
ORDER BY region;";
			}
			else
			{
				commandText = @"
SELECT  a.RegionCode,
		a.Region,
		cd.MaskRegion & a.regioncode > 0 as Enable
FROM    farm.regions as a, 
		farm.regions as b, 
		clientsdata as cd
WHERE   b.regioncode = ?HomeRegion
		and cd.firmcode = ?ClientCode
		and a.regioncode & ?AdminRegionMask & (b.defaultshowregionmask | cd.MaskRegion) > 0
GROUP BY regioncode
ORDER BY region;";
			}
			Data.Tables["EnableRegions"].Clear();
			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(commandText, c);
					adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", _homeRegion);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
					adapter.Fill(Data, "EnableRegions");
				});
			WorkRegionList.DataBind();
			SetRegions();
		}
		
		private void UpdateMaskRegion()
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var maskRegionCommand = new MySqlCommand(@"SELECT MaskRegion FROM ClientsData WHERE FirmCode = ?ClientCode;", connection);
				maskRegionCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
				var oldMaskRegion = Convert.ToUInt64(maskRegionCommand.ExecuteScalar());
				var newMaskRegion = oldMaskRegion;
				foreach (ListItem item in WorkRegionList.Items)
				{
					if (item.Selected)
						newMaskRegion |= Convert.ToUInt64(item.Value);
					else
						newMaskRegion &= ~Convert.ToUInt64(item.Value);
				}

				if (oldMaskRegion == newMaskRegion)
					return;

				var updateCommand = new MySqlCommand(
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
		AND clientsdata2.FirmType = 1;", connection);
				updateCommand.Parameters.AddWithValue("?MaskRegion", newMaskRegion);
				updateCommand.Parameters.AddWithValue("?ClientCode", _clientCode);
				updateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
				updateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				updateCommand.ExecuteNonQuery();
			}
		}
		
		private void UpdateHomeRegion()
		{
			var currentHomeRegion = Convert.ToUInt64(HomeRegion.SelectedValue);
			if (_homeRegion == currentHomeRegion) 
				return;

			With.Transaction(
				(connection, transaction) =>
					{
						var command = new MySqlCommand(@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

UPDATE ClientsData 
SET RegionCode = ?RegionCode
WHERE FirmCode = ?ClientCode;", connection, transaction);
						command.Parameters.AddWithValue("?RegionCode", currentHomeRegion);
						command.Parameters.AddWithValue("?ClientCode", _clientCode);
						command.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
						command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
						command.ExecuteNonQuery();
					});
		}
		
		private ulong GetHomeRegion()
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var homeRegionCommand = new MySqlCommand("SELECT RegionCode FROM ClientsData WHERE FirmCode = ?ClientCode;",
														 connection);
				homeRegionCommand.Parameters.AddWithValue("?ClientCode", _clientCode);

				return Convert.ToUInt64(homeRegionCommand.ExecuteScalar());
			}
		}

		protected void RegionalSettingsGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{			
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				var rows = Data.Tables["EnableRegions"].Select(String.Format("RegionCode = {0}", Data.Tables["RegionSettings"].Rows[e.Row.DataItemIndex]["RegionCode"]));
				if (rows.Length > 0)
				{
					if (Convert.ToBoolean(rows[0]["Enable"]) == false)
						e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5"); 
				}
				else
				{
					e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5");
				}
			}
		}

		protected void ShowClientsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowDataBound(sender, e);
		}

		protected void ShowClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowCommand(sender, e, Data);
		}

		protected void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowDeleting(sender, e, Data);
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

		protected void OrderSettings_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					var row = Data.Tables["OrderSendConfig"].NewRow();
					row["SenderId"] = 1;
					row["FormaterId"] = 12;
					row["RegionCode"] = DBNull.Value;
					row["SendDebugMessage"] = false;
					row["ErrorNotificationDelay"] = false;
					Data.Tables["OrderSendConfig"].Rows.Add(row);
					((GridView)sender).DataBind();
					break;
			}
		}

		protected void OrderSettings_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			Data.Tables["OrderSendConfig"].DefaultView[e.RowIndex].Delete();
			((GridView)sender).DataBind();			
		}

		protected void OrderSettings_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var orderSender = (DropDownList)e.Row.FindControl("Sender");
			var orderFormater = (DropDownList)e.Row.FindControl("Formater");
			var region = (DropDownList)e.Row.FindControl("Region");

			orderSender.DataSource = Data.Tables["Senders"];
			orderSender.DataBind();
			orderSender.SelectedValue = ((DataRowView) e.Row.DataItem)["SenderId"].ToString();

			orderFormater.DataSource = Data.Tables["Formaters"];
			orderFormater.DataBind();
			orderFormater.SelectedValue = ((DataRowView)e.Row.DataItem)["FormaterId"].ToString();

			region.DataSource = Data.Tables["SenRuleRegions"];
			region.DataBind();
			region.SelectedValue = ((DataRowView)e.Row.DataItem)["RegionCode"].ToString();
		}
	}
}
