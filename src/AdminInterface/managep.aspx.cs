using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class managep : Page
	{
		protected DataSet DS1;
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataTable WorkReg;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn7;
		protected DataColumn DataColumn8;
		protected DataColumn DataColumn3;

		MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());

		MySqlCommand myMySqlCommand = new MySqlCommand();
		MySqlTransaction myTrans;
		object ClientCode;
		object HomeRegionCode;
		protected DataTable PD;
		protected DataColumn DataColumn4;
		protected DataColumn DataColumn6;
		protected DataColumn PriceCode;
		protected DataColumn PriceName;
		protected DataColumn AgencyEnabled;
		protected DataColumn Enabled;
		protected DataColumn PriceType;
		protected DataColumn ShowInWeb;
		protected DataColumn AlowInt;

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
			PD = new DataTable();
			DataColumn4 = new DataColumn();
			DataColumn6 = new DataColumn();
			PriceCode = new DataColumn();
			PriceName = new DataColumn();
			AgencyEnabled = new DataColumn();
			Enabled = new DataColumn();
			PriceType = new DataColumn();
			ShowInWeb = new DataColumn();
			AlowInt = new DataColumn();
			DS1.BeginInit();
			Regions.BeginInit();
			WorkReg.BeginInit();
			PD.BeginInit();
			DS1.DataSetName = "NewDataSet";
			DS1.Locale = new CultureInfo("ru-RU");
			DS1.Tables.AddRange(new DataTable[] {Regions, WorkReg, PD});
			Regions.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			WorkReg.Columns.AddRange(new DataColumn[] {DataColumn5, DataColumn7, DataColumn8, DataColumn3});
			WorkReg.TableName = "WorkReg";
			DataColumn5.ColumnName = "RegionCode";
			DataColumn7.ColumnName = "ShowMask";
			DataColumn7.DataType = typeof (Boolean);
			DataColumn8.ColumnName = "RegMask";
			DataColumn8.DataType = typeof (Boolean);
			DataColumn3.ColumnName = "Region";
			PD.Columns.AddRange(
				new DataColumn[]
					{DataColumn4, DataColumn6, PriceCode, PriceName, AgencyEnabled, Enabled, PriceType, ShowInWeb, AlowInt});
			PD.TableName = "PD";
			DataColumn4.ColumnName = "FirmCode";
			DataColumn4.DataType = typeof (uint);
			DataColumn6.ColumnName = "ShortName";
			PriceCode.ColumnName = "PriceCode";
			PriceCode.DataType = typeof (uint);
			PriceName.ColumnName = "PriceName";
			AgencyEnabled.ColumnName = "AgencyEnabled";
			AgencyEnabled.DataType = typeof (Boolean);
			Enabled.ColumnName = "Enabled";
			Enabled.DataType = typeof (Boolean);
			PriceType.ColumnName = "PriceType";
			PriceType.DataType = typeof (Int16);
			ShowInWeb.ColumnName = "ShowInWeb";
			ShowInWeb.DataType = typeof (Boolean);
			AlowInt.ColumnName = "AlowInt";
			AlowInt.DataType = typeof (Boolean);
			DS1.EndInit();
			Regions.EndInit();
			WorkReg.EndInit();
			PD.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			ClientCode = Convert.ToInt32(Request["cc"]);
			if (!(IsPostBack))
			{
				PostDataFromDB();
			}
		}

		private void PostDataFromDB()
		{
			if (Func.SelectTODS(" set @UserName:='" + Session["UserName"] + "'; set @FirmCode:="
			                    + ClientCode +
			                    "; SELECT cd.firmcode, ShortName, pricesdata.PriceCode, PriceName, pricesdata.AgencyEnabled, pricesdata.Enabled, AlowInt," +
			                    " DateCurPrice, DateLastForm" +
			                    " FROM (clientsdata as cd, farm.regions, accessright.regionaladmins, pricesdata, farm.formrules fr," +
			                    " pricescosts pc)" + " where regions.regioncode=cd.regioncode" +
			                    " and pricesdata.firmcode=cd.firmcode " + " and pricesdata.pricecode=fr.firmcode " +
			                    " and pc.showpricecode=pricesdata.pricecode " +
			                    " and cd.regioncode & regionaladmins.regionmask > 0 " + " and regionaladmins.UserName=@UserName" +
			                    " and if(UseRegistrant=1, Registrant=@UserName, 1=1)" + " and AlowManage=1" +
			                    " and AlowCreateVendor=1" + " and cd.firmcode=@FirmCode group by 3", "PD", DS1))
			{
				R.DataBind();
			}
			NameLB.Text = DS1.Tables["PD"].Rows[0]["ShortName"].ToString();
		}

		public void SetWorkRegions(Int64 RegCode, bool OldRegion)
		{
			Func.SelectTODS(
				" select a.RegionCode, a.Region, ShowRegionMask & a.regioncode>0 as ShowMask," +
				" MaskRegion & a.regioncode>0 as RegMask, OrderRegionMask & a.regioncode>0 as OrderMask" +
				" from (farm.regions as a, farm.regions as b, clientsdata, retclientsset)" + " where b.regioncode=" + RegCode +
				" and clientsdata.firmcode=" + ClientCode + " and a.regioncode & b.defaultshowregionmask>0 " +
				" and clientcode=firmcode" + " order by region", "WorkReg", DS1);
			WRList.DataBind();
			ShowList.DataBind();
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (OldRegion)
				{
					WRList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["RegMask"]);
					ShowList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["ShowMask"]);
				}
				else
				{
					ShowList.Items[i].Selected = true;
					if (WRList.Items[i].Value == RegCode.ToString())
					{
						WRList.Items[i].Selected = true;
					}
				}
			}
		}

		protected void R_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			string Запрос;
			try
			{
				myMySqlConnection.Open();
			}
			catch (Exception)
			{
				goto exitTryCatchStatement0;
			}
			exitTryCatchStatement0:
			;
			try
			{
				myMySqlCommand.Connection = myMySqlConnection;
				myMySqlCommand.CommandText = " set @UserName:='" + Session["UserName"]
				                             + "'; set @FirmCode:=" + ClientCode
				                             + "; SELECT max(cd.regioncode & regionaladmins.regionmask > 0"
				                             + " and regionaladmins.UserName=@UserName "
				                             + " and if(UseRegistrant=1, Registrant=@UserName, 1=1) " + " and AlowManage=1 "
				                             + " and AlowCreateVendor=1 "
				                             + " and cd.firmcode=@FirmCode) FROM (clientsdata as cd, accessright.regionaladmins) ";
				if (Convert.ToInt32(myMySqlCommand.ExecuteScalar()) == 1)
				{
					myTrans = myMySqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
					Запрос = "insert into pricesdata(PriceCode, FirmCode) values(Null, " + ClientCode + "); ";
					Запрос += "select @PriceCode:=Last_insert_id(); ";
					Запрос += "insert into farm.formrules(Firmcode) values (@PriceCode); ";
					Запрос += "insert into farm.sources(FirmCode) values (@PriceCode); ";
					Запрос += "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " +
					          " Select @PriceCode, @PriceCode, 1, @PriceCode;" +
					          " Insert into farm.costformrules(PC_CostCode) Select @PriceCode; ";
					Запрос += "insert into pricesregionaldata(regioncode, pricecode, enabled) " +
					          "SELECT regions.regioncode, pricesdata.pricecode, if(pricesdata.pricetype<>1, 1, 0) " +
					          "FROM (clientsdata, farm.regions, pricesdata) " +
					          "left join pricesregionaldata on pricesregionaldata.pricecode=pricesdata.pricecode and pricesregionaldata.regioncode=regions.regioncode " +
					          "where pricesdata.firmcode=clientsdata.firmcode " + "and clientsdata.firmstatus=1 " +
					          "and clientsdata.firmtype=0 " + "and pricesdata.pricecode=@PriceCode " +
					          "and (clientsdata.maskregion & regions.regioncode)>0 " + "and pricesregionaldata.pricecode is null; ";
					Запрос +=
						"insert into intersection(regioncode, clientcode, pricecode, invisibleonfirm, invisibleonclient, CostCode) " +
						"select regions.regioncode, clientsdata.firmcode, pricesdata.pricecode, retclientsset.invisibleonfirm, " +
						" 1 as invisibleonclient, pricesdata.PriceCode " +
						"from (clientsdata, farm.regions, pricesdata, pricesregionaldata, retclientsset) " +
						"left join intersection on intersection.clientcode=clientsdata.firmcode and intersection.pricecode=pricesdata.pricecode and intersection.regioncode=regions.regioncode " +
						"left join clientsdata as b on b.firmcode=pricesdata.firmcode " +
						"where clientsdata.firmstatus=1 and clientsdata.firmsegment=b.firmsegment " + "and clientsdata.firmtype=1 " +
						"and (clientsdata.maskregion & regions.regioncode)>0 " + "and (b.maskregion & regions.regioncode)>0 " +
						"and retclientsset.clientcode=clientsdata.firmcode " + "and pricesdata.PriceCode=@PriceCode " +
						"and intersection.pricecode is null " + "and pricesdata.pricetype<>1 " +
						"and pricesregionaldata.regioncode=regions.regioncode " +
						"and pricesregionaldata.pricecode=pricesdata.pricecode; ";
					myMySqlCommand.Transaction = myTrans;
					myMySqlCommand.CommandText = Запрос;
					myMySqlCommand.ExecuteNonQuery();
				}
				myTrans.Commit();
				PostDataFromDB();
			}
			catch (Exception)
			{
				myTrans.Rollback();
			}
			finally
			{
				myMySqlConnection.Close();
			}
		}
	}
}