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
			new NotificationService().NotifySupplierAboutDrugstoreRegistration(client.Id);
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
			updateCommand.Parameters.AddWithValue("?MultiUserLevel", MultiUserLevelTB.Text);
			updateCommand.Parameters.AddWithValue("?AdvertisingLevel", AdvertisingLevelCB.Checked);
			updateCommand.Parameters.AddWithValue("?AlowWayBill", WayBillCB.Checked);
			updateCommand.Parameters.AddWithValue("?EnableUpdate", EnableUpdateCB.Checked);
			updateCommand.Parameters.AddWithValue("?CalculateLeader", CalculateLeaderCB.Checked);
			updateCommand.Parameters.AddWithValue("?AllowDelayOfPayment", AllowDelayOfPaymentCB.Checked);
			updateCommand.Parameters.AddWithValue("?AllowSubmitOrders", AllowSubmitOrdersCB.Checked);
			updateCommand.Parameters.AddWithValue("?SubmitOrders", SubmitOrdersCB.Checked);
			updateCommand.Parameters.AddWithValue("?ServiceClient", ServiceClientCB.Checked);
			updateCommand.Parameters.AddWithValue("?OrdersVisualizationMode", OrdersVisualizationModeCB.Checked);
			updateCommand.Parameters.AddWithValue("?HomeRegionCode", RegionDD.SelectedItem.Value);
			updateCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

			if (NoisedCosts.Visible && NoisedCosts.Checked)
				updateCommand.Parameters.AddWithValue("?FirmCodeOnly", NotNoisedSupplier.SelectedValue);
			else
				updateCommand.Parameters.AddWithValue("?FirmCodeOnly", null);

			var insertedRelationship = new Dictionary<DataRow, Relationship>();

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession<Client>(s => {
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
from clientsdata cd
	join retclientsset rcs on cd.FirmCode = rcs.ClientCode
where firmcode=?clientCode";
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
		AND clientsdata2.firmtype = 1;";
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
	AllowDelayOfPayment    = ?AllowDelayOfPayment,
    AllowSubmitOrders       = ?AllowSubmitOrders, 
    SubmitOrders            = ?SubmitOrders, 
    ServiceClient           = ?ServiceClient, 
    OrdersVisualizationMode = ?OrdersVisualizationMode,
	FirmCodeOnly			= ?FirmCodeOnly
where clientcode=firmcode and firmcode=?clientCode; ";

					var giveAnalitFPermission = false;
					foreach (DataRow row in Data.Tables["Include"].Rows)
					{
						if (row.RowState != DataRowState.Modified)
							break;

						if (Convert.ToInt32(row["IncludeType", DataRowVersion.Original]) == (int) RelationshipType.Base
							&& (Convert.ToInt32(row["IncludeType", DataRowVersion.Current]) == (int)RelationshipType.Network
								|| Convert.ToInt32(row["IncludeType", DataRowVersion.Current]) == (int)RelationshipType.BasePlus))
							giveAnalitFPermission = true;
					}
					if (giveAnalitFPermission)
					{
						updateCommand.CommandText += @"
insert into Usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId, up.Id
from (Usersettings.Osuseraccessright ouar, Usersettings.UserPermissions up)
  left join Usersettings.AssignedPermissions ap on ap.UserId = ouar.RowId
where ouar.clientcode = ?ClientCode and up.Shortcut = 'AF' and ap.UserId is null;
";
					}

					var client = Client.FindAndCheck((uint) ClientCode);
					Relationship lastUpdateRelationship = null;
					foreach (var row in Data.Tables["Include"].Rows.Cast<DataRow>())
					{
						if (row.RowState == DataRowState.Added)
						{
							var parent = Client.FindAndCheck(Convert.ToUInt32(row["FirmCode"]));
							var relationshipType = (RelationshipType)Convert.ToUInt32(row["IncludeType"]);
							lastUpdateRelationship = client.AddRelationship(parent, relationshipType);
							insertedRelationship.Add(row, lastUpdateRelationship);
						}
						else if (row.RowState == DataRowState.Deleted)
						{
							lastUpdateRelationship = client.Parents.First(r => r.Id == Convert.ToUInt32(row["Id", DataRowVersion.Original]));
							client.RemoveRelationship(lastUpdateRelationship);
						}
						else if (row.RowState == DataRowState.Modified)
						{
							var relationship = client.Parents.First(r => r.Id == Convert.ToUInt32(row["Id"]));
							relationship.RelationshipType = (RelationshipType)Convert.ToUInt32(row["IncludeType"]);
							relationship.Parent = Client.FindAndCheck(Convert.ToUInt32(row["FirmCode"]));
							lastUpdateRelationship = relationship;
						}
					}
					Data.Tables["Include"].AcceptChanges();
					if (lastUpdateRelationship != null)
					{
						var command = new MySqlCommand(SharedCommands.UpdateWorkRules, connection);
						command.Parameters.AddWithValue("?Parent", lastUpdateRelationship.Parent.Id);
						command.Parameters.AddWithValue("?Child", lastUpdateRelationship.Child.Id);
						command.Parameters.AddWithValue("?IncludeType", (uint)lastUpdateRelationship.RelationshipType);
						command.ExecuteNonQuery();
					}

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

			insertedRelationship.Each(k => k.Key["Id"] = k.Value.Id);

			ResultL.ForeColor = Color.Green;
			ResultL.Text = "���������.";
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
			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(sqlCommand, c);
					adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?RegCode", RegCode);
					adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
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

					command.CommandText = @"
SELECT  RegionCode
FROM clientsdata as cd
WHERE cd.firmcode = ?ClientCode ";
					HomeRegionCode = Convert.ToUInt64(command.ExecuteScalar());
					SecurityContext.Administrator.CheckClientHomeRegion(HomeRegionCode);


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
						MultiUserLevelTB.Text = reader["MultiUserLevel"].ToString();
						AdvertisingLevelCB.Checked = Convert.ToBoolean(reader["AdvertisingLevel"]);
						WayBillCB.Checked = Convert.ToBoolean(reader["AlowWayBill"]);
						EnableUpdateCB.Checked = Convert.ToBoolean(reader["EnableUpdate"]);
						CalculateLeaderCB.Checked = Convert.ToBoolean(reader["CalculateLeader"]);
						AllowDelayOfPaymentCB.Checked = Convert.ToBoolean(reader["AllowDelayOfPayment"]);
						AllowSubmitOrdersCB.Checked = Convert.ToBoolean(reader["AllowSubmitOrders"]);
						SubmitOrdersCB.Checked = Convert.ToBoolean(reader["SubmitOrders"]);
						ServiceClientCB.Checked = Convert.ToBoolean(reader["ServiceClient"]);
						OrdersVisualizationModeCB.Checked = Convert.ToBoolean(reader["OrdersVisualizationMode"]);
						if (Convert.ToInt32(reader["InvisibleOnFirm"]) != 0)
						{
							var noise = reader["FirmCodeOnly"] != DBNull.Value;
							SetNoiseStatus(noise);
							if (noise)
								NotNoisedSupplier.SelectedValue = reader["FirmCodeOnly"].ToString();
						}
						else
						{
							NoiseRow.Visible = false;
							NoisedCosts.Visible = false;
							NotNoisedSupplierLabel.Visible = false;
							NotNoisedSupplier.Visible = false;
						}
					}

					var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  id,
		cd.FirmCode,
		convert(concat(cd.FirmCode ,'. ' ,cd.ShortName) using cp1251) ShortName,
		ir.IncludeType
FROM IncludeRegulation ir
	JOIN ClientsData cd ON cd.firmcode = ir.PrimaryClientCode
WHERE ir.IncludeClientCode = ?ClientCode
		and cd.RegionCode & ?AdminMaskRegion
		{0};
", SecurityContext.Administrator.GetClientFilterByType("cd")), c);

					Data = new DataSet();
					adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
					adapter.Fill(Data, "Include");

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

					IncludeGrid.DataSource = Data.Tables["Include"].DefaultView;
					IncludeGrid.DataBind();

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
					var data = new DataSet();
					var searchText = ((TextBox)IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText"));
					With.Connection(
						c => {
							var adapter = new MySqlDataAdapter(@"
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
ORDER BY cd.shortname;", c);
							adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
							adapter.SelectCommand.Parameters.AddWithValue("?ParentRegionCode", ClientCode);
							adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", searchText.Text));
							adapter.Fill(data);
						});

					var parents = ((DropDownList)IncludeGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ParentList"));
					parents.DataSource = data;
					parents.DataBind();
					parents.Visible = data.Tables[0].Rows.Count > 0;
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
			ShowRegulationHelper.ShowClientsGrid_RowCommand(sender, e, Data);
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
			NotNoisedSupplierLabel.Visible = noise;
			NotNoisedSupplier.Visible = noise;

			var adapter = new MySqlDataAdapter(@"
select 0 as FirmCode, '��������� ��� ����� ����� ���� �����������' as ShortName
union
SELECT suppliers.FirmCode,
       suppliers.ShortName
FROM Usersettings.ClientsData cd
  JOIN Usersettings.clientsdata suppliers on cd.BillingCode = suppliers.BillingCode and suppliers.FirmType = 0
WHERE cd.firmcode = ?ClientCode;", Literals.GetConnectionString());
			adapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
			var data = new DataSet();
			adapter.Fill(data);
			NotNoisedSupplier.DataSource = data;
			NotNoisedSupplier.DataBind();
		}
	}
}