using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class manageret : Page
	{
		protected DataSet DS1;
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataTable WorkReg;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn7;
		protected DataColumn DataColumn8;
		protected DataTable Clientsdata;
		protected DataColumn DataColumn9;
		protected DataColumn DataColumn10;
		protected DataColumn DataColumn11;
		protected DataColumn DataColumn12;
		protected DataColumn DataColumn13;
		protected DataColumn DataColumn14;
		protected DataColumn DataColumn15;
		protected DataColumn DataColumn16;
		protected DataColumn DataColumn17;
		protected DataColumn DataColumn18;
		protected DataColumn DataColumn19;
		protected DataColumn DataColumn20;
		protected DataColumn DataColumn21;
		protected DataTable admin;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataTable RetClientsSet;
		MySqlConnection _connection = new MySqlConnection();
		MySqlCommand myMySqlCommand = new MySqlCommand();
		MySqlDataReader myMySqlDataReader;
		MySqlTransaction myTrans;
		int ClientCode;
		ulong HomeRegionCode;
		string InsertCommand;


		[DebuggerStepThrough]
		private void InitializeComponent()
		{
			DS1 = new DataSet();
			Regions = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			WorkReg = new DataTable();
			DataColumn5 = new DataColumn();
			DataColumn7 = new DataColumn();
			DataColumn8 = new DataColumn();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			Clientsdata = new DataTable();
			DataColumn9 = new DataColumn();
			DataColumn10 = new DataColumn();
			DataColumn11 = new DataColumn();
			DataColumn12 = new DataColumn();
			DataColumn13 = new DataColumn();
			DataColumn14 = new DataColumn();
			DataColumn15 = new DataColumn();
			DataColumn16 = new DataColumn();
			DataColumn17 = new DataColumn();
			DataColumn18 = new DataColumn();
			DataColumn19 = new DataColumn();
			DataColumn20 = new DataColumn();
			DataColumn21 = new DataColumn();
			admin = new DataTable();
			RetClientsSet = new DataTable();
			DS1.BeginInit();
			Regions.BeginInit();
			WorkReg.BeginInit();
			Clientsdata.BeginInit();
			admin.BeginInit();
			RetClientsSet.BeginInit();
			DS1.DataSetName = "NewDataSet";
			DS1.Locale = new CultureInfo("ru-RU");
			DS1.Tables.AddRange(new DataTable[] { Regions, WorkReg, Clientsdata, admin, RetClientsSet });
			Regions.Columns.AddRange(new DataColumn[] { DataColumn1, DataColumn2 });
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof(Int64);
			WorkReg.Columns.AddRange(new DataColumn[] { DataColumn5, DataColumn7, DataColumn8, DataColumn3, DataColumn4 });
			WorkReg.TableName = "WorkReg";
			DataColumn5.ColumnName = "RegionCode";
			DataColumn5.DataType = typeof(Int32);
			DataColumn7.ColumnName = "ShowMask";
			DataColumn7.DataType = typeof(Boolean);
			DataColumn8.ColumnName = "RegMask";
			DataColumn8.DataType = typeof(Boolean);
			DataColumn3.ColumnName = "Region";
			DataColumn4.ColumnName = "OrderMask";
			DataColumn4.DataType = typeof(Boolean);
			Clientsdata.Columns.AddRange(
				new DataColumn[]
					{
						DataColumn9, DataColumn10, DataColumn11, DataColumn12, DataColumn13, DataColumn14, DataColumn15, DataColumn16,
						DataColumn17, DataColumn18, DataColumn19, DataColumn20, DataColumn21
					});
			Clientsdata.TableName = "Clientsdata";
			DataColumn9.ColumnName = "adress";
			DataColumn10.ColumnName = "bussinfo";
			DataColumn11.ColumnName = "bussstop";
			DataColumn12.ColumnName = "fax";
			DataColumn13.ColumnName = "firmsegment";
			DataColumn13.DataType = typeof(Int16);
			DataColumn14.ColumnName = "firmtype";
			DataColumn14.DataType = typeof(Int16);
			DataColumn15.ColumnName = "oldcode";
			DataColumn15.DataType = typeof(Int16);
			DataColumn16.ColumnName = "phone";
			DataColumn17.ColumnName = "regioncode";
			DataColumn17.DataType = typeof(Int64);
			DataColumn18.ColumnName = "shortname";
			DataColumn19.ColumnName = "url";
			DataColumn20.ColumnName = "fullname";
			DataColumn21.ColumnName = "mail";
			admin.TableName = "admin";
			RetClientsSet.TableName = "RetClientsSet";
			DS1.EndInit();
			Regions.EndInit();
			WorkReg.EndInit();
			Clientsdata.EndInit();
			admin.EndInit();
			RetClientsSet.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		private DataSet Data
		{
			get { return (DataSet)Session["IncludeData"]; }
			set { Session["IncludeData"] = value; }
		}

		protected void ParametersSave_Click(object sender, EventArgs e)
		{
			if (!IsValid)
				return;

			ProcessChanges();
			myMySqlCommand.Parameters.AddWithValue("?InvisibleOnFirm", VisileStateList.SelectedItem.Value);
			myMySqlCommand.Parameters.AddWithValue("?AlowRegister", RegisterCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?AlowRejection", RejectsCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?MultiUserLevel", MultiUserLevelTB.Text);
			myMySqlCommand.Parameters.AddWithValue("?AdvertisingLevel", AdvertisingLevelCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?AlowWayBill", WayBillCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?EnableUpdate", EnableUpdateCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?CalculateLeader", CalculateLeaderCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?AllowSubmitOrders", AllowSubmitOrdersCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?SubmitOrders", SubmitOrdersCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?ServiceClient", ServiceClientCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?OrdersVisualizationMode", OrdersVisualizationModeCB.Checked);
			myMySqlCommand.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
			myMySqlCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			myMySqlCommand.Parameters.AddWithValue("?HomeRegionCode", RegionDD.SelectedItem.Value);
			myMySqlCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

			if (NoisedCosts.Visible && NoisedCosts.Checked)
				myMySqlCommand.Parameters.AddWithValue("?PriceCodeOnly", NotNoisedPrice.SelectedValue);
			else
				myMySqlCommand.Parameters.AddWithValue("?PriceCodeOnly", null);

			try
			{
				_connection.Open();
				myTrans = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				myMySqlCommand.Transaction = myTrans;
				myMySqlCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;
";
				myMySqlCommand.ExecuteNonQuery();

				myMySqlCommand.CommandText = @"
select MaskRegion, OrderRegionMask
from clientsdata cd
	join retclientsset rcs on cd.FirmCode = rcs.ClientCode
where firmcode=?clientCode";

				ulong workRegionMask;
				ulong orderRegionMask;
				using (var reader = myMySqlCommand.ExecuteReader())
				{
					reader.Read();
					workRegionMask = reader.GetUInt64("MaskRegion");
					orderRegionMask = reader.GetUInt64("OrderRegionMask");
				}

				var currentWorkRegionMask = workRegionMask;
				var currentOrderRegionMask = orderRegionMask;

				foreach (ListItem item in WRList.Items)
				{
					if (item.Selected)
						currentWorkRegionMask |= Convert.ToUInt64(item.Value);
					else
						currentWorkRegionMask &= ~Convert.ToUInt64(item.Value);
				}

				foreach (ListItem item in OrderList.Items)
				{
					if (item.Selected)
						currentOrderRegionMask |= Convert.ToUInt64(item.Value);
					else
						currentOrderRegionMask &= ~Convert.ToUInt64(item.Value);
				}

				myMySqlCommand.Parameters.AddWithValue("?WorkMask", currentWorkRegionMask);
				myMySqlCommand.Parameters.AddWithValue("?OrderMask", currentOrderRegionMask);

				if (currentWorkRegionMask != workRegionMask)
				{
					InsertCommand =
@"
UPDATE clientsdata SET MaskRegion = ?workMask WHERE FirmCode = ?ClientCode;

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
FROM clientsdata as clientsdata2
	JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN clientsdata ON clientsdata.firmsegment = clientsdata2.firmsegment
		JOIN pricesdata ON pricesdata.firmcode = clientsdata.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND clientsdata2.FirmCode = ?clientCode
		AND clientsdata2.firmtype = 1;
";

				}
				if (VisileStateList.Enabled)
				{
					myMySqlCommand.CommandText = "select InvisibleOnFirm=?InvisibleOnFirm from retclientsset where clientcode=?clientCode";
					if (Convert.ToInt32(myMySqlCommand.ExecuteScalar()) == 0)
					{
						InsertCommand += " update retclientsset, intersection, pricesdata set retclientsset.invisibleonfirm=?InvisibleOnFirm, intersection.invisibleonfirm=?InvisibleOnFirm";
						if (Convert.ToInt32(VisileStateList.SelectedValue) != 0)
							InsertCommand += ", DisabledByFirm=if(PriceType=2, 1, 0), InvisibleOnClient=if(PriceType=2, 1, 0)";
						InsertCommand += " where intersection.clientcode=retclientsset.clientcode and intersection.pricecode=pricesdata.pricecode and intersection.clientcode=?clientCode; ";
					}
				}
				InsertCommand +=
@"
UPDATE UserSettings.retclientsset, 
        UserSettings.clientsdata 
SET OrderRegionMask     = ?orderMask, 
    RegionCode              =?homeRegionCode , 
    WorkRegionMask          =if(WorkRegionMask & ?workMask > 0, WorkRegionMask, ?homeRegionCode), 
    AlowRegister            =?AlowRegister, 
    AlowRejection           =?AlowRejection, 
    MultiUserLevel          =?MultiUserLevel, 
    AdvertisingLevel        =?AdvertisingLevel, 
    AlowWayBill             =?AlowWayBill, 
    EnableUpdate            =?EnableUpdate, 
    CalculateLeader         = ?CalculateLeader, 
    AllowSubmitOrders       = ?AllowSubmitOrders, 
    SubmitOrders            = ?SubmitOrders, 
    ServiceClient           = ?ServiceClient, 
    OrdersVisualizationMode = ?OrdersVisualizationMode,
	PriceCodeOnly			= ?PriceCodeOnly
";

				InsertCommand += " where clientcode=firmcode and firmcode=?clientCode";

				var adapter = new MySqlDataAdapter();
				adapter.InsertCommand = new MySqlCommand(
@"INSERT 
INTO    UserSettings.IncludeRegulation  
        SET IncludeClientCode = ?ClientCode,
        PrimaryClientCode     = ?PrimaryClientCode,
        IncludeType           = ?IncludeType;

CALL UpdateInclude(?PrimaryClientCode, ?ClientCode, ?IncludeType);", _connection);
				adapter.InsertCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				adapter.InsertCommand.Parameters.Add("?IncludeType", MySqlDbType.UInt32, 0, "IncludeType");
				adapter.InsertCommand.Parameters.Add("?PrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");

				adapter.UpdateCommand = new MySqlCommand(
@"
DELETE FROM UserSettings.IncludeRegulation 
WHERE	IncludeClientCode = ?ClientCode 
		AND PrimaryClientCode = ?OldPrimaryClientCode;

INSERT 
INTO    UserSettings.IncludeRegulation  
        SET IncludeClientCode = ?ClientCode,
        PrimaryClientCode     = ?PrimaryClientCode,
        IncludeType           = ?IncludeType;

CALL UpdateInclude(?PrimaryClientCode, ?ClientCode, ?IncludeType);
", _connection);
				adapter.UpdateCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				adapter.UpdateCommand.Parameters.Add("?IncludeType", MySqlDbType.UInt32, 0, "IncludeType");
				adapter.UpdateCommand.Parameters.Add("?PrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");
				adapter.UpdateCommand.Parameters.Add("?OldPrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");
				adapter.UpdateCommand.Parameters["?OldPrimaryClientCode"].SourceVersion = DataRowVersion.Original;

				adapter.DeleteCommand = new MySqlCommand(
@"
DELETE FROM UserSettings.IncludeRegulation 
WHERE	id = ?id;
", _connection);
				adapter.DeleteCommand.Parameters.Add("?id", MySqlDbType.UInt32, 0, "id");

				adapter.DeleteCommand.Transaction = myTrans;
				adapter.UpdateCommand.Transaction = myTrans;
				adapter.InsertCommand.Transaction = myTrans;

				adapter.Update(Data.Tables["Include"]);

				var exportRulesAdapter = new MySqlDataAdapter();
				exportRulesAdapter.UpdateCommand = new MySqlCommand(@"
UPDATE Usersettings.Ret_Save_Grids
SET Enabled = ?Enabled
WHERE Id = ?Id;", _connection, myTrans);
				exportRulesAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.UInt16, 0, "Enabled");
				exportRulesAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.UInt32, 0, "Id");

				exportRulesAdapter.Update(Data.Tables["ExportRules"]);

				myMySqlCommand.CommandText = InsertCommand;
				myMySqlCommand.ExecuteNonQuery();


				var permissions = new DataSet();
				var permissionAdapter = new MySqlDataAdapter(@"
SELECT up.id, o.RowId, up.name, if(ap.UserId is null, 0, 1) as Enabled
FROM (UserSettings.OsUserAccessRight O, UserSettings.UserPermissions up)
  left join UserSettings.AssignedPermissions ap on o.rowid = ap.userid and up.id = ap.PermissionId
where o.clientcode = ?ClientCode
order by up.name;", Literals.GetConnectionString());
				permissionAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				permissionAdapter.Fill(permissions, "Permissions");

				for(var i = 0; i < Permissions.Items.Count; i++)
				{
					if (Convert.ToBoolean(permissions.Tables["Permissions"].DefaultView[i]["Enabled"]) != Permissions.Items[i].Selected)
					{
						if (Permissions.Items[i].Selected)
							myMySqlCommand.CommandText = @"
insert into UserSettings.AssignedPermissions(UserId, PermissionId)
values(?UserId, ?PermissionId)";
						else
							myMySqlCommand.CommandText = @"
delete from UserSettings.AssignedPermissions 
where UserId = ?UserId and PermissionId = ?PermissionId";
						myMySqlCommand.Parameters.Clear();
						myMySqlCommand.Parameters.AddWithValue("?UserId", permissions.Tables["Permissions"].DefaultView[i]["RowId"]);
						myMySqlCommand.Parameters.AddWithValue("?PermissionId", permissions.Tables["Permissions"].DefaultView[i]["Id"]);
						myMySqlCommand.ExecuteNonQuery();
					}
				}

				ShowRegulationHelper.Update(_connection, myTrans, Data, ClientCode);

				myTrans.Commit();
			}
			catch (Exception)
			{
				myTrans.Rollback();
				throw;
			}
			finally
			{
				_connection.Close();
			}
			ResultL.ForeColor = Color.Green;
			ResultL.Text = "Сохранено.";
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			var OldRegion = false;
			if (Convert.ToUInt64(RegionDD.SelectedItem.Value) == HomeRegionCode)
				OldRegion = true;

			SetWorkRegions(Convert.ToUInt64(RegionDD.SelectedItem.Value), OldRegion, false);
		}

		private void SetWorkRegions(ulong RegCode, bool OldRegion, bool AllRegions)
		{

			string sqlCommand =
@"
SELECT  a.RegionCode, 
        a.Region, 
        ShowRegionMask & a.regioncode >0 as ShowMask,  
        MaskRegion & a.regioncode     >0 as RegMask, 
        OrderRegionMask & a.regioncode>0 as OrderMask  
FROM    farm.regions                     as a, 
        farm.regions                     as b, 
        clientsdata, 
        retclientsset, 
        accessright.regionaladmins  
WHERE 
";
			if (!AllRegions)
				sqlCommand += " b.regioncode=?RegCode and";
			sqlCommand += " clientsdata.firmcode=?ClientCode and a.regioncode & (b.defaultshowregionmask | MaskRegion)>0 "
					  + " and clientcode=firmcode" + " and regionaladmins.username=?UserName"
					  + " and a.regioncode & regionaladmins.RegionMask > 0"
					  + " group by regioncode" + " order by region";
			var adapter = new MySqlDataAdapter(sqlCommand, _connection);
			adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
			adapter.SelectCommand.Parameters.AddWithValue("?RegCode", RegCode);
			adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			try
			{
				_connection.Open();
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.Fill(DS1, "WorkReg");
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			WRList.DataBind();
			OrderList.DataBind();
			for (var i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (OldRegion)
				{
					WRList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["RegMask"]);
					OrderList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["OrderMask"]);
				}
				else
				{
					if (WRList.Items[i].Value == RegCode.ToString())
						WRList.Items[i].Selected = true;
					OrderList.Items[i].Selected = WRList.Items[i].Selected;
				}
			}
		}

		protected void AllRegCB_CheckedChanged(object sender, EventArgs e)
		{
			if (AllRegCB.Checked)
				SetWorkRegions(Convert.ToUInt64(RegionDD.SelectedItem.Value), true, true);
			else
				SetWorkRegions(Convert.ToUInt64(RegionDD.SelectedItem.Value), true, false);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckPermisions(PermissionType.ViewDrugstore, PermissionType.ManageDrugstore);

			_connection.ConnectionString = Literals.GetConnectionString();

			ClientCode = Convert.ToInt32(Request["cc"]);

			myMySqlCommand.Connection = _connection;
			if (IsPostBack) 
				return;

			try
			{
				_connection.Open();
				var transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);

				myMySqlCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				myMySqlCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

				myMySqlCommand.CommandText = @"
SELECT  RegionCode
FROM clientsdata as cd
WHERE cd.firmcode = ?ClientCode 
";
				myMySqlCommand.Transaction = transaction;
				HomeRegionCode = Convert.ToUInt64(myMySqlCommand.ExecuteScalar());
				SecurityContext.Administrator.CheckClientHomeRegion(HomeRegionCode);


				var regionAdapter = new MySqlDataAdapter(@"
SELECT  r.regioncode, 
        r.region 
FROM farm.regions r
WHERE r.RegionCode & ?AdminMaskRegion > 0
ORDER BY r.region;
", _connection);
				regionAdapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
				regionAdapter.SelectCommand.Transaction = transaction;
				regionAdapter.Fill(DS1, "admin");
				RegionDD.DataBind();
				for (var i = 0; i <= RegionDD.Items.Count - 1; i++)
				{
					if (Convert.ToUInt64(RegionDD.Items[i].Value) == HomeRegionCode)
					{
						RegionDD.SelectedIndex = i;
						break;
					}
				}
				myMySqlCommand.CommandText = @"
SELECT  InvisibleOnFirm, 
        AlowRegister, 
        AlowRejection, 
        MultiUserLevel, 
        AdvertisingLevel, 
        AlowWayBill, 
        EnableUpdate, 
        length(rui.UniqueCopyID) = 0 as Length, 
        CalculateLeader, 
        AllowSubmitOrders, 
        SubmitOrders, 
        ServiceClient, 
        OrdersVisualizationMode, 
        ShowMessageCount,
		pd.PriceCode as NotNoisedPriceCode,
		concat(pd.PriceName, ' ', cd.ShortName) as NotNoisedPriceName
FROM retclientsset rcs
	JOIN ret_update_info rui on rcs.clientcode = rui.ClientCode
	LEFT JOIN usersettings.pricesdata pd on pd.pricecode = rcs.PriceCodeOnly
	LEFT JOIN usersettings.clientsdata cd on cd.firmcode = pd.firmcode
WHERE rcs.clientcode = ?ClientCode";
				using (myMySqlDataReader = myMySqlCommand.ExecuteReader())
				{
					myMySqlDataReader.Read();
					VisileStateList.SelectedValue = myMySqlDataReader["InvisibleOnFirm"].ToString();
					VisileStateList.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.RegisterInvisible);

					RegisterCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowRegister"]);
					RejectsCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowRejection"]);
					MultiUserLevelTB.Text = myMySqlDataReader["MultiUserLevel"].ToString();
					AdvertisingLevelCB.Checked = Convert.ToBoolean(myMySqlDataReader["AdvertisingLevel"]);
					WayBillCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowWayBill"]);
					EnableUpdateCB.Checked = Convert.ToBoolean(myMySqlDataReader["EnableUpdate"]);
					CalculateLeaderCB.Checked = Convert.ToBoolean(myMySqlDataReader["CalculateLeader"]);
					AllowSubmitOrdersCB.Checked = Convert.ToBoolean(myMySqlDataReader["AllowSubmitOrders"]);
					SubmitOrdersCB.Checked = Convert.ToBoolean(myMySqlDataReader["SubmitOrders"]);
					ServiceClientCB.Checked = Convert.ToBoolean(myMySqlDataReader["ServiceClient"]);
					OrdersVisualizationModeCB.Checked = Convert.ToBoolean(myMySqlDataReader["OrdersVisualizationMode"]);
					if (Convert.ToInt32(myMySqlDataReader["InvisibleOnFirm"]) != 0)
					{
						var noise = myMySqlDataReader["NotNoisedPriceCode"] != DBNull.Value;
						SetNoiseStatus(noise);
						if (noise)
							NotNoisedPrice.SelectedValue = myMySqlDataReader["NotNoisedPriceCode"].ToString();
					}
					else
					{
						NoiseRow.Visible = false;
						NoisedCosts.Visible = false;
						NotNoisedPriceLabel.Visible = false;
						NotNoisedPrice.Visible = false;
					}
				}

				var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  id,
		cd.FirmCode,
        convert(concat(cd.FirmCode ,'. ' ,cd.ShortName) using cp1251) ShortName,
		ir.IncludeType
FROM  IncludeRegulation ir 
	JOIN ClientsData cd ON cd.firmcode = ir.PrimaryClientCode  
WHERE ir.IncludeClientCode = ?ClientCode
	  and cd.RegionCode & ?AdminMaskRegion
	  {0};
", SecurityContext.Administrator.GetClientFilterByType("cd")), _connection);

				Data = new DataSet();
				adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
				adapter.SelectCommand.Transaction = transaction;
				adapter.Fill(Data, "Include");

				adapter.SelectCommand.CommandText = @"
SELECT rsg.ID, sg.DisplayName, rsg.Enabled
FROM UserSettings.Save_Grids sg
	JOIN UserSettings.Ret_Save_Grids rsg ON sg.Id = rsg.SaveGridId
WHERE rsg.ClientCode = ?ClientCode
ORDER BY sg.DisplayName;
";
				adapter.Fill(Data, "ExportRules");

				adapter.SelectCommand.CommandText = @"
SELECT up.id, up.name, if(ap.UserId is null, 0, 1) as Enabled
FROM (UserSettings.OsUserAccessRight O, UserSettings.UserPermissions up)
  left join UserSettings.AssignedPermissions ap on o.rowid = ap.userid and up.id = ap.PermissionId
where o.clientcode = ?ClientCode
order by up.name;";
				adapter.Fill(Data, "Permissions");

				ShowRegulationHelper.Load(adapter, Data);

				ShowClientsGrid.DataSource = Data.Tables["ShowClients"].DefaultView;
				ShowClientsGrid.DataBind();
				IncludeGrid.DataSource = Data.Tables["Include"].DefaultView;
				IncludeGrid.DataBind();
				ExportRulesList.DataSource = Data.Tables["ExportRules"].DefaultView;
				ExportRulesList.DataBind();
				Permissions.DataSource = Data.Tables["Permissions"].DefaultView;
				Permissions.DataBind();

				for (var i = 0; i < ExportRulesList.Items.Count; i++)
					ExportRulesList.Items[i].Selected = Convert.ToBoolean(Data.Tables["ExportRules"].DefaultView[i]["Enabled"]);

				for (var i = 0; i < Permissions.Items.Count; i++)
					Permissions.Items[i].Selected = Convert.ToBoolean(Data.Tables["Permissions"].DefaultView[i]["Enabled"]);

				transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			SetWorkRegions(HomeRegionCode, true, false);
		}

		protected void GeneratePasswords_Click(object sender, EventArgs e)
		{
			//CommandFactory.SetClientPassword(Convert.ToInt32(ClientCode)).Execute();
			ResultL.Text = "Пароли сгенерированны";
		}

		protected void IncludeGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ProcessChanges();
			Data.Tables["Include"].DefaultView[e.RowIndex].Delete();
			IncludeGrid.DataSource = Data;
			IncludeGrid.DataBind();
		}

		protected void IncludeGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Search":
					var adapter = new MySqlDataAdapter
(@"
SELECT DISTINCT cd.FirmCode, 
       convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName
FROM (clientsdata as cd, clientsdata parent)
	LEFT JOIN includeregulation ir ON ir.includeclientcode = cd.firmcode  
WHERE cd.ShortName like ?SearchText
	  and cd.RegionCode = parent.RegionCode
	  AND cd.RegionCode & ?AdminMaskRegion > 0
      AND cd.FirmStatus = 1  
      AND cd.billingstatus= 1  
      AND cd.FirmType = 1
	  and parent.firmcode = ?ParentRegionCode
ORDER BY cd.shortname;", _connection);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
					adapter.SelectCommand.Parameters.AddWithValue("?ParentRegionCode", ClientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?SearchText", string.Format("%{0}%", ((TextBox)IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText")).Text));
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

					var ParentList = ((DropDownList)IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ParentList"));
					ParentList.DataSource = data;
					ParentList.DataBind();
					ParentList.Visible = data.Tables[0].Rows.Count > 0;
					break;
				case "Add":
					ProcessChanges();
					Data.Tables["Include"].Rows.Add(Data.Tables["Include"].NewRow());
					IncludeGrid.DataSource = Data;
					IncludeGrid.DataBind();
					break;
			}
		}

		private void ProcessChanges()
		{
			foreach (GridViewRow row in IncludeGrid.Rows)
			{
				if (Data.Tables["Include"].DefaultView[row.RowIndex]["IncludeType"].ToString() != ((DropDownList)row.FindControl("IncludeTypeList")).SelectedValue)
					Data.Tables["Include"].DefaultView[row.RowIndex]["IncludeType"] = ((DropDownList)row.FindControl("IncludeTypeList")).SelectedValue;

				if (Data.Tables["Include"].DefaultView[row.RowIndex]["ShortName"].ToString() != ((DropDownList)row.FindControl("ParentList")).SelectedItem.Text)
				{
					Data.Tables["Include"].DefaultView[row.RowIndex]["ShortName"] = ((DropDownList)row.FindControl("ParentList")).SelectedItem.Text;
					Data.Tables["Include"].DefaultView[row.RowIndex]["FirmCode"] = ((DropDownList)row.FindControl("ParentList")).SelectedValue;
				}
			}

			var i = 0;
			foreach (ListItem item in ExportRulesList.Items)
			{
				if (Convert.ToBoolean(Data.Tables["ExportRules"].DefaultView[i]["Enabled"]) != item.Selected)
					Data.Tables["ExportRules"].DefaultView[i]["Enabled"] = Convert.ToInt32(item.Selected);
				i++;
			}

			ShowRegulationHelper.ProcessChanges(ShowClientsGrid, Data);
		}

		protected void IncludeGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				((DropDownList)e.Row.FindControl("ParentList")).Items.Add(new ListItem(((DataRowView)e.Row.DataItem)["ShortName"].ToString(), ((DataRowView)e.Row.DataItem)["FirmCode"].ToString()));
				((DropDownList)e.Row.FindControl("IncludetypeList")).SelectedValue = ((DataRowView)e.Row.DataItem)["IncludeType"].ToString();
				((Button)e.Row.FindControl("SearchButton")).CommandArgument = e.Row.RowIndex.ToString();
			}
		}

		protected void ParentValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = !String.IsNullOrEmpty(args.Value);
		}

		protected void ShowClientsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowDataBound(sender, e);
		}

		protected void ShowClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowCommand(sender,
			                                                e,
			                                                Data);
		}

		protected void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowDeleting(sender, e, Data);
		}

		protected void NoisedCosts_CheckedChanged(object sender, EventArgs e)
		{
			var noise = ((CheckBox) sender).Checked;
			SetNoiseStatus(noise);
		}

		private void SetNoiseStatus(bool noise)
		{
			NoisedCosts.Checked = noise;
			NotNoisedPriceLabel.Visible = noise;
			NotNoisedPrice.Visible = noise;

			var adapter = new MySqlDataAdapter(@"
SELECT pd.PriceCode,
       concat(suppliers.ShortName, ' ', pd.PriceName) as PriceName
FROM Usersettings.ClientsData cd
  JOIN Usersettings.clientsdata suppliers on cd.BillingCode = suppliers.BillingCode and suppliers.FirmType = 0
	JOIN Usersettings.pricesdata pd on pd.firmcode = suppliers.firmcode
WHERE cd.firmcode = ?ClientCode;", Literals.GetConnectionString());
			adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
			var data = new DataSet();
			adapter.Fill(data);
			NotNoisedPrice.DataSource = data;
			NotNoisedPrice.DataBind();
		}
	}
}