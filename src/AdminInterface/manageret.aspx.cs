using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using DAL;

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
		MySqlConnection myMySqlConnection = new MySqlConnection();
		MySqlCommand myMySqlCommand = new MySqlCommand();
		MySqlDataReader myMySqlDataReader;
		MySqlTransaction myTrans;
		int ClientCode;
		long HomeRegionCode;
		long WorkMask;
		long OrderMask;
		string InsertCommand;


		[DebuggerStepThrough()]
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

		private DataSet _includeData
		{
			get { return (DataSet)Session["IncludeData"]; }
			set { Session["IncludeData"] = value; }
		}

		protected void ParametersSave_Click(object sender, EventArgs e)
		{
			if (ResetCopyIDCB.Checked & CopyIDWTB.Text.Length < 5 & ResetCopyIDCB.Enabled)
			{
				ResultL.Text = "��������� �� ���������.<br>������� ������� ������ ��������������.";
				ResultL.ForeColor = Color.Red;
				return;
			}
			ProcessChanges();
			myMySqlConnection.Open();
			myTrans = myMySqlConnection.BeginTransaction();
			myMySqlCommand.Transaction = myTrans;
			myMySqlCommand.Parameters.Add("InvisibleOnFirm", InvisibleCB.Checked);
			myMySqlCommand.Parameters.Add("AlowRegister", RegisterCB.Checked);
			myMySqlCommand.Parameters.Add("AlowRejection", RejectsCB.Checked);
			myMySqlCommand.Parameters.Add("MultiUserLevel", MultiUserLevelTB.Text);
			myMySqlCommand.Parameters.Add("AdvertisingLevel", AdvertisingLevelCB.Checked);
			myMySqlCommand.Parameters.Add("AlowWayBill", WayBillCB.Checked);
			myMySqlCommand.Parameters.Add("AlowChangeSegment", ChangeSegmentCB.Checked);
			myMySqlCommand.Parameters.Add("EnableUpdate", EnableUpdateCB.Checked);
			myMySqlCommand.Parameters.Add("ResetIDCause", CopyIDWTB.Text);
			myMySqlCommand.Parameters.Add("CalculateLeader", CalculateLeaderCB.Checked);
			myMySqlCommand.Parameters.Add("AllowSubmitOrders", AllowSubmitOrdersCB.Checked);
			myMySqlCommand.Parameters.Add("SubmitOrders", SubmitOrdersCB.Checked);
			myMySqlCommand.Parameters.Add("ServiceClient", ServiceClientCB.Checked);
			myMySqlCommand.Parameters.Add("OrdersVisualizationMode", OrdersVisualizationModeCB.Checked);
			myMySqlCommand.Parameters.Add("Host", HttpContext.Current.Request.UserHostAddress);
			myMySqlCommand.Parameters.Add("UserName", Session["UserName"]);
			myMySqlCommand.Parameters.Add("HomeRegionCode", RegionDD.SelectedItem.Value);
			myMySqlCommand.Parameters.Add("ClientCode", ClientCode);

			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (WRList.Items[i].Selected)
					WorkMask = WorkMask + Convert.ToInt64(WRList.Items[i].Value);
				if (OrderList.Items[i].Selected)
					OrderMask = OrderMask + Convert.ToInt64(OrderList.Items[i].Value);
			}

			myMySqlCommand.Parameters.Add("WorkMask", WorkMask);
			myMySqlCommand.Parameters.Add("OrderMask", OrderMask);

			try
			{
				if (ResetCopyIDCB.Enabled & ResetCopyIDCB.Checked)
				{
					myMySqlCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;
set @ResetIdCause = ?ResetIdCause;
";
				}
				else
				{
					myMySqlCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;
";					
				}
				myMySqlCommand.ExecuteNonQuery();

				myMySqlCommand.CommandText = "select MaskRegion=?workMask from clientsdata where firmcode=?clientCode";
				if (Convert.ToInt32(myMySqlCommand.ExecuteScalar()) == 0)
				{
					InsertCommand +=
@"
INSERT 
INTO    intersection
        (
                ClientCode, 
                regioncode, 
                pricecode, 
                invisibleonclient, 
                InvisibleonFirm, 
                CostCode
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
FROM    (clientsdata, farm.regions, pricesdata, pricesregionaldata, pricescosts pc)  
LEFT JOIN clientsdata as clientsdata2 
        ON clientsdata2.firmcode=?clientCode  
LEFT JOIN intersection 
        ON intersection.pricecode  =pricesdata.pricecode 
        AND intersection.regioncode=regions.regioncode 
        AND intersection.clientcode=clientsdata2.firmcode  
LEFT JOIN retclientsset as a 
        ON a.clientcode=clientsdata2.firmcode  
WHERE   intersection.pricecode IS NULL 
        AND clientsdata.firmstatus                       =1 
        AND clientsdata.firmsegment                      =clientsdata2.firmsegment  
        AND clientsdata.firmtype                         =0  
        AND pricesdata.firmcode                          =clientsdata.firmcode  
        AND pricesregionaldata.pricecode                 =pricesdata.pricecode  
        AND pricesregionaldata.regioncode                =regions.regioncode  
        AND pricesdata.pricetype                        <>1  
        AND pricesdata.pricecode                         =pc.showpricecode 
        AND (clientsdata.maskregion & regions.regioncode)>0  
        AND (?workMask  & regions.regioncode)            >0;";
				}
				if (InvisibleCB.Enabled)
				{
					myMySqlCommand.CommandText = "select InvisibleOnFirm=?InvisibleOnFirm from retclientsset where clientcode=?clientCode";
					if (Convert.ToInt32(myMySqlCommand.ExecuteScalar()) == 0)
					{
						InsertCommand += " update retclientsset, intersection, pricesdata set retclientsset.invisibleonfirm=?InvisibleOnFirm, intersection.invisibleonfirm=?InvisibleOnFirm";
						if (InvisibleCB.Checked)
							InsertCommand += ", DisabledByFirm=if(PriceType=2, 1, 0), InvisibleOnClient=if(PriceType=2, 1, 0)";
						InsertCommand += " where intersection.clientcode=retclientsset.clientcode and intersection.pricecode=pricesdata.pricecode and intersection.clientcode=?clientCode; ";
					}
				}
				InsertCommand +=
@"
UPDATE UserSettings.retclientsset, 
        UserSettings.clientsdata 
SET OrderRegionMask     =?orderMask, 
        MaskRegion              =?workMask , 
        RegionCode              =?homeRegionCode , 
        WorkRegionMask          =if(WorkRegionMask & ?workMask > 0, WorkRegionMask, ?homeRegionCode), 
        AlowRegister            =?AlowRegister, 
        AlowRejection           =?AlowRejection, 
        MultiUserLevel          =?MultiUserLevel, 
        AdvertisingLevel        =?AdvertisingLevel, 
        AlowWayBill             =?AlowWayBill, 
        AlowChangeSegment       =?AlowChangeSegment, 
        EnableUpdate            =?EnableUpdate, 
        CalculateLeader         = ?CalculateLeader, 
        AllowSubmitOrders       = ?AllowSubmitOrders, 
        SubmitOrders            = ?SubmitOrders, 
        ServiceClient           = ?ServiceClient, 
        OrdersVisualizationMode = ?OrdersVisualizationMode  
";
				if (ResetCopyIDCB.Enabled & ResetCopyIDCB.Checked)
					InsertCommand += ", UniqueCopyID=''";
				InsertCommand += " where clientcode=firmcode and firmcode=?clientCode";

				MySqlDataAdapter adapter = new MySqlDataAdapter();
				adapter.InsertCommand = new MySqlCommand(
@"
INSERT 
INTO    UserSettings.IncludeRegulation  
        SET IncludeClientCode = ?ClientCode,
        PrimaryClientCode     = ?PrimaryClientCode,
        IncludeType           = ?IncludeType;

CALL UpdateInclude(?PrimaryClientCode, ?ClientCode, ?IncludeType);
", myMySqlConnection);
				adapter.InsertCommand.Parameters.Add("ClientCode", ClientCode);
				adapter.InsertCommand.Parameters.Add("IncludeType", MySqlDbType.UInt32, 0, "IncludeType");
				adapter.InsertCommand.Parameters.Add("PrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");
				
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
", myMySqlConnection);
				adapter.UpdateCommand.Parameters.Add("ClientCode", ClientCode);
				adapter.UpdateCommand.Parameters.Add("IncludeType", MySqlDbType.UInt32, 0, "IncludeType");
				adapter.UpdateCommand.Parameters.Add("PrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");
				adapter.UpdateCommand.Parameters.Add("OldPrimaryClientCode", MySqlDbType.UInt32, 0, "FirmCode");
				adapter.UpdateCommand.Parameters["OldPrimaryClientCode"].SourceVersion = DataRowVersion.Original;

				adapter.DeleteCommand = new MySqlCommand(
@"
DELETE FROM UserSettings.IncludeRegulation 
WHERE	id = ?id;
", myMySqlConnection);
				adapter.DeleteCommand.Parameters.Add("id", MySqlDbType.UInt32, 0, "id");

				adapter.DeleteCommand.Transaction = myTrans;
				adapter.UpdateCommand.Transaction = myTrans;
				adapter.InsertCommand.Transaction = myTrans;
				
				adapter.Update(_includeData);

				myMySqlCommand.CommandText = InsertCommand;
				myMySqlCommand.ExecuteNonQuery();
				myTrans.Commit();
			}
			catch (Exception ex)
			{
				myTrans.Rollback();
				throw new Exception("������ �� �������� manageret.aspx", ex);
			}
			finally
			{
				myMySqlConnection.Close();
			}
			ResultL.ForeColor = Color.Green;
			ResultL.Text = "���������.";
		}

		protected void SendMessage_Click(object sender, EventArgs e)
		{
			myMySqlConnection.Open();
			myTrans = myMySqlConnection.BeginTransaction();
			myMySqlCommand.Transaction = myTrans;
			try
			{
				myMySqlCommand.CommandText = "update retclientsset set ShowMessageCount="
											 + SendMessageCountDD.SelectedItem.Value + ", Message=?Message where clientcode="
											 + ClientCode;
				myMySqlCommand.Parameters.Add("Message", MySqlDbType.VarString);
				myMySqlCommand.Parameters["Message"].Value = MessageTB.Text;
				myMySqlCommand.ExecuteNonQuery();
				myTrans.Commit();
				MessageTB.Text = "";
				StatusL.Visible = true;
			}
			catch (Exception err)
			{
				myTrans.Rollback();
				StatusL.Text = err.Message;
				StatusL.ForeColor = Color.Red;
				StatusL.Visible = true;
				return;
			}
			finally
			{
				myMySqlConnection.Close();
			}
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool OldRegion = false;
			if (Convert.ToInt64(RegionDD.SelectedItem.Value) == HomeRegionCode)
				OldRegion = true;

			SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), OldRegion, false);
		}

		private void SetWorkRegions(Int64 RegCode, bool OldRegion, bool AllRegions)
		{
			string SQLTXT =
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
			if (!(AllRegions))
			{
				SQLTXT += " b.regioncode=" + RegCode + " and";
			}
			SQLTXT += " clientsdata.firmcode=" + ClientCode + " and a.regioncode & (b.defaultshowregionmask | MaskRegion)>0 "
					  + " and clientcode=firmcode" + " and regionaladmins.username='"
					  + Session["UserName"] + "'" + " and a.regioncode & regionaladmins.RegionMask > 0"
					  + " group by regioncode" + " order by region";
			Func.SelectTODS(SQLTXT, "WorkReg", DS1);
			WRList.DataBind();
			OrderList.DataBind();
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
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
				SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), true, true);
			else
				SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), true, false);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

			myMySqlConnection.ConnectionString = Literals.GetConnectionString();
			StatusL.Visible = false;
			ClientCode = Convert.ToInt32(Request["cc"]);
			DeletePrepareDataButton.Enabled = File.Exists(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", ClientCode));
			myMySqlCommand.Connection = myMySqlConnection;
			if (!IsPostBack)
			{
				myMySqlConnection.Open();
				myMySqlCommand.Parameters.Add("ClientCode", ClientCode);
				myMySqlCommand.Parameters.Add("UserName", Session["UserName"]);

				myMySqlCommand.CommandText =
@"
SELECT  RegionCode, 
        MaskRegion
FROM    clientsdata as cd, 
        accessright.regionaladmins  
WHERE   cd.regioncode & regionaladmins.regionmask > 0 
        AND UserName                              =?UserName 
        AND FirmType                              =if(AlowCreateRetail+AlowCreateVendor=2, FirmType, if(AlowCreateRetail=1, 1, 0))  
        AND FirmSegment                           =if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)
        AND if(UseRegistrant                      =1, Registrant=?UserName, 1=1)  
        AND AlowManage                            =1 
        AND cd.firmcode                           =?ClientCode 
";
				HomeRegionCode = Convert.ToInt64(myMySqlCommand.ExecuteScalar());
				if (Convert.ToInt32(HomeRegionCode) < 1)
					return;

				Func.SelectTODS(
					"select regions.regioncode, regions.region from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" +
					Session["UserName"] + "' order by region", "admin", DS1);
				RegionDD.DataBind();
				for (int i = 0; i <= RegionDD.Items.Count - 1; i++)
				{
					if (Convert.ToInt64(RegionDD.Items[i].Value) == HomeRegionCode)
					{
						RegionDD.SelectedIndex = i;
						break;
					}
				}
				SetWorkRegions(Convert.ToInt64(HomeRegionCode), true, false);
				myMySqlCommand.CommandText =
@"
SELECT  InvisibleOnFirm, 
        AlowRegister, 
        AlowRejection, 
        MultiUserLevel, 
        AdvertisingLevel, 
        AlowWayBill, 
        retclientsset.AlowChangeSegment, 
        EnableUpdate, 
        AlowCreateInvisible, 
        length(UniqueCopyID)=0 as Length, 
        CalculateLeader, 
        AllowSubmitOrders, 
        SubmitOrders, 
        ServiceClient, 
        OrdersVisualizationMode, 
        ShowMessageCount 
FROM    retclientsset, 
        accessright.regionaladmins 
WHERE   clientcode   = ?ClientCode 
        AND username = ?UserName 
";
				myMySqlDataReader = myMySqlCommand.ExecuteReader();
				myMySqlDataReader.Read();
				InvisibleCB.Checked = Convert.ToBoolean(myMySqlDataReader["InvisibleOnFirm"]);
				InvisibleCB.Enabled = Convert.ToBoolean(myMySqlDataReader["AlowCreateInvisible"]);
				RegisterCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowRegister"]);
				RejectsCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowRejection"]);
				MultiUserLevelTB.Text = myMySqlDataReader["MultiUserLevel"].ToString();
				AdvertisingLevelCB.Checked = Convert.ToBoolean(myMySqlDataReader["AdvertisingLevel"]);
				WayBillCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowWayBill"]);
				ChangeSegmentCB.Checked = Convert.ToBoolean(myMySqlDataReader["AlowChangeSegment"]);
				EnableUpdateCB.Checked = Convert.ToBoolean(myMySqlDataReader["EnableUpdate"]);
				ResetCopyIDCB.Checked = Convert.ToBoolean(myMySqlDataReader["Length"]);
				CalculateLeaderCB.Checked = Convert.ToBoolean(myMySqlDataReader["CalculateLeader"]);
				AllowSubmitOrdersCB.Checked = Convert.ToBoolean(myMySqlDataReader["AllowSubmitOrders"]);
				SubmitOrdersCB.Checked = Convert.ToBoolean(myMySqlDataReader["SubmitOrders"]);
				ServiceClientCB.Checked = Convert.ToBoolean(myMySqlDataReader["ServiceClient"]);
				OrdersVisualizationModeCB.Checked = Convert.ToBoolean(myMySqlDataReader["OrdersVisualizationMode"]);
				MessageLeftL.Visible = (Convert.ToInt32(myMySqlDataReader["ShowMessageCount"]) > 0);
				if (!ResetCopyIDCB.Checked)
				{
					ResetCopyIDCB.Enabled = true;
					CopyIDWTB.Enabled = true;
					IDSetL.Visible = false;
				}
				myMySqlDataReader.Close();

				MySqlDataAdapter adapter = new MySqlDataAdapter(
@"
SELECT  id,
		cd.FirmCode,
        convert(concat(cd.FirmCode ,'. ' ,cd.ShortName) using cp1251) ShortName,
		ir.IncludeType
FROM    IncludeRegulation ir 
INNER JOIN ClientsData cd 
        ON cd.firmcode       = ir.PrimaryClientCode  
WHERE   ir.IncludeClientCode =  ?ClientCode;
", Literals.GetConnectionString());

				_includeData = new DataSet();
				adapter.SelectCommand.Parameters.Add("ClientCode", ClientCode);
				adapter.Fill(_includeData);

				IncludeGrid.DataSource = _includeData.Tables[0].DefaultView;				
				IncludeGrid.DataBind();

				myMySqlConnection.Close();
			}
		}

		protected void GeneratePasswords_Click(object sender, EventArgs e)
		{
			CommandFactory.SetClientPassword(Convert.ToInt32(ClientCode)).Execute();
			ResultL.Text = "������ ��������������";
		}
		
		protected void IncludeGrid_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
		{
			ProcessChanges();
			_includeData.Tables[0].DefaultView[e.RowIndex].Delete();
			IncludeGrid.DataSource = _includeData;
			IncludeGrid.DataBind();
		}

		protected void IncludeGrid_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Search":
					MySqlDataAdapter adapter = new MySqlDataAdapter
