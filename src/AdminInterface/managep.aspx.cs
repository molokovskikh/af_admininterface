using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Framework.Scopes;
using Common.MySql;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;
using NHibernate.Linq;

namespace AddUser
{
	partial class managep : BasePage
	{
		private readonly Dictionary<object, string> _configuratedCostTypes
			= new Dictionary<object, string> {
				{ 0, "Мультиколоночный" },
				{ 1, "Многофайловый" },
			};

		private readonly Dictionary<object, string> _unconfiguratedCostTypes
			= new Dictionary<object, string> {
				{ 0, "Мультиколоночный" },
				{ 1, "Многофайловый" },
				{ DBNull.Value, "Не настроенный" },
			};

		private DataSet Data
		{
			get { return (DataSet)Session["RegionalSettingsData"]; }
			set { Session["RegionalSettingsData"] = value; }
		}

		private Supplier supplier;

		protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.Administrator.CheckPermisions(PermissionType.ViewSuppliers, PermissionType.ManageSuppliers);
			uint id;
			if (!UInt32.TryParse(Request["cc"], out id))
				throw new ArgumentException(String.Format("Не верное значение ClientCode = {0}", id), "ClientCode");

			supplier = Supplier.Find(id);
			HandlersLink.NavigateUrl = "~/SpecialHandlers/?supplierId=" + supplier.Id;

			if (!IsPostBack)
				LoadPageData();
			else
				ConnectDataSource();
		}

		private void LoadPageData()
		{
			Data = GetData(supplier);
			HeaderLabel.Text = String.Format("Конфигурация клиента \"{0}\"", supplier.Name);
			ConnectDataSource();
			DataBind();
			SetRegions();
		}

		private void ConnectDataSource()
		{
			PricesGrid.DataSource = Data;
			RegionalSettingsGrid.DataSource = Data;
			WorkRegionList.DataSource = Data;
			HomeRegion.DataSource = Data;
			OrderSendRules.DataSource = Data;
		}

