using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class manageret : Page
	{
		private int ClientCode;
		private ulong HomeRegionCode;

		protected DataSet _data;

		private DataSet Data
		{
			get { return (DataSet)Session["IncludeData"]; }
			set { Session["IncludeData"] = value; }
		}

		protected void NotifySuppliers_Click(object sender, EventArgs e)
		{
			var client = Client.Find(Convert.ToUInt32(ClientCode));
			new NotificationService().NotifySupplierAboutDrugstoreRegistration(client);
			Mailer.ClientRegistrationResened(client);
		}

		protected void ParametersSave_Click(object sender, EventArgs e)
		{
			if (!IsValid)
				return;

			ProcessChanges();

			var updateCommand = new MySqlCommand();
			updateCommand.Parameters.AddWithValue("?InvisibleOnFirm", VisileStateList.SelectedItem.Value);
			updateCommand.Parameters.AddWithValue("?AlowRegister", RegisterCB.Checked);
			updateCommand.Parameters.AddWithValue("?AlowRejection", RejectsCB.Checked);
			updateCommand.Parameters.AddWithValue("?AdvertisingLevel", AdvertisingLevelCB.Checked);
			updateCommand.Parameters.AddWithValue("?AlowWayBill", WayBillCB.Checked);
			updateCommand.Parameters.AddWithValue("?EnableUpdate", EnableUpdateCB.Checked);
			updateCommand.Parameters.AddWithValue("?CalculateLeader", CalculateLeaderCB.Checked);
			updateCommand.Parameters.AddWithValue("?AllowDelayOfPayment", AllowDelayOfPaymentCB.Checked);
			updateCommand.Parameters.AddWithValue("?ServiceClient", ServiceClientCB.Checked);
			updateCommand.Parameters.AddWithValue("?OrdersVisualizationMode", OrdersVisualizationModeCB.Checked);
			updateCommand.Parameters.AddWithValue("?HomeRegionCode", RegionDD.SelectedItem.Value);
			updateCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

			if (NoisedCosts.Visible && NoisedCosts.Checked)
				updateCommand.Parameters.AddWithValue("?FirmCodeOnly", NotNoisedSupplier.SelectedValue);
			else
				updateCommand.Parameters.AddWithValue("?FirmCodeOnly", null);

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession(s => {
					var connection = (MySqlConnection)s.Connection;

					updateCommand.Connection = connection;
					var tempCommand = new MySqlCommand("", connection);
					tempCommand.CommandText = @"
set @inHost = ?Host;
set @inUser = ?UserName;";
					tempCommand.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
					tempCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

					tempCommand.ExecuteNonQuery();

					tempCommand.CommandText =
						@"
select MaskRegion, OrderRegionMask
from Future.Clients cd
	join retclientsset rcs on cd.Id = rcs.ClientCode
where cd.Id = ?clientCode";
					tempCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

					ulong workRegionMask;
					ulong orderRegionMask;
					using (var reader = tempCommand.ExecuteReader())
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

					updateCommand.Parameters.AddWithValue("?WorkMask", currentWorkRegionMask);
					updateCommand.Parameters.AddWithValue("?OrderMask", currentOrderRegionMask);

					if (currentWorkRegionMask != workRegionMask)
					{
						updateCommand.CommandText =
							@"
UPDATE future.clients SET MaskRegion = ?workMask WHERE id = ?ClientCode;

INSERT
INTO Future.Intersection (
	ClientId,
	RegionId,
	PriceId,
	CostId
)
SELECT  DISTINCT drugstore.Id,
	regions.regioncode,
	pricesdata.pricecode,
	(
		SELECT costcode
		FROM pricescosts pcc
		WHERE basecost AND pcc.PriceCode = pricesdata.PriceCode
	) as CostCode
FROM Future.Clients as drugstore
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.Segment
		JOIN pricesdata ON pricesdata.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pricesdata.pricecode AND i.RegionId = regions.regioncode AND i.ClientId = drugstore.Id
WHERE i.Id IS NULL
	AND supplier.firmtype = 0
	AND drugstore.Id = ?clientCode
	AND drugstore.FirmType = 1;";
					}
					if (VisileStateList.Enabled)
					{
						tempCommand.CommandText = "select InvisibleOnFirm=?InvisibleOnFirm from retclientsset where clientcode=?clientCode";
						tempCommand.Parameters.AddWithValue("?InvisibleOnFirm", VisileStateList.SelectedItem.Value);
						if (Convert.ToInt32(tempCommand.ExecuteScalar()) == 0)
						{
							updateCommand.CommandText += " update retclientsset, intersection, pricesdata set retclientsset.invisibleonfirm=?InvisibleOnFirm, intersection.invisibleonfirm=?InvisibleOnFirm";
							if (Convert.ToInt32(VisileStateList.SelectedValue) != 0)
								updateCommand.CommandText += ", DisabledByFirm=if(PriceType=2, 1, 0), InvisibleOnClient=if(PriceType=2, 1, 0)";
							updateCommand.CommandText += " where intersection.clientcode=retclientsset.clientcode and intersection.pricecode=pricesdata.pricecode and intersection.clientcode=?clientCode; ";
						}
					}
					updateCommand.CommandText += @"
UPDATE UserSettings.retclientsset, 
        Future.Clients
SET OrderRegionMask     = ?orderMask, 
    RegionCode              =?homeRegionCode , 
    WorkRegionMask          =if(WorkRegionMask & ?workMask > 0, WorkRegionMask, ?homeRegionCode), 
    AlowRegister            =?AlowRegister, 
    AlowRejection           =?AlowRejection, 
    AdvertisingLevel        =?AdvertisingLevel, 
    AlowWayBill             =?AlowWayBill, 
    EnableUpdate            =?EnableUpdate, 
    CalculateLeader         = ?CalculateLeader, 
	AllowDelayOfPayment    = ?AllowDelayOfPayment,
    ServiceClient           = ?ServiceClient, 
    OrdersVisualizationMode = ?OrdersVisualizationMode,
	FirmCodeOnly			= ?FirmCodeOnly
where clientcode=Id and Id=?clientCode; ";

					var client = Client.FindAndCheck((uint) ClientCode);

					updateCommand.ExecuteNonQuery();

					for (var i = 0; i < ExportRulesList.Items.Count; i++)
					{
						var table = Data.Tables["ExportRules"];
						var list = ExportRulesList;
						if (Convert.ToBoolean(table.DefaultView[i]["Enabled"]) != list.Items[i].Selected)
						{
							table.DefaultView[i]["Enabled"] = list.Items[i].Selected;
							if (list.Items[i].Selected)
								updateCommand.CommandText = @"
insert into UserSettings.ret_save_grids(ClientCode, SaveGridId)
values(?ClientCode, ?SaveGridId)";
							else
								updateCommand.CommandText = @"
delete from UserSettings.ret_save_grids
where ClientCode = ?ClientCode and SaveGridId = ?SaveGridId";
							updateCommand.Parameters.Clear();
							updateCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
							updateCommand.Parameters.AddWithValue("?SaveGridId", table.DefaultView[i]["Id"]);
							updateCommand.ExecuteNonQuery();
						}
					}

					for (var i = 0; i < PrintRulesList.Items.Count; i++)
					{
						var table = Data.Tables["PrintRules"];
						var list = PrintRulesList;
						if (Convert.ToBoolean(table.DefaultView[i]["Enabled"]) != list.Items[i].Selected)
						{
							table.DefaultView[i]["Enabled"] = list.Items[i].Selected;
							if (list.Items[i].Selected)
								updateCommand.CommandText = @"
insert into UserSettings.ret_save_grids(ClientCode, SaveGridId)
values(?ClientCode, ?SaveGridId)";
							else
								updateCommand.CommandText = @" 
delete from UserSettings.ret_save_grids
where ClientCode = ?ClientCode and SaveGridId = ?SaveGridId";
							updateCommand.Parameters.Clear();
							updateCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
							updateCommand.Parameters.AddWithValue("?SaveGridId", table.DefaultView[i]["Id"]);
							updateCommand.ExecuteNonQuery();
						}
					}

					ShowRegulationHelper.Update(connection, null, Data, ClientCode);
				});
				scope.VoteCommit();
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
        c.MaskRegion & a.regioncode > 0 as RegMask,
        rcs.OrderRegionMask & a.regioncode>0 as OrderMask
FROM    farm.regions as a,
        farm.regions as b,
        Future.Clients c,
        retclientsset rcs
WHERE 
";
			if (!AllRegions)
				sqlCommand += " b.regioncode=?RegCode and ";
			sqlCommand += @"
c.Id = ?ClientId
and a.regioncode & (b.defaultshowregionmask | MaskRegion) > 0
and rcs.clientcode = c.Id
and a.regioncode & ?AdminMask > 0
group by regioncode
order by region";
			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(sqlCommand, c);
					adapter.SelectCommand.Parameters.AddWithValue("?ClientId", ClientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?RegCode", RegCode);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminMask", SecurityContext.Administrator.RegionMask);
					adapter.Fill(_data, "WorkReg");
				});

			WRList.DataSource = _data.Tables["WorkReg"];
			WRList.DataBind();
			OrderList.DataSource = _data.Tables["WorkReg"];
			OrderList.DataBind();
			for (var i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (OldRegion)
				{
					WRList.Items[i].Selected = Convert.ToBoolean(_data.Tables["Workreg"].Rows[i]["RegMask"]);
					OrderList.Items[i].Selected = Convert.ToBoolean(_data.Tables["Workreg"].Rows[i]["OrderMask"]);
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
			ClientCode = Convert.ToInt32(Request["cc"]);
			_data = new DataSet();

			if (IsPostBack) 
				return;

			With.Connection(
				c => {
					var command = new MySqlCommand("", c);
					command.Parameters.AddWithValue("?ClientCode", ClientCode);
					command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);

					var client = Client.FindAndCheck((uint)ClientCode);
					var settings = DrugstoreSettings.Find(client.Id);
					HomeRegionCode = client.HomeRegion.Id;

					var regionAdapter = new MySqlDataAdapter(@"
SELECT  r.regioncode, 
        r.region 
FROM farm.regions r
WHERE r.RegionCode & ?AdminMaskRegion > 0
ORDER BY r.region;", c);
					regionAdapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
					regionAdapter.Fill(_data, "admin");

					command.CommandText = @"
SELECT  InvisibleOnFirm, 
        AlowRegister, 
        AlowRejection, 
        MultiUserLevel, 
        AdvertisingLevel, 
        AlowWayBill, 
        EnableUpdate, 
        CalculateLeader, 
		AllowDelayOfPayment,
        AllowSubmitOrders, 
        SubmitOrders, 
        ServiceClient, 
        OrdersVisualizationMode, 
		rcs.FirmCodeOnly
FROM retclientsset rcs
WHERE rcs.clientcode = ?ClientCode";
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						VisileStateList.SelectedValue = reader["InvisibleOnFirm"].ToString();
						VisileStateList.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.RegisterInvisible);

						RegisterCB.Checked = Convert.ToBoolean(reader["AlowRegister"]);
						RejectsCB.Checked = Convert.ToBoolean(reader["AlowRejection"]);
						AdvertisingLevelCB.Checked = Convert.ToBoolean(reader["AdvertisingLevel"]);
						WayBillCB.Checked = Convert.ToBoolean(reader["AlowWayBill"]);
						EnableUpdateCB.Checked = Convert.ToBoolean(reader["EnableUpdate"]);
						CalculateLeaderCB.Checked = Convert.ToBoolean(reader["CalculateLeader"]);
						AllowDelayOfPaymentCB.Checked = Convert.ToBoolean(reader["AllowDelayOfPayment"]);
						ServiceClientCB.Checked = Convert.ToBoolean(reader["ServiceClient"]);
						OrdersVisualizationModeCB.Checked = Convert.ToBoolean(reader["OrdersVisualizationMode"]);
						if (Convert.ToInt32(reader["InvisibleOnFirm"]) != 0)
						{
							SetNoiseStatus(settings.IsNoised, client);
							if (settings.IsNoised)
								NotNoisedSupplier.SelectedValue = settings.FirmCodeOnly.ToString();
						}
						else
						{
							NoiseRow.Visible = false;
							NoisedCosts.Visible = false;
							NotNoisedSupplierLabel.Visible = false;
							NotNoisedSupplier.Visible = false;
						}
					}

					var adapter = new MySqlDataAdapter("", c);
					Data = new DataSet();
					adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);

					adapter.SelectCommand.CommandText = @"
SELECT sg.Id, sg.DisplayName, rsg.ClientCode is not null as Enabled
FROM UserSettings.Save_Grids sg
	LEFT JOIN UserSettings.Ret_Save_Grids rsg ON sg.Id = rsg.SaveGridId and rsg.ClientCode = ?ClientCode
WHERE sg.Id < 32768
ORDER BY sg.DisplayName;";
					adapter.Fill(Data, "ExportRules");

					adapter.SelectCommand.CommandText = @"
SELECT sg.Id, sg.DisplayName, rsg.ClientCode is not null as Enabled
FROM UserSettings.Save_Grids sg
	LEFT JOIN UserSettings.Ret_Save_Grids rsg ON sg.Id = rsg.SaveGridId and rsg.ClientCode = ?ClientCode
WHERE sg.Id > 16384
ORDER BY sg.DisplayName;";
					adapter.Fill(Data, "PrintRules");

					ShowRegulationHelper.Load(adapter, Data);

					ShowClientsGrid.DataSource = Data.Tables["ShowClients"].DefaultView;
					ShowClientsGrid.DataBind();

					ExportRulesList.DataSource = Data.Tables["ExportRules"].DefaultView;
					ExportRulesList.DataBind();

					PrintRulesList.DataSource = Data.Tables["PrintRules"].DefaultView;
					PrintRulesList.DataBind();

					RegionDD.DataSource = _data.Tables["Admin"].DefaultView;
					RegionDD.DataBind();

					for (var i = 0; i <= RegionDD.Items.Count - 1; i++)
					{
						if (Convert.ToUInt64(RegionDD.Items[i].Value) == HomeRegionCode)
						{
							RegionDD.SelectedIndex = i;
							break;
						}
					}

					for (var i = 0; i < ExportRulesList.Items.Count; i++)
						ExportRulesList.Items[i].Selected = Convert.ToBoolean(Data.Tables["ExportRules"].DefaultView[i]["Enabled"]);

					for (var i = 0; i < PrintRulesList.Items.Count; i++)
						PrintRulesList.Items[i].Selected = Convert.ToBoolean(Data.Tables["PrintRules"].DefaultView[i]["Enabled"]);
				});

			SetWorkRegions(HomeRegionCode, true, false);
		}

		private void ProcessChanges()
		{
			ShowRegulationHelper.ProcessChanges(ShowClientsGrid, Data);
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
			ShowRegulationHelper.ShowClientsGrid_RowCommand(sender, e, Data);
		}

		protected void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ShowRegulationHelper.ShowClientsGrid_RowDeleting(sender, e, Data);
		}

		protected void NoisedCosts_CheckedChanged(object sender, EventArgs e)
		{
			var client = Client.FindAndCheck((uint)ClientCode);
			HomeRegionCode = client.HomeRegion.Id;

			var noise = ((CheckBox) sender).Checked;
			SetNoiseStatus(noise, client);
		}

		private void SetNoiseStatus(bool isNoised, Client client)
		{
			NoisedCosts.Checked = isNoised;
			NotNoisedSupplierLabel.Visible = isNoised;
			NotNoisedSupplier.Visible = isNoised;

			var adapter = new MySqlDataAdapter(@"
select 0 as FirmCode, 'Зашумлять все прайс листы всех поставщиков' as ShortName
union
SELECT suppliers.FirmCode,
       suppliers.ShortName
FROM Usersettings.ClientsData cd
WHERE cd.BillingCode = ?PayerId and suppliers.FirmType = 0;", Literals.GetConnectionString());
			adapter.SelectCommand.Parameters.AddWithValue("?PayerId", client.BillingInstance.PayerID);
			var data = new DataSet();
			adapter.Fill(data);
			NotNoisedSupplier.DataSource = data;
			NotNoisedSupplier.DataBind();
		}
	}
}