(@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName
FROM    (accessright.showright, clientsdata as cd)  
LEFT JOIN includeregulation ir 
        ON ir.includeclientcode              =cd.firmcode  
WHERE   cd.regioncode & showright.regionmask > 0  
        AND showright.UserName               =?UserName  
        AND FirmType                         =if(ShowRet+ShowOpt=2, FirmType, if(ShowRet=1, 1, 0)) 
        AND if(UseRegistrant                 =1, Registrant=?UserName, 1=1)  
        AND cd.ShortName like ?SearchText
        AND FirmStatus   =1  
        AND billingstatus=1  
        AND FirmType     =1  
        AND ir.primaryclientcode is null  
ORDER BY cd.shortname;
", Literals.GetConnectionString());
					adapter.SelectCommand.Parameters.Add("UserName", Session["UserName"]);
					adapter.SelectCommand.Parameters.Add("SearchText", string.Format("%{0}%", ((TextBox)IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText")).Text));


					DataSet data = new DataSet();
					adapter.Fill(data);

					DropDownList ParentList = ((DropDownList) IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ParentList"));
					ParentList.DataSource = data;
					ParentList.DataBind();
					ParentList.Visible = data.Tables[0].Rows.Count > 0;
					break;
				case "Add":
					ProcessChanges();
					_includeData.Tables[0].Rows.Add(_includeData.Tables[0].NewRow());
					IncludeGrid.DataSource = _includeData;
					IncludeGrid.DataBind();
					break;
			}
		}

		private void ProcessChanges()
		{
			foreach (GridViewRow row in IncludeGrid.Rows)
			{
				if (_includeData.Tables[0].DefaultView[row.RowIndex]["IncludeType"].ToString() != ((DropDownList)row.FindControl("IncludeTypeList")).SelectedValue)
					_includeData.Tables[0].DefaultView[row.RowIndex]["IncludeType"] = ((DropDownList)row.FindControl("IncludeTypeList")).SelectedValue;
				
				if (_includeData.Tables[0].DefaultView[row.RowIndex]["ShortName"].ToString() != ((DropDownList)row.FindControl("ParentList")).SelectedItem.Text)
				{
					_includeData.Tables[0].DefaultView[row.RowIndex]["ShortName"] = ((DropDownList)row.FindControl("ParentList")).SelectedItem.Text;
					_includeData.Tables[0].DefaultView[row.RowIndex]["FirmCode"] = ((DropDownList)row.FindControl("ParentList")).SelectedValue;
				}
				
			}
		}

		protected void IncludeGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				((DropDownList)e.Row.FindControl("ParentList")).Items.Add(new ListItem(((DataRowView)e.Row.DataItem)["ShortName"].ToString(), ((DataRowView)e.Row.DataItem)["FirmCode"].ToString()));
				((DropDownList) e.Row.FindControl("IncludetypeList")).SelectedValue = ((DataRowView) e.Row.DataItem)["IncludeType"].ToString();
				((Button)e.Row.FindControl("SearchButton")).CommandArgument = e.Row.RowIndex.ToString();
			}
		}

		protected void ParentValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = !String.IsNullOrEmpty(args.Value);
		}

		protected void DeletePrepareDataButton_Click(object sender, EventArgs e)
		{
			File.Delete(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", ClientCode));
			DeletePrepareDataButton.Enabled = false;
		}
}
}