		public static DataSet GetData(Supplier supplier)
		{
			var data = new DataSet();

			var pricesCommandText =
				@"
SELECT  pd.PriceCode,
		pd.PriceName,
		pd.AgencyEnabled,
		pd.Enabled,
		pi.PriceDate,
		pd.UpCost,
		pd.PriceType,
		pd.CostType,
		pd.BuyingMatrix
FROM pricesdata pd
	JOIN usersettings.pricescosts pc on pd.PriceCode = pc.PriceCode and pc.BaseCost = 1
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
		s.RegionMask & a.regioncode > 0 as Enable
FROM    farm.regions as a, 
		farm.regions as b, 
		Customers.Suppliers as s
WHERE   b.regioncode = ?HomeRegion
		AND s.Id = ?ClientCode
		AND a.regioncode & ?AdminRegionMask & (b.defaultshowregionmask | s.RegionMask) > 0
GROUP BY regioncode
ORDER BY region;";

			var orderSendConfig = @"
SELECT osr.Id, osr.SenderId, osr.FormaterId, osr.SendDebugMessage, osr.ErrorNotificationDelay, r.RegionCode
FROM ordersendrules.order_send_rules osr
	join Customers.Suppliers s on s.Id = osr.firmcode
	left join farm.regions r on osr.regioncode = r.regioncode
where s.Id = ?ClientCode
	  and (s.RegionMask & ?AdminRegionMask & r.regioncode > 0 or osr.RegionCode is null)
order by s.HomeRegion;";

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
FROM (farm.regions r, Customers.Suppliers s)
where s.Id = ?ClientCode
	  and s.RegionMask & ?AdminRegionMask & r.regioncode > 0
order by r.Region;";


			With.Connection(
				c => {
					var dataAdapter = new MySqlDataAdapter(pricesCommandText, c);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?supplierId", supplier.Id);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", supplier.Id);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", supplier.HomeRegion.Id);

					dataAdapter.Fill(data, "Prices");

					dataAdapter.SelectCommand.CommandText = regionSettingsCommnadText;
					dataAdapter.Fill(data, "RegionSettings");

					dataAdapter.SelectCommand.CommandText = regionsCommandText;
					dataAdapter.Fill(data, "Regions");

					dataAdapter.SelectCommand.CommandText = enableRegionsCommandText;
					dataAdapter.Fill(data, "EnableRegions");

					dataAdapter.SelectCommand.CommandText = orderSendConfig;
					dataAdapter.Fill(data, "OrderSendConfig");

					dataAdapter.SelectCommand.CommandText = senders;
					dataAdapter.Fill(data, "Senders");

					dataAdapter.SelectCommand.CommandText = formaters;
					dataAdapter.Fill(data, "Formaters");

					dataAdapter.SelectCommand.CommandText = sendRuleRegions;
					dataAdapter.Fill(data, "SenRuleRegions");
					var row = data.Tables["SenRuleRegions"].NewRow();
					row["RegionCode"] = DBNull.Value;
					row["Region"] = "Любой регион";
					data.Tables["SenRuleRegions"].Rows.InsertAt(row, 0);
				});
			return data;
		}


		private void SetRegions()
		{
			HomeRegion.SelectedValue = supplier.HomeRegion.Id.ToString();
			for (var i = 0; i < Data.Tables["EnableRegions"].Rows.Count; i++)
				WorkRegionList.Items[i].Selected = Convert.ToBoolean(Data.Tables["EnableRegions"].Rows[i]["Enable"]);
		}

		protected void PricesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName) {
				case "Add":
					var row = Data.Tables["Prices"].NewRow();
					row["UpCost"] = 0;
					row["Enabled"] = false;
					row["AgencyEnabled"] = false;
					row["BuyingMatrix"] = false;
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
			if (!WorkRegionList.Items.Cast<ListItem>().Any(i => i.Selected)) {
				RegionValidationError.Visible = true;
				return;
			}

			RegionValidationError.Visible = false;
			ShowAllRegionsCheck.Checked = false;
			UpdateHomeRegion();
			UpdateMaskRegion();
			ProcessChanges();
			var message = "";
			Save(supplier, Data, HttpContext.Current.Request.UserHostAddress, ref message);

			if (!String.IsNullOrEmpty(message)) {
				messageDiv.InnerText = message;
				messageDiv.Style.Add("display", "block");
			}

			LoadPageData();
		}

		public void Save(Supplier supplier, DataSet data, string host, ref string message)
		{
			var pricesDataAdapter = new MySqlDataAdapter();
			pricesDataAdapter.DeleteCommand = new MySqlCommand(
				@"
Set @InHost = ?UserHost;
Set @InUser = ?UserName;

DELETE FROM PricesData
WHERE PriceCode = ?PriceCode;

DELETE FROM Customers.Intersection
WHERE PriceId = ?PriceCode;

DELETE FROM PricesRegionalData
WHERE PriceCode = ?PriceCode;
");

			pricesDataAdapter.DeleteCommand.Parameters.AddWithValue("?UserHost", host);
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
	BuyingMatrix = ?BuyingMatrix,
	FirmCode = ?ClientCode;
SET @InsertedPriceCode = Last_Insert_ID();

INSERT INTO farm.formrules() VALUES();
SET @NewFormRulesId = Last_Insert_ID();

INSERT INTO farm.sources(RequestInterval) VALUES(IF(?PriceType=1, 86400, NULL));	
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
		Customers.Suppliers s,
		farm.regions r
WHERE   p.PriceCode  = @InsertedPriceCode
		AND p.FirmCode = s.Id
		AND (r.RegionCode & s.RegionMask > 0)
		AND not exists
		(
			SELECT * 
			FROM    pricesregionaldata prd 
			WHERE   prd.PriceCode      = p.PriceCode 
					AND prd.RegionCode = r.RegionCode
		);

select @NewPriceCostId;
");
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserHost", host);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			pricesDataAdapter.InsertCommand.Parameters.AddWithValue("?ClientCode", supplier.Id);
			pricesDataAdapter.InsertCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.InsertCommand.Parameters.Add("?PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.InsertCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("?AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.InsertCommand.Parameters.Add("?BuyingMatrix", MySqlDbType.Bit, 0, "BuyingMatrix");
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
	BuyingMatrix = ?BuyingMatrix
WHERE PriceCode = ?PriceCode;

UPDATE farm.sources fs
JOIN Usersettings.PriceItems pi ON fs.Id = pi.SourceId
JOIN Usersettings.PricesCosts pc ON pi.Id = pc.PriceItemId AND pc.PriceCode = ?PriceCode
SET fs.RequestInterval = IF(?PriceType = 1, 86400, NULL);

call UpdateCostType(?PriceCode, ?CostType);
");

			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost", host);
			pricesDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			pricesDataAdapter.UpdateCommand.Parameters.Add("?UpCost", MySqlDbType.Decimal, 0, "UpCost");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?PriceType", MySqlDbType.Int32, 0, "PriceType");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?Enabled", MySqlDbType.Bit, 0, "Enabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?AgencyEnabled", MySqlDbType.Bit, 0, "AgencyEnabled");
			pricesDataAdapter.UpdateCommand.Parameters.Add("?BuyingMatrix", MySqlDbType.Bit, 0, "BuyingMatrix");
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

			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserHost",
				host);
			regionalSettingsDataAdapter.UpdateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

			var updateIntersection = data.Tables["Prices"].Rows.Cast<DataRow>().Any(r => r.RowState == DataRowState.Added);

			var modifiedIntersection = data.Tables["Prices"].Rows.Cast<DataRow>().Where(r => r.RowState == DataRowState.Modified);
			var vipChangeFlag = false;
			foreach (
				var dataRow in modifiedIntersection.Where(dataRow => Convert.ToInt32(dataRow["PriceType"]) == (int)PriceType.Vip)) {
				ActiveRecordLinqBase<Intersection>.Queryable.Where(i => i.Price.Id == Convert.ToUInt32(dataRow["PriceCode"])).ToList().ForEach(
					inter => {
						inter.AvailableForClient = false;
						ActiveRecordMediator.Save(inter);
					});
				vipChangeFlag = true;
			}
			if (vipChangeFlag) {
				message = "Все клиенты были отключены от VIP прайсов";
			}


			With.Transaction((connection, transaction) => {
				pricesDataAdapter.InsertCommand.Connection = connection;
				pricesDataAdapter.InsertCommand.Transaction = transaction;
				pricesDataAdapter.UpdateCommand.Connection = connection;
				pricesDataAdapter.UpdateCommand.Transaction = transaction;
				pricesDataAdapter.DeleteCommand.Connection = connection;
				pricesDataAdapter.DeleteCommand.Transaction = transaction;
				regionalSettingsDataAdapter.UpdateCommand.Transaction = transaction;
				regionalSettingsDataAdapter.UpdateCommand.Connection = connection;

				pricesDataAdapter.Update(data.Tables["Prices"]);
				regionalSettingsDataAdapter.Update(data.Tables["RegionSettings"]);
				using (var scope = new ConnectionScope(connection, FlushAction.Never)) {
					var currentSupplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(supplier.Id);

					BindRule(currentSupplier, data);
					ActiveRecordMediator.Save(currentSupplier);

					if (updateIntersection) {
						//нагрузка балансируется (один запрос может уйти в одну базу, другой в другую)
						//если код ниже будет выполнен в другой транзакции то в той базе где он выполнится
						//может еще не быть создаваемого прайса

						//FlushAction.Never - что бы не автоматически не запускать транзакцию
						var addedPriceId = currentSupplier.Prices.Max(p => p.Id);
						Maintainer.MaintainIntersection(supplier);
						ArHelper.WithSession(s => {
							s.CreateSQLQuery(
								@"
DROP TEMPORARY TABLE IF EXISTS tmp;
CREATE TEMPORARY TABLE tmp ENGINE MEMORY
SELECT adr.Id, rootAdr.SupplierDeliveryId
FROM Customers.AddressIntersection adr
	join Customers.Intersection ins on ins.Id = adr.IntersectionId
	join pricesdata as rootPrice on
		rootPrice.PriceCode =
			(select min(pricecode) from pricesdata as p where p.firmcode = :supplierId )
		join Customers.Intersection rootIns on rootIns.PriceId = rootPrice.PriceCode
			and rootIns.ClientId = ins.ClientId and rootIns.RegionId = ins.RegionId
		join Customers.AddressIntersection rootAdr on rootadr.AddressId = adr.AddressId
			and rootAdr.IntersectionId = rootIns.Id
WHERE ins.PriceId = :priceId
;

UPDATE Customers.AddressIntersection adr
SET SupplierDeliveryId =
(select tmp.SupplierDeliveryId
from tmp tmp
where tmp.Id = adr.Id
limit 1)
WHERE Exists(select 1 from Customers.Intersection ins where ins.Id = adr.IntersectionId and ins.PriceId = :priceId
);")
								.SetParameter("priceId", addedPriceId)
								.SetParameter("supplierId", supplier.Id)
								.ExecuteUpdate();
						});
					}
					scope.Flush();
				}
			});
		}

		private void BindRule(Supplier supplier, DataSet data)
		{
			foreach (var row in data.Tables["OrderSendConfig"].Rows.Cast<DataRow>()) {
				switch (row.RowState) {
					case DataRowState.Added: {
						var rule = new OrderSendRules();
						rule.Supplier = supplier;
						BindRule(rule, row);
						CreateNewSpecialOrders(supplier, Convert.ToUInt32(row["FormaterId"]));
						supplier.OrderRules.Add(rule);
						break;
					}
					case DataRowState.Deleted: {
						var rule = GetExistRule(supplier, row);
						if (rule == null)
							continue;
						rule.Delete();
						supplier.OrderRules.Remove(rule);
						break;
					}
					case DataRowState.Modified: {
						var rule = GetExistRule(supplier, row);
						if (rule == null)
							continue;
						BindRule(rule, row);
						CreateNewSpecialOrders(supplier, Convert.ToUInt32(row["FormaterId"]));
						break;
					}
				}
			}
			data.Tables["OrderSendConfig"].AcceptChanges();
		}

		private static void BindRule(OrderSendRules rule, DataRow row)
		{
			var formaterId = Convert.ToUInt32(row["FormaterId"]);
			var senderId = Convert.ToUInt32(row["SenderId"]);
			rule.Sender = OrderHandler.Find(senderId);
			rule.Formater = OrderHandler.Find(formaterId);
			rule.SendDebugMessage = Convert.ToBoolean(row["SendDebugMessage"]);
			rule.ErrorNotificationDelay = Convert.ToUInt32(row["ErrorNotificationDelay"]);
			rule.RegionCode = Equals(row["RegionCode"], DBNull.Value) ? null : (ulong?)Convert.ToUInt64(row["RegionCode"]);
		}

		private static OrderSendRules GetExistRule(Supplier supplier, DataRow row)
		{
			var id = Convert.ToUInt32(row["id", DataRowVersion.Original]);
			if (id == 0)
				return null;
			return supplier.OrderRules.FirstOrDefault(r => r.Id == id);
		}

		public void CreateNewSpecialOrders(Supplier supplier, uint formaterId)
		{
			var orderHandler = DbSession.Query<OrderHandler>().FirstOrDefault(t => t.Id == formaterId);
			if(DbSession.Query<SpecialHandler>()
				.FirstOrDefault(t => t.Supplier.Id == supplier.Id && t.Handler.Id == formaterId) == null
				&& !OrderHandler.DefaultHandlerByType[HandlerTypes.Formatter].Contains(orderHandler.ClassName)) {
				var handler = new SpecialHandler {
					Supplier = supplier,
					Handler = DbSession.Query<OrderHandler>().FirstOrDefault(t => t.Id == formaterId)
				};
				// задаем имя по умолчанию
				var handlerName = "Специальный формат";
				// проверяем свободно ли имя и если нет, то ищем свободное
				if(DbSession.Query<SpecialHandler>()
					.Count(t => t.Supplier.Id == supplier.Id && t.Name.ToLower() == handlerName) != 0) {
					handlerName += DateTime.Now.ToString("yyMMdd");
				}
				int i = 1;
				string postfix = "";
				while(DbSession.Query<SpecialHandler>()
					.Count(t => t.Supplier.Id == supplier.Id && t.Name.ToLower() == handlerName + postfix) != 0) {
					postfix = "_" + i;
					i++;
				}
				handler.Name = handlerName + postfix;
				DbSession.Save(handler);
				DbSession.Flush();
			}
		}

		private void ProcessChanges()
		{
			for (var i = 0; i < RegionalSettingsGrid.Rows.Count; i++) {
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
			for (var i = 0; i < PricesGrid.Rows.Count; i++) {
				if (Convert.ToString(Data.Tables["Prices"].DefaultView[i]["UpCost"]) != ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text)
					Data.Tables["Prices"].DefaultView[i]["UpCost"] = ((TextBox)PricesGrid.Rows[i].FindControl("UpCostText")).Text;
				if (Convert.ToString(Data.Tables["Prices"].DefaultView[i]["PriceType"]) != ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue)
					Data.Tables["Prices"].DefaultView[i]["PriceType"] = ((DropDownList)PricesGrid.Rows[i].FindControl("PriceTypeList")).SelectedValue;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["AgencyEnabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked)
					Data.Tables["Prices"].DefaultView[i]["AgencyEnabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("EnableCheck")).Checked;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["Enabled"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked)
					Data.Tables["Prices"].DefaultView[i]["Enabled"] = ((CheckBox)PricesGrid.Rows[i].FindControl("InWorkCheck")).Checked;
				if (Convert.ToBoolean(Data.Tables["Prices"].DefaultView[i]["BuyingMatrix"]) != ((CheckBox)PricesGrid.Rows[i].FindControl("BuyingMatrix")).Checked)
					Data.Tables["Prices"].DefaultView[i]["BuyingMatrix"] = ((CheckBox)PricesGrid.Rows[i].FindControl("BuyingMatrix")).Checked;
				if (Data.Tables["Prices"].DefaultView[i]["CostType"].ToString() != ((DropDownList)PricesGrid.Rows[i].FindControl("CostType")).SelectedValue) {
					var value = ((DropDownList)PricesGrid.Rows[i].FindControl("CostType")).SelectedValue;
					if (value == DBNull.Value.ToString())
						Data.Tables["Prices"].DefaultView[i]["CostType"] = DBNull.Value;
					else
						Data.Tables["Prices"].DefaultView[i]["CostType"] = value;
				}
			}

			for (var i = 0; i < OrderSendRules.Rows.Count; i++) {
				var row = OrderSendRules.Rows[i];
				var dataRow = Data.Tables["OrderSendConfig"].DefaultView[i];

				if (Convert.ToUInt32(((DropDownList)row.FindControl("Sender")).SelectedValue) != Convert.ToUInt32(dataRow["SenderId"]))
					dataRow["Senderid"] = Convert.ToUInt32(((DropDownList)row.FindControl("Sender")).SelectedValue);

				if (Convert.ToUInt32(((DropDownList)row.FindControl("Formater")).SelectedValue) != Convert.ToUInt32(dataRow["FormaterId"]))
					dataRow["FormaterId"] = Convert.ToUInt32(((DropDownList)row.FindControl("Formater")).SelectedValue);

				if (((DropDownList)row.FindControl("Region")).SelectedValue != dataRow["RegionCode"].ToString()) {
					var value = ((DropDownList)row.FindControl("Region")).SelectedValue;
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
		}

		protected void ShowAllRegionsCheck_CheckedChanged(object sender, EventArgs e)
		{
			string commandText;
			if (((CheckBox)sender).Checked) {
				commandText = @"
SELECT  a.RegionCode,   
		a.Region,
		s.RegionMask & a.regioncode > 0 as Enable
FROM farm.regions as a, Customers.Suppliers as s
WHERE a.regionCode & ?AdminRegionMask > 0 and s.Id = ?ClientCode
ORDER BY region;";
			}
			else {
				commandText = @"
SELECT  a.RegionCode,
		a.Region,
		s.RegionMask & a.regioncode > 0 as Enable
FROM    farm.regions as a, 
		farm.regions as b, 
		Customers.Suppliers as s
WHERE   b.regioncode = ?HomeRegion
		and s.Id = ?ClientCode
		and a.regioncode & ?AdminRegionMask & (b.defaultshowregionmask | s.RegionMask) > 0
GROUP BY regioncode
ORDER BY region;";
			}
			Data.Tables["EnableRegions"].Clear();
			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(commandText, c);
					adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", supplier.Id);
					adapter.SelectCommand.Parameters.AddWithValue("?HomeRegion", supplier.HomeRegion.Id);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
					adapter.Fill(Data, "EnableRegions");
				});
			WorkRegionList.DataBind();
			SetRegions();
		}

		private void UpdateMaskRegion()
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
				connection.Open();
				var oldMaskRegion = supplier.RegionMask;
				var newMaskRegion = oldMaskRegion;
				foreach (ListItem item in WorkRegionList.Items) {
					if (item.Selected)
						newMaskRegion |= Convert.ToUInt64(item.Value);
					else
						newMaskRegion &= ~Convert.ToUInt64(item.Value);
				}

				if (oldMaskRegion == newMaskRegion)
					return;

				supplier.RegionMask = newMaskRegion;
				ActiveRecordMediator.SaveAndFlush(supplier);
				//здесь длинная транзакция activerecord, что бы изменения были видны запросам комитем
				SessionScope.Current.Commit();

				var updateCommand = new MySqlCommand(
					@"
SET @InHost = ?UserHost;
SET @InUser = ?UserName;

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
FROM pricesdata p, Customers.Suppliers s, farm.regions r
WHERE s.Id = ?ClientCode
		AND p.FirmCode = s.Id
		AND (r.RegionCode & s.RegionMask > 0)
		AND not exists
		(SELECT * 
		FROM    pricesregionaldata prd 
		WHERE   prd.PriceCode      = p.PriceCode 
				AND prd.RegionCode = r.RegionCode
		);

INSERT INTO regionaldata(FirmCode, RegionCode)
SELECT s.Id, r.RegionCode
FROM Customers.Suppliers s, Farm.Regions r
WHERE   s.Id = ?ClientCode
		AND(r.RegionCode & s.RegionMask) > 0 
		AND NOT exists
		(SELECT *
		FROM regionaldata rd 
		WHERE rd.FirmCode = s.Id
			AND rd.RegionCode = r.RegionCode
		);
", connection);
				updateCommand.Parameters.AddWithValue("?MaskRegion", newMaskRegion);
				updateCommand.Parameters.AddWithValue("?ClientCode", supplier.Id);
				updateCommand.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
				updateCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				updateCommand.ExecuteNonQuery();

				//описание см ст 430
				using (new ConnectionScope(connection, FlushAction.Never))
					Maintainer.MaintainIntersection(supplier);
			}
		}

		private void UpdateHomeRegion()
		{
			var currentHomeRegion = Convert.ToUInt64(HomeRegion.SelectedValue);
			if (supplier.HomeRegion.Id == currentHomeRegion)
				return;

			supplier.HomeRegion = Common.Web.Ui.Models.Region.Find(currentHomeRegion);
			ActiveRecordMediator.SaveAndFlush(supplier);
			//здесь длинная транзакция activerecord, что бы изменения были видны запросам комитем
			SessionScope.Current.Commit();
		}

		protected void RegionalSettingsGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow) {
				var rows = Data.Tables["EnableRegions"].Select(String.Format("RegionCode = {0}", Data.Tables["RegionSettings"].Rows[e.Row.DataItemIndex]["RegionCode"]));
				if (rows.Length > 0) {
					if (Convert.ToBoolean(rows[0]["Enable"]) == false)
						e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5");
				}
				else {
					e.Row.BackColor = ColorTranslator.FromHtml("#B5B5B5");
				}
			}
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
			switch (e.CommandName) {
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
			orderSender.SelectedValue = ((DataRowView)e.Row.DataItem)["SenderId"].ToString();

			orderFormater.DataSource = Data.Tables["Formaters"];
			orderFormater.DataBind();
			orderFormater.SelectedValue = ((DataRowView)e.Row.DataItem)["FormaterId"].ToString();

			region.DataSource = Data.Tables["SenRuleRegions"];
			region.DataBind();
			region.SelectedValue = ((DataRowView)e.Row.DataItem)["RegionCode"].ToString();
		}
	}
}