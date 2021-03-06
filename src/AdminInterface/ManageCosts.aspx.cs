using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;
using NHibernate.Linq;

namespace AddUser
{
	partial class ManageCosts : BasePage
	{
		private uint priceId;
		private uint warningCostId;

		protected void PostB_Click(object sender, EventArgs e)
		{
			UpdateLB.Text = "";

			var dataSet = FillDataSet();
			With.Transaction((c, t) => {
				PriceRegionSettings.DataSource = dataSet;

				foreach (DataGridItem Itm in CostsDG.Items) {
					for (var i = 0; i <= dataSet.Tables[0].Rows.Count - 1; i++) {
						if (dataSet.Tables[0].Rows[i]["CostCode"].ToString() == ((HiddenField)Itm.FindControl("CostCode")).Value) {
							if (dataSet.Tables[0].Rows[i]["CostName"].ToString() != ((TextBox)(Itm.FindControl("CostName"))).Text)
								dataSet.Tables[0].Rows[i]["CostName"] = ((TextBox)(Itm.FindControl("CostName"))).Text;
							if (Convert.ToInt32(dataSet.Tables[0].Rows[i]["Enabled"]) != Convert.ToInt32(((CheckBox)(Itm.FindControl("Ena"))).Checked))
								dataSet.Tables[0].Rows[i]["Enabled"] = Convert.ToInt32(((CheckBox)(Itm.FindControl("Ena"))).Checked);
							if (Convert.ToInt32(dataSet.Tables[0].Rows[i]["AgencyEnabled"]) != Convert.ToInt32(((CheckBox)(Itm.FindControl("Pub"))).Checked))
								dataSet.Tables[0].Rows[i]["AgencyEnabled"] = Convert.ToInt32(((CheckBox)(Itm.FindControl("Pub"))).Checked);
						}
					}
				}

				var price = DbSession.Query<Price>().FirstOrDefault(p => p.Id == priceId);
				for (var i = 0; i < PriceRegionSettings.Rows.Count; i++) {
					dataSet.Tables["PriceRegionSettings"].Rows[i]["Enabled"] = ((CheckBox)PriceRegionSettings.Rows[i].FindControl("EnableCheck")).Checked;
					dataSet.Tables["PriceRegionSettings"].Rows[i]["UpCost"] = ((TextBox)PriceRegionSettings.Rows[i].FindControl("UpCostText")).Text;
					dataSet.Tables["PriceRegionSettings"].Rows[i]["MinReq"] = ((TextBox)PriceRegionSettings.Rows[i].FindControl("MinReqText")).Text;

					if (dataSet.Tables["PriceRegionSettings"].Rows[i]["BaseCost"].ToString() != ((DropDownList)PriceRegionSettings.Rows[i].FindControl("RegionalBaseCost")).SelectedValue) {
						dataSet.Tables["PriceRegionSettings"].Rows[i]["BaseCost"] = ((DropDownList)PriceRegionSettings.Rows[i].FindControl("RegionalBaseCost")).SelectedValue;
					}
				}
				var adapter = new MySqlDataAdapter("", c);
				adapter.UpdateCommand = new MySqlCommand("", c);
				var command = adapter.UpdateCommand;
				command.Parameters.Add(new MySqlParameter("?CostCode", MySqlDbType.Int32));
				command.Parameters["?CostCode"].Direction = ParameterDirection.Input;
				command.Parameters["?CostCode"].SourceColumn = "CostCode";
				command.Parameters["?CostCode"].SourceVersion = DataRowVersion.Current;

				command.Parameters.Add(new MySqlParameter("?CostName", MySqlDbType.VarChar));
				command.Parameters["?CostName"].Direction = ParameterDirection.Input;
				command.Parameters["?CostName"].SourceColumn = "CostName";
				command.Parameters["?CostName"].SourceVersion = DataRowVersion.Current;

				command.Parameters.Add(new MySqlParameter("?Enabled", MySqlDbType.Bit));
				command.Parameters["?Enabled"].Direction = ParameterDirection.Input;
				command.Parameters["?Enabled"].SourceColumn = "Enabled";
				command.Parameters["?Enabled"].SourceVersion = DataRowVersion.Current;

				command.Parameters.Add(new MySqlParameter("?AgencyEnabled", MySqlDbType.Bit));
				command.Parameters["?AgencyEnabled"].Direction = ParameterDirection.Input;
				command.Parameters["?AgencyEnabled"].SourceColumn = "AgencyEnabled";
				command.Parameters["?AgencyEnabled"].SourceVersion = DataRowVersion.Current;

				command.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
				command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);


				command.CommandText =
					@"
set @inHost = ?Host;
set @inUser = ?UserName;

UPDATE pricescosts
SET CostName = ?CostName,
	Enabled = ?Enabled,
	AgencyEnabled = ?AgencyEnabled
WHERE CostCode =?CostCode;
";
				adapter.Update(dataSet, "Costs");

				command.Parameters.Clear();
				command.CommandText =
					@"
UPDATE PricesRegionalData
SET UpCost = ?UpCost,
	MinReq = ?MinReq,
	Enabled = ?Enabled,
	BaseCost = ?BaseCost
WHERE RowID = ?Id
";

				command.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
				command.Parameters.Add("?MinReq", MySqlDbType.Decimal, 0, "MinReq");
				command.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
				command.Parameters.Add("?Id", MySqlDbType.Int32, 0, "RowId");
				command.Parameters.Add("?BaseCost", MySqlDbType.Decimal, 0, "BaseCost");
				adapter.Update(dataSet, "PriceRegionSettings");

				UpdateLB.Text = "Сохранено.";
			});
			DbSession.SessionFactory.Evict(typeof(Price), priceId);
			PostDataToGrid();
		}

