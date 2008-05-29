using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Models;
using AdminInterface.Security;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class ManageCosts : Page
	{
		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			DS = new DataSet();
			Costs = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			MyDA = new MySqlDataAdapter();
			SelCommand = new MySqlCommand();
			MyCn = new MySqlConnection();
			UpdCommand = new MySqlCommand();
			DS.BeginInit();
			Costs.BeginInit();
			DS.DataSetName = "NewDataSet";
			DS.Locale = new CultureInfo("ru-RU");
			DS.Tables.AddRange(new[] {Costs});
			Costs.Columns.AddRange(new[] {DataColumn1, DataColumn2, DataColumn3, DataColumn4});
			Costs.TableName = "Costs";
			DataColumn1.ColumnName = "CostCode";
			DataColumn1.DataType = typeof (Int32);
			DataColumn2.ColumnName = "CostName";
			DataColumn3.ColumnName = "BaseCost";
			DataColumn3.DataType = typeof (Boolean);
			DataColumn4.ColumnName = "CostID";
			MyDA.DeleteCommand = null;
			MyDA.InsertCommand = null;
			MyDA.SelectCommand = SelCommand;
			MyDA.UpdateCommand = UpdCommand;
			SelCommand.CommandTimeout = 0;
			SelCommand.CommandType = CommandType.Text;
			SelCommand.Connection = MyCn;
			SelCommand.Transaction = null;
			SelCommand.UpdatedRowSource = UpdateRowSource.Both;
			UpdCommand.CommandTimeout = 0;
			UpdCommand.CommandType = CommandType.Text;
			UpdCommand.Connection = MyCn;
			UpdCommand.Transaction = null;
			UpdCommand.UpdatedRowSource = UpdateRowSource.Both;
			DS.EndInit();
			Costs.EndInit();
		}

		MySqlTransaction MyTrans;
		Int32 PriceCode;
		protected DataSet DS;
		protected MySqlDataAdapter MyDA;
		protected MySqlConnection MyCn;
		protected MySqlCommand SelCommand;
		protected MySqlCommand UpdCommand;
		protected DataTable Costs;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected RadioButton BaseCostRB;
		protected RadioButton BCRB;

		private void Page_Init(object sender, EventArgs e)
		{
			InitializeComponent();
		}

		protected void PostB_Click(object sender, EventArgs e)
		{
			UpdateLB.Text = "";
			try
			{
                FillDataSet();
				MyCn.Open();
				PriceRegionSettings.DataSource = DS;
				MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted);

				foreach (DataGridItem Itm in CostsDG.Items)
				{
					for (var i = 0; i <= DS.Tables[0].Rows.Count - 1; i++)
					{
						if (DS.Tables[0].Rows[i]["CostCode"].ToString() == ((HiddenField)Itm.FindControl("CostCode")).Value)
						{
							if (DS.Tables[0].Rows[i]["CostName"].ToString() != ((TextBox) (Itm.FindControl("CostName"))).Text)
								DS.Tables[0].Rows[i]["CostName"] = ((TextBox) (Itm.FindControl("CostName"))).Text;
							if (DS.Tables[0].Rows[i]["CostCode"].ToString() == Request.Form["uid"])
								DS.Tables[0].Rows[i]["BaseCost"] = true;
							else
								DS.Tables[0].Rows[i]["BaseCost"] = false;
							if (Convert.ToInt32(DS.Tables[0].Rows[i]["Enabled"]) != Convert.ToInt32(((CheckBox)(Itm.FindControl("Ena"))).Checked))
								DS.Tables[0].Rows[i]["Enabled"] = Convert.ToInt32(((CheckBox) (Itm.FindControl("Ena"))).Checked);
							if (Convert.ToInt32(DS.Tables[0].Rows[i]["AgencyEnabled"]) != Convert.ToInt32(((CheckBox)(Itm.FindControl("Pub"))).Checked))
								DS.Tables[0].Rows[i]["AgencyEnabled"] = Convert.ToInt32(((CheckBox)(Itm.FindControl("Pub"))).Checked);
						}
					}
				}

				for (var i = 0; i < PriceRegionSettings.Rows.Count; i++ )
				{
					DS.Tables["PriceRegionSettings"].Rows[i]["Enabled"] = ((CheckBox)PriceRegionSettings.Rows[i].FindControl("EnableCheck")).Checked;
					DS.Tables["PriceRegionSettings"].Rows[i]["UpCost"] = ((TextBox)PriceRegionSettings.Rows[i].FindControl("UpCostText")).Text;
					DS.Tables["PriceRegionSettings"].Rows[i]["MinReq"] = ((TextBox)PriceRegionSettings.Rows[i].FindControl("MinReqText")).Text;
				}
				UpdCommand.Parameters.Add(new MySqlParameter("?CostCode", MySqlDbType.Int32));
				UpdCommand.Parameters["?CostCode"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["?CostCode"].SourceColumn = "CostCode";
				UpdCommand.Parameters["?CostCode"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("?CostName", MySqlDbType.VarChar));
				UpdCommand.Parameters["?CostName"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["?CostName"].SourceColumn = "CostName";
				UpdCommand.Parameters["?CostName"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("?BaseCost", MySqlDbType.Bit));
				UpdCommand.Parameters["?BaseCost"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["?BaseCost"].SourceColumn = "BaseCost";
				UpdCommand.Parameters["?BaseCost"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("?Enabled", MySqlDbType.Bit));
				UpdCommand.Parameters["?Enabled"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["?Enabled"].SourceColumn = "Enabled";
				UpdCommand.Parameters["?Enabled"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("?AgencyEnabled", MySqlDbType.Bit));
				UpdCommand.Parameters["?AgencyEnabled"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["?AgencyEnabled"].SourceColumn = "AgencyEnabled";
				UpdCommand.Parameters["?AgencyEnabled"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
                UpdCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);


				UpdCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;

UPDATE pricescosts 
SET		BaseCost	 =?BaseCost, 
        CostName     =?CostName, 
        Enabled      =?Enabled, 
        AgencyEnabled=?AgencyEnabled 
WHERE   CostCode     =?CostCode;
";
				MyDA.Update(DS, "Costs");

				UpdCommand.Parameters.Clear();
				UpdCommand.CommandText =
@"
UPDATE PricesRegionalData
SET UpCost = ?UpCost,
    MinReq = ?MinReq, 
    Enabled = ?Enabled
WHERE RowID = ?Id
";

				UpdCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
				UpdCommand.Parameters.Add("?MinReq", MySqlDbType.Decimal, 0, "MinReq");
				UpdCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
				UpdCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "RowId");
				MyDA.Update(DS, "PriceRegionSettings");
				
				MyTrans.Commit();
				DataBind();
				UpdateLB.Text = "Сохранено.";
			}
			catch
			{
				MyTrans.Rollback();
				throw;
			}
			finally
			{
				MyCn.Close();
			}
		}

		public string IsChecked(bool Checked)
		{
		    if (Checked)
				return "checked";

		    return "";
		}

	    protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.Administrator.CheckPermisions(PermissionType.ViewSuppliers, PermissionType.ManageSuppliers);

			MyCn.ConnectionString = Literals.GetConnectionString();
			PriceCode = Convert.ToInt32(Request["pc"]);
            if (!IsPostBack)
                PostDataToGrid();
		}

		protected void CreateCost_Click(object sender, EventArgs e)
		{
			Int32 FirmCode;
			var adapter = new MySqlDataAdapter(@"
SELECT  pd.FirmCode, 
        pd.PriceName, 
        cd.ShortName, 
		pd.CostType,
		r.Region
FROM pricesdata pd
	JOIN clientsdata cd on cd.firmcode = pd.firmcode 
		JOIN farm.Regions r on r.RegionCode = cd.RegionCode
WHERE pd.pricecode = ?PriceCode; ", MyCn);
			try
			{
				MyCn.Open();

				adapter.SelectCommand.Transaction = MyCn.BeginTransaction(IsolationLevel.RepeatableRead);

                adapter.SelectCommand.Parameters.AddWithValue("?PriceCode", PriceCode);
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

				var MyReader = adapter.SelectCommand.ExecuteReader();
				MyReader.Read();
				FirmCode = Convert.ToInt32(MyReader["FirmCode"]);
				var ShortName = MyReader["ShortName"].ToString();
				var PriceName = MyReader["PriceName"].ToString();
				var region = MyReader["Region"].ToString();
				var costType = Convert.ToInt32(MyReader["CostType"]);
				MyReader.Close();

                adapter.SelectCommand.Parameters.AddWithValue("?FirmCode", FirmCode);



                if (costType == 0)
                    adapter.SelectCommand.CommandText +=
@"
SELECT pc.PriceItemId
FROM Usersettings.PricesData pd
    JOIN Usersettings.PricesCosts pc on pd.PriceCode = pc.PriceCode
WHERE pd.PriceCode = ?PriceCode and pc.BaseCost = 1
INTO @NewPriceItemId;

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT ?PriceCode, 0, @NewPriceItemId;
SET @NewPriceCostId:=Last_Insert_ID(); 

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId; 
";
                else
                    adapter.SelectCommand.CommandText +=
@"
INSERT INTO farm.formrules() VALUES();   
SET @NewFormRulesId = Last_Insert_ID();

INSERT INTO farm.sources() VALUES(); 
SET @NewSourceId = Last_Insert_ID();

INSERT INTO usersettings.PriceItems(FormRuleId, SourceId) VALUES(@NewFormRulesId, @NewSourceId);
SET @NewPriceItemId = Last_Insert_ID();

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT ?PriceCode, 0, @NewPriceItemId;
SET @NewPriceCostId:=Last_Insert_ID(); 

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId; 
";

			    adapter.SelectCommand.ExecuteNonQuery();

				adapter.SelectCommand.CommandText =
@"
SELECT  regionaladmins.username, 
        regionaladmins.email 
FROM accessright.regionaladmins
WHERE  username = ?UserName;
";

				adapter.Fill(DS, "admin");
				adapter.SelectCommand.Transaction.Commit();
				Func.Mail("register@analit.net", 
					String.Empty, 
					"\"" + ShortName + "\" - регистрация ценовой колонки", 
					false, 
					String.Format(
@"Оператор: {0} 
Поставщик: {1}
Регион: {2}
Прайс-лист: {3}
", SecurityContext.Administrator.UserName, ShortName, region, PriceName),
				          "RegisterList@subscribe.analit.net", String.Empty, DS.Tables["admin"].Rows[0]["email"].ToString());
			}
			catch
			{
				adapter.SelectCommand.Transaction.Rollback();
				throw;
			}
			finally
			{
				MyCn.Close();
			}
			PostDataToGrid();
		}

		private void PostDataToGrid()
		{
			FillDataSet();
			PriceRegionSettings.DataSource = DS;
			DataBind();
		}

		private void FillDataSet()
        {
            try
            {
                MyCn.Open();
                var transaction = MyCn.BeginTransaction(IsolationLevel.ReadCommitted);
                MyDA.SelectCommand.Parameters.AddWithValue("?PriceCode", PriceCode);
                SelCommand.Transaction = transaction;
                SelCommand.CommandText = @"
select pd.PriceName, pd.CostType, cd.regioncode
from pricesdata pd
	join clientsdata cd on cd.firmcode = pd.firmcode
where PriceCode=?PriceCode";
                int costType;
                using (var reader = SelCommand.ExecuteReader())
                {
                    reader.Read();
                    PriceNameLB.Text = reader.GetString("PriceName");
                    costType = reader.GetInt32("CostType");
					SecurityContext.Administrator.CheckClientHomeRegion(reader.GetUInt64("RegionCode"));
                }

                if (costType == 0)
                {
                    foreach (DataGridColumn column in CostsDG.Columns)
                    {
                        if (column.HeaderText == "Дата ценовой колонки")
                        {
                            column.Visible = false;
                            break;
                        }
                    }
                }

                SelCommand.CommandText =
@"
SELECT  pc.CostCode, 
        cast(concat(ifnull(ExtrMask, ''), ' - ', if(FieldName='BaseCost', concat(TxtBegin, ' - ', TxtEnd), if(left(FieldName,1)='F',  concat('№', right(Fieldname, length(FieldName)-1)), Fieldname))) as CHAR) CostID, 
        pc.CostName,
        pi.PriceDate, 
        pc.BaseCost, 
        pc.Enabled, 
        pc.AgencyEnabled  
FROM usersettings.pricescosts pc
    JOIN usersettings.PriceItems pi on pi.Id = pc.PriceItemId
        JOIN farm.sources s on pi.SourceId = s.Id
    JOIN farm.costformrules cf on cf.CostCode = pc.CostCode
WHERE pc.PriceCode = ?PriceCode;";
                
                MyDA.Fill(DS, "Costs");

                SelCommand.CommandText = @"
SELECT  RowId, 
        Region, 
        UpCost, 
        MinReq, 
        Enabled  
FROM PricesRegionalData prd   
    JOIN Farm.Regions r ON prd.RegionCode = r.RegionCode  
WHERE PriceCode = ?PriceCode  
	  and r.RegionCode & ?AdminRegionMask > 0;";

            	SelCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
                MyDA.Fill(DS, "PriceRegionSettings");
                transaction.Commit();
            }
            finally
            {
                MyCn.Close();
            }
        }
	}
}