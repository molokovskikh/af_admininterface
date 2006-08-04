using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class managep : Page
	{
		private MySqlConnection		_connection = new MySqlConnection();
		private string				_userName;
		
		private DataSet _data
		{
			get { return (DataSet) Session["RegionalSettingsData"]; }
			set { Session["RegionalSettingsData"] = value; }
		}
		
		private int _clientCode
		{
			get { return Convert.ToInt32(Session["ClientCode"]); }
			set { Session["ClientCode"] = value; }
		}
		
		private MySqlDataAdapter _dataAdapter
		{
			get { return (MySqlDataAdapter) Session["DataAdapter"]; }
			set { Session["DataAdapter"] = value;}
		}

		protected void Page_Load(object sender, EventArgs e)
		{		
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

			_userName = Session["UserName"].ToString();

			int clientCode;
			if (Int32.TryParse(Request["cc"], out clientCode))
				_clientCode = clientCode;
			else
				throw new ArgumentException(String.Format("Не верное значение ClientCode = {0}", clientCode), "ClientCode");
			
			_connection.ConnectionString = Literals.GetConnectionString();
					
			if (!IsPostBack)
			{
				GetData();
				ConnectDataSource();
				DataBind();
			}
			else
				ConnectDataSource();	
			
		}
		
		private void ConnectDataSource()
		{
			PricesGrid.DataSource = _data;
			RegionalSettingsGrid.DataSource = _data;
			ShowRegionList.DataSource = _data;
			WorkRegionList.DataSource = _data;
			HomeRegion.DataSource = _data;
		}

		private void GetData()
		{
			_data = new DataSet();
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
SELECT RegionCode, Region
FROM farm.regions as a
ORDER BY region 
";

			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(pricesCommandText, _connection);
			_dataAdapter = dataAdapter;
			dataAdapter.SelectCommand.Parameters.Add("ClientCode", _clientCode);
			dataAdapter.SelectCommand.Parameters.Add("UserName", _userName);
			dataAdapter.SelectCommand.Parameters.Add("UserName", _userName);
			
			dataAdapter.Fill(_data, "Prices");
			
			dataAdapter.SelectCommand.CommandText = regionSettingsCommnadText;
			dataAdapter.Fill(_data, "RegionSettings");

			dataAdapter.SelectCommand.CommandText = regionsCommandText;
			dataAdapter.Fill(_data, "Regions");
		
			HeaderLabel.Text = String.Format("Конфигурация клиента \"{0}\"", _data.Tables["Prices"].Rows[0]["ShortName"].ToString());
		}


		public void SetWorkRegions(Int64 RegCode, bool OldRegion)
		{
/*
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
*/
		}


		protected void R_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
/*			string Запрос;
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
					Запрос =
@"
set @inHost = ?Host;
set @inUser = ?UserName;
";
					Запрос += "insert into pricesdata(PriceCode, FirmCode) values(Null, " + ClientCode + "); ";
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
					myMySqlCommand.Parameters.Add("Host", HttpContext.Current.Request.UserHostAddress);
					myMySqlCommand.Parameters.Add("UserName", Session["UserName"]);

					myMySqlCommand.ExecuteNonQuery();
				}
				myTrans.Commit();
				BindData();
			}
			catch (Exception)
			{
				myTrans.Rollback();
			}
			finally
			{
				myMySqlConnection.Close();
			}*/
		}
		protected void PricesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					_data.Tables["Prices"].Rows.Add(_data.Tables["Prices"].NewRow());
					DataBind();
					break;
			}
			
		}
		protected void PricesGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			_data.Tables["Prices"].Rows.RemoveAt(e.RowIndex);
			DataBind();
		}
		
		protected void SaveButton_Click(object sender, EventArgs e)
		{
			
		}
}
}