		public bool CanDelete(object baseCost)
		{
			if (Convert.ToBoolean(baseCost))
				return false;

			return true;
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

			priceId = Convert.ToUInt32(Request["pc"]);
			if (!IsPostBack)
				PostDataToGrid();
		}

		protected void CreateCost_Click(object sender, EventArgs e)
		{
			var collumnCreator = new CostCollumnCreator(field => NotificationHelper.NotifyAboutRegistration(
				String.Format("\"{0}\" - регистрация ценовой колонки", field.ShortName),
				String.Format(
					@"Оператор: {0}
Поставщик: {1}
Регион: {2}
Прайс-лист: {3}
", field.OperatorName, field.ShortName, field.Region, field.PriceName)));

			collumnCreator.CreateCost(priceId, SecurityContext.Administrator.UserName);

			PostDataToGrid();
		}

		private void PostDataToGrid()
		{
			var data = FillDataSet();
			PriceRegionSettings.DataSource = data;
			CostsDG.DataSource = data;
			DataBind();
		}

		private DataSet FillDataSet()
		{
			var data = new DataSet();
			var price = DbSession.Load<Price>(priceId);
			With.Transaction((c, t) => {
				var supplier = price.Supplier;
				SecurityContext.Administrator.CheckRegion(supplier.HomeRegion.Id);
				PriceNameLB.Text = price.Name;

				if (price.CostType == 0) {
					foreach (DataGridColumn column in CostsDG.Columns) {
						if (column.HeaderText == "Дата ценовой колонки") {
							column.Visible = false;
							break;
						}
					}
				}
				var adapter = new MySqlDataAdapter("", c);
				var command = adapter.SelectCommand;
				command.Parameters.AddWithValue("?PriceCode", price.Id);
				command.CommandText =
					@"
SELECT  pc.CostCode,
		cast(concat(ifnull(ExtrMask, ''), ' - ', if(FieldName='BaseCost', concat(TxtBegin, ' - ', TxtEnd), if(left(FieldName,1)='F',  concat('№', right(Fieldname, length(FieldName)-1)), Fieldname))) as CHAR) CostID,
		pc.CostName,
		pi.PriceDate,
		pc.Enabled,
		pc.AgencyEnabled,
		exists (select * from PricesRegionalData prd where prd.BaseCost = pc.CostCode) as RegionBaseCode
FROM usersettings.pricescosts pc
	JOIN usersettings.PriceItems pi on pi.Id = pc.PriceItemId
		JOIN farm.sources s on pi.SourceId = s.Id
	JOIN farm.costformrules cf on cf.CostCode = pc.CostCode
WHERE pc.PriceCode = ?PriceCode;";

				adapter.Fill(data, "Costs");

				command.CommandText = @"
SELECT  RowId,
		Region,
		UpCost,
		MinReq,
		Enabled,
		BaseCost
FROM PricesRegionalData prd
	JOIN Farm.Regions r ON prd.RegionCode = r.RegionCode
WHERE PriceCode = ?PriceCode
	  and r.RegionCode & ?AdminRegionMask > 0;";

				command.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
				adapter.Fill(data, "PriceRegionSettings");
			});
			return data;
		}

		protected void CostsDG_DeleteCommand(object source, DataGridCommandEventArgs e)
		{
			var costId = Convert.ToUInt32(e.CommandArgument);

			var cost = DbSession.Load<Cost>(costId);
			var skipWarning = ((Button)e.CommandSource).Text == "Все равно удалить";
			if (!skipWarning && cost.IsConfigured) {
				warningCostId = costId;
				PostDataToGrid();
				return;
			}

			With.Transaction((c, t) => {
				var selectCurrentCost = @"
select pd.CostType, f.Id as RuleId, s.Id as SourceId, pi.Id as ItemId, prd.RegionCode as Region, prd.BaseCost as CostCode
from usersettings.PricesCosts pc
	join usersettings.PricesData pd on pd.PriceCode = pc.PriceCode
		join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
			join Farm.FormRules f on f.Id = pi.FormRuleId
			join Farm.sources s on s.Id = pi.SourceId
join usersettings.pricesregionaldata prd on prd.pricecode=pd.pricecode and prd.Enabled=1
where pc.CostCode = ?CostCode;";

				var command = new MySqlCommand(selectCurrentCost, c, t);
				command.Parameters.AddWithValue("?CostCode", costId);
				uint costType;
				uint baseCost;
				uint sourceId;
				uint ruleId;
				uint itemId;
				ulong region;
				var deleteCommandText = "";
				using (var reader = command.ExecuteReader()) {
					reader.Read();
					do {
						costType = reader.GetUInt32("CostType");
						baseCost = reader.GetUInt32("CostCode");
						sourceId = reader.GetUInt32("SourceId");
						ruleId = reader.GetUInt32("RuleId");
						itemId = reader.GetUInt32("ItemId");
						region = reader.GetUInt64("Region");
						deleteCommandText += String.Format(@"
update Customers.Intersection
set CostId = {0}
where CostId = {1}
and RegionId = {2};", baseCost, costId, region);
					} while (reader.Read());
				}

				if (costType == 1) {
					deleteCommandText += @"
delete from Farm.FormRules
where Id = ?RuleId;

delete from Farm.sources
where Id = ?SourceId;

delete from usersettings.PriceItems
where Id = ?ItemId;

delete Farm.Core0
from Farm.Core0
join Farm.CoreCosts on Farm.Core0.Id = Farm.CoreCosts.Core_Id
where Farm.CoreCosts.PC_CostCode = ?CostCode;";
				}

				deleteCommandText += @"
delete from Farm.CoreCosts
where PC_CostCode = ?CostCode;

delete from Farm.CostFormRules
where CostCode = ?CostCode;

delete from usersettings.pricescosts where costcode = ?costcode;";
				command.CommandText = deleteCommandText;
				command.Parameters.AddWithValue("?BaseCostCode", baseCost);
				command.Parameters.AddWithValue("?SourceId", sourceId);
				command.Parameters.AddWithValue("?RuleId", ruleId);
				command.Parameters.AddWithValue("?ItemId", itemId);
				command.ExecuteNonQuery();
			});
			Response.Redirect("ManageCosts.aspx?pc=" + priceId);
		}

		protected bool ShowWarning(uint costId)
		{
			return costId == warningCostId;
		}

		protected object DeleteLabel(uint costId)
		{
			if (costId == warningCostId)
				return "Все равно удалить";
			return "Удалить";
		}

		protected void PriceRegionSettings_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var baseCost = (DropDownList)e.Row.FindControl("RegionalBaseCost");

			var price = DbSession.QueryOver<Price>().Where(t => t.Id == priceId).SingleOrDefault();
			baseCost.DataSource = price.Costs;
			baseCost.DataBind();
			baseCost.SelectedValue = ((DataRowView)e.Row.DataItem)["BaseCost"].ToString();
		}
	}
}
