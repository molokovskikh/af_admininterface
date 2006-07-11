using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
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
			DS.Tables.AddRange(new DataTable[] {Costs});
			Costs.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2, DataColumn3, DataColumn4});
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
			SelCommand.CommandText = null;
			SelCommand.CommandTimeout = 0;
			SelCommand.CommandType = CommandType.Text;
			SelCommand.Connection = MyCn;
			SelCommand.Transaction = null;
			SelCommand.UpdatedRowSource = UpdateRowSource.Both;
			UpdCommand.CommandText = null;
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
		MySqlDataReader MyReader;
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
			string StrHost;
			string StrUser;
			UpdateLB.Text = "";
			StrHost = HttpContext.Current.Request.UserHostAddress;
			StrUser = HttpContext.Current.User.Identity.Name;
			if (StrUser.Substring(0, 6) == "ANALIT\\")
			{
				StrUser = StrUser.Substring(7);
			}
			try
			{
				MyCn.Open();
				FillDataSet();
				MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted);

				foreach (DataGridItem Itm in CostsDG.Items)
				{
					for (int i = 0; i <= DS.Tables[0].Rows.Count - 1; i++)
					{
						if (DS.Tables[0].Rows[i]["CostCode"].ToString() == Itm.Cells[5].Text)
						{
							if (DS.Tables[0].Rows[i]["CostName"].ToString() != ((TextBox) (Itm.FindControl("CostName"))).Text)
								DS.Tables[0].Rows[i]["CostName"] = ((TextBox) (Itm.Cells[0].FindControl("CostName"))).Text;
							if (DS.Tables[0].Rows[i]["CostCode"].ToString() == Request.Form["uid"].ToString())
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
				UpdCommand.Parameters.Add(new MySqlParameter("CostCode", MySqlDbType.Int32));
				UpdCommand.Parameters["CostCode"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["CostCode"].SourceColumn = "CostCode";
				UpdCommand.Parameters["CostCode"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("CostName", MySqlDbType.VarChar));
				UpdCommand.Parameters["CostName"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["CostName"].SourceColumn = "CostName";
				UpdCommand.Parameters["CostName"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("BaseCost", MySqlDbType.Bit));
				UpdCommand.Parameters["BaseCost"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["BaseCost"].SourceColumn = "BaseCost";
				UpdCommand.Parameters["BaseCost"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("Enabled", MySqlDbType.Bit));
				UpdCommand.Parameters["Enabled"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["Enabled"].SourceColumn = "Enabled";
				UpdCommand.Parameters["Enabled"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add(new MySqlParameter("AgencyEnabled", MySqlDbType.Bit));
				UpdCommand.Parameters["AgencyEnabled"].Direction = ParameterDirection.Input;
				UpdCommand.Parameters["AgencyEnabled"].SourceColumn = "AgencyEnabled";
				UpdCommand.Parameters["AgencyEnabled"].SourceVersion = DataRowVersion.Current;

				UpdCommand.Parameters.Add("Host", HttpContext.Current.Request.UserHostAddress);
				UpdCommand.Parameters.Add("UserName", Session["UserName"]);


				UpdCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;
update pricescosts set BaseCost=?BaseCost, CostName=?CostName, Enabled=?Enabled, AgencyEnabled=?AgencyEnabled where CostCode=?CostCode;
";
				MyDA.Update(DS, "Costs");
				MyTrans.Commit();
				CostsDG.DataBind();
				UpdateLB.Text = "Сохранено.";
			}
			catch (Exception ex)
			{
				ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" + ex.Message +
				             "]";
				MyTrans.Rollback();
			}
			finally
			{
				MyCn.Close();
			}
		}

		public string IsChecked(bool Checked)
		{
			if (Checked)
			{
				return "checked";
			}
			else
			{
				return "";
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			MyCn.ConnectionString = Literals.GetConnectionString();
			PriceCode = Convert.ToInt32(Request["pc"]);
			if (!IsPostBack)
				PostDataToGrid();
		}

		protected void CreateCost_Click(object sender, EventArgs e)
		{
			Int32 FirmCode;
			string ShortName;
			string PriceName;
			UpdCommand.Connection = MyCn;
			UpdCommand.Transaction = MyTrans;

			try
			{
				MyCn.Open();
				MyTrans = MyCn.BeginTransaction();

				UpdCommand.CommandText = "select pd.FirmCode, pd.PriceName, cd.ShortName from pricesdata pd, clientsdata cd" +
				                         " where cd.firmcode=pd.firmcode and pd.pricecode=" + PriceCode;
				MyReader = UpdCommand.ExecuteReader();
				MyReader.Read();
				FirmCode = Convert.ToInt32(MyReader["FirmCode"]);
				ShortName = MyReader["ShortName"].ToString();
				PriceName = MyReader["PriceName"].ToString();
				MyReader.Close();
				UpdCommand.CommandText = "INSERT INTO pricesdata(Firmcode, PriceCode) values(" + FirmCode + ", null); " +
				                         " set @NewPriceCode:=Last_Insert_ID(); insert into farm.formrules(firmcode) values(@NewPriceCode); " +
				                         " insert into farm.sources(FirmCode) values(@NewPriceCode);";
				UpdCommand.CommandText += "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " +
				                          " Select @NewPriceCode, @NewPriceCode, 0, " + PriceCode + ";" +
				                          " Insert into farm.costformrules(PC_CostCode) Select @NewPriceCode;";
				UpdCommand.ExecuteNonQuery();
				MyTrans.Commit();
				Func.SelectTODS(
					"select regionaladmins.username, regions.regioncode, regions.region, regionaladmins.alowcreateretail, regionaladmins.alowcreatevendor, regionaladmins.alowchangesegment, regionaladmins.defaultsegment, AlowCreateInvisible, regionaladmins.email from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" +
					Session["UserName"] + "' order by region", "admin", DS);
				Func.Mail("register@analit.net", "\"" + ShortName + "\" - регистрация ценовой колонки", MailFormat.Text,
				          "Оператор: " + Session["UserName"] + "\nПрайс-лист: " + PriceName + "\n",
				          "RegisterList@subscribe.analit.net", DS.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
				PostDataToGrid();
			}
			catch (Exception ex)
			{
				ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" + ex.Message +
				             "]";
				MyTrans.Rollback();
			}
			finally
			{
				MyCn.Close();
			}
		}

		private void PostDataToGrid()
		{
			try
			{
				if (MyCn.State == ConnectionState.Closed)
					MyCn.Open();
				FillDataSet();
				CostsDG.DataBind();
			}
			catch (Exception ex)
			{
				ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" + ex.Message +"]";
			}
			finally
			{
				MyCn.Close();
			}
		}

		private void FillDataSet()
		{
			SelCommand.CommandText = "select PriceName from (pricesdata) where PriceCode=" + PriceCode;
			MyReader = SelCommand.ExecuteReader();
			MyReader.Read();
			PriceNameLB.Text = MyReader[0].ToString();
			MyReader.Close();
			SelCommand.CommandText =
					" SELECT CostCode, concat(ifnull(ExtrMask, ''), ' - ', if(FieldName='BaseCost', concat(TxtBegin, ' - ', TxtEnd), if(left(FieldName,1)='F'," +
					" concat('№', right(Fieldname, length(FieldName)-1)), Fieldname))) CostID, CostName, BaseCost, pc.Enabled, pc.AgencyEnabled" +
					" FROM (farm.costformrules cf, pricescosts pc, pricesdata pd)" +
					" left join farm.sources s on s.firmcode=pc.pricecode" + " where cf.pc_costcode=pc.costcode" +
					" and pd.pricecode=showpricecode" + " and ShowPriceCode=" + PriceCode;
			MyDA.Fill(DS, "Costs");
		}
	}
}