using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Common.MySql;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using log4net;
using MySql.Data.MySqlClient;
using AdminInterface.Helpers;

namespace AdminInterface
{
	public partial class RegisterPage : Page
	{
		protected DataSet data = new DataSet();

		private void SetWorkRegions(string regCode, bool allRegions)
		{
			string commandText;
			if (allRegions)
			{
				commandText = @"
SELECT  a.RegionCode, 
        a.Region, 
        (b.defaultshowregionmask & ?RegionCode) > 0 as ShowMask, 
        a.regioncode = ?RegionCode as RegMask 
FROM    farm.regions as a,
        farm.regions as b
WHERE   a.regioncode & b.defaultshowregionmask       > 0 
        AND a.regioncode & ?AdminRegionMask > 0 
GROUP BY regioncode 
ORDER BY region;";
			}
			else
			{
				commandText = @"
SELECT  a.RegionCode, 
        a.Region, 
        (b.defaultshowregionmask & ?RegionCode) > 0 as ShowMask, 
        a.regioncode = ?RegionCode as RegMask 
FROM    farm.regions as a, 
        farm.regions as b
WHERE   b.regioncode = ?RegionCode 
        AND a.regioncode & b.defaultshowregionmask   > 0 
        AND a.regioncode & ?AdminRegionMask > 0 
GROUP BY regioncode 
ORDER BY region;";
			}

			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(commandText, c);
					adapter.SelectCommand.Parameters.AddWithValue("?RegionCode", regCode);
					adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
					adapter.Fill(data, "WorkReg");
				});

			WRList.DataSource = data.Tables["WorkReg"];
			WRList.DataBind();
			OrderList.DataSource = data.Tables["WorkReg"];
			OrderList.DataBind();
			for (var i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (WRList.Items[i].Value == regCode)
					WRList.Items[i].Selected = true;
				OrderList.Items[i].Selected = WRList.Items[i].Selected;
			}
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);
		}

		protected void Register_Click(object sender, EventArgs e)
		{
			if (!IsValid)
				return;

			uint billingCode = 0;
			uint clientCode = 0;
			var shortname = ShortNameTB.Text.Replace("№", "N");
			var username = LoginTB.Text.Trim().ToLower();
			var password = Func.GeneratePassword();
			Int64 maskRegion = 0;
			Int64 orderMask = 0;
			for (var i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (WRList.Items[i].Selected)
					maskRegion += Convert.ToInt64(WRList.Items[i].Value);
				if (OrderList.Items[i].Selected)
					orderMask += Convert.ToInt64(OrderList.Items[i].Value);
			}

			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession<Client>(s => {
					var connection = (MySqlConnection) s.Connection;
					var command = new MySqlCommand("", connection);
					DbLogHelper.SetupParametersForTriggerLogging<Client>(SecurityContext.Administrator.UserName, HttpContext.Current.Request.UserHostAddress);

					command.Parameters.AddWithValue("?MaskRegion", maskRegion);
					command.Parameters.AddWithValue("?OrderMask", orderMask);
					command.Parameters.AddWithValue("?fullname", FullNameTB.Text);

					command.Parameters.AddWithValue("?shortname", shortname);
					command.Parameters.AddWithValue("?BeforeNamePrefix", "");
					if (TypeDD.SelectedItem.Text == "Аптека")
						command.Parameters["?BeforeNamePrefix"].Value = "Аптека";
					command.Parameters.AddWithValue("?firmsegment", SegmentDD.SelectedItem.Value);
					command.Parameters.AddWithValue("?RegionCode", RegionDD.SelectedItem.Value);
					command.Parameters.AddWithValue("?adress", AddressTB.Text);
					command.Parameters.AddWithValue("?firmtype", TypeDD.SelectedItem.Value);
					command.Parameters.AddWithValue("?registrant", SecurityContext.Administrator.UserName);
					command.Parameters.Add("?ClientCode", MySqlDbType.Int32);
					command.Parameters.AddWithValue("?OSUserName", username);
					command.Parameters.AddWithValue("?ServiceClient", ServiceClient.Checked);
					command.Parameters.AddWithValue("?invisibleonfirm", CustomerType.SelectedItem.Value);

					if (!String.IsNullOrEmpty(Suppliers.SelectedValue))
						command.Parameters.AddWithValue("?FirmCodeOnly", Suppliers.SelectedValue);
					else
						command.Parameters.AddWithValue("?FirmCodeOnly", null);

					if (IncludeCB.Checked)
						billingCode = Convert.ToUInt32(new MySqlCommand("select billingcode from clientsdata where firmcode=" + IncludeSDD.SelectedValue, connection).ExecuteScalar());
					else if (!PayerPresentCB.Checked || (PayerPresentCB.Checked && PayerDDL.SelectedItem == null))
						billingCode = CreatePayer(command);
					else
						billingCode = Convert.ToUInt32(PayerDDL.SelectedItem.Value);

					clientCode = CreateClient(billingCode, command);

					if (TypeDD.SelectedItem.Text == "Аптека")
						CreateDrugstore(CustomerType.SelectedItem.Text != "Стандартный", command);
					else
						CreateSupplier(command);

					if (IncludeCB.Checked)
						CreateRelationship(Client.FindAndCheck(clientCode), connection);

					ADHelper.CreateUserInAD(username,
											password,
											clientCode.ToString());

					CreateFtpDirectory(String.Format(@"\\acdcserv\ftp\optbox\{0}\", clientCode),
					                   String.Format(@"ANALIT\{0}", username));
				});
				scope.VoteCommit();
			}

			if (TypeDD.SelectedItem.Text == "Аптека"
				&& !IncludeCB.Checked
				&& !ServiceClient.Checked
				&& CustomerType.SelectedItem.Text == "Стандартный"
				|| (TypeDD.SelectedItem.Text == "Аптека"
					&& IncludeCB.Checked
					&& !ServiceClient.Checked
					&& IncludeType.SelectedItem.Text != "Скрытый"))
			{
				new NotificationService().NotifySupplierAboutDrugstoreRegistration(clientCode);
			}

			if (TypeDD.SelectedItem.Text == "Поставщик")
				Mailer.SupplierRegistred(shortname, RegionDD.SelectedItem.Text);

			NotificationHelper.NotifyAboutRegistration(
				String.Format("\"{0}\" - успешная регистрация", FullNameTB.Text),
				String.Format("Оператор: {0}\nРегион: {1}\nИмя пользователя: {2}\nКод: {3}\n\nСегмент: {4}\nТип: {5}",
							  SecurityContext.Administrator.UserName,
							  RegionDD.SelectedItem.Text,
							  username,
							  clientCode,
							  SegmentDD.SelectedItem.Text,
							  TypeDD.SelectedItem.Text));

			Session["DogN"] = billingCode;
			Session["Code"] = clientCode;
			Session["Name"] = FullNameTB.Text;
			Session["ShortName"] = shortname;
			Session["Login"] = username;
			Session["Password"] = password;
			Session["Tariff"] = TypeDD.SelectedItem.Text;
			Session["Register"] = true;

			var log = SendClientCardIfNeeded(clientCode, billingCode, shortname, username, password);
			log.Save();

			var sendBillingNotificationNow = true;
			string redirectTo;
			if (IsBasicClient())
			{
				redirectTo = String.Format("client/{0}", clientCode);
			}
			else
			{
				if (!IncludeCB.Checked && EnterBillingInfo.Checked)
				{
					sendBillingNotificationNow = false;
					redirectTo = String.Format("Register/Register.rails?id={0}&clientCode={2}&showRegistrationCard={1}",
											   billingCode,
											   ShowRegistrationCard.Checked,
											   clientCode);
				}
				else if (ShowRegistrationCard.Checked)
					redirectTo = "report.aspx";
				else
					redirectTo = String.Format("Client/{0}", clientCode);
			}

			if (sendBillingNotificationNow)
				new NotificationService()
					.SendNotificationToBillingAboutClientRegistration(clientCode,
																	  billingCode,
																	  shortname,
																	  SecurityContext.Administrator.UserName,
																	  IncludeType.SelectedItem.Text,
																	  null);
			Response.Redirect(redirectTo);
		}

		private bool IsBasicClient()
		{
			return (IncludeCB.Checked && IncludeType.SelectedItem.Text == "Базовый");
		}

		public bool IsSlaveClient()
		{
			return IncludeCB.Checked;
		}

		private PasswordChangeLogEntity SendClientCardIfNeeded(uint clientCode,
		                                    uint billingCode,
		                                    string shortname,
		                                    string username,
		                                    string password)
		{
			var log = new PasswordChangeLogEntity(HttpContext.Current.Request.UserHostAddress,
			                                      SecurityContext.Administrator.UserName,
			                                      username);
			if (!SendRegistrationCard.Checked)
				return log;

			if (IsBasicClient())
				return log;

			string mailTo;

			if (TBOrderManagerMail.Text.Trim().Length == 0 && TBClientManagerMail.Text.Trim().Length == 0)
				mailTo = EmailTB.Text;
			else
				mailTo = TBClientManagerMail.Text.Trim() + ", " + TBOrderManagerMail.Text.Trim();


			var smtpid = ReportHelper.SendClientCard(clientCode,
			                                         billingCode,
			                                         shortname,
			                                         FullNameTB.Text,
			                                         TypeDD.SelectedItem.Text,
			                                         username,
			                                         password,
			                                         mailTo,
			                                         AdditionEmailToSendRegistrationCard.Text,
			                                         true);

			log.SetSentTo(smtpid, EmailHelper.JoinMails(mailTo, AdditionEmailToSendRegistrationCard.Text));
			return log;
		}

		protected void PayerPresentCB_CheckedChanged(object sender, EventArgs e)
		{
			if (PayerPresentCB.Checked)
			{
				PayerPresentCB.Text = "Плательщик существует:";
				PayerFTB.Visible = true;
				FindPayerB.Visible = true;
			}
			else
			{
				PayerPresentCB.Text = "Плательщик существует";
				PayerDDL.Visible = false;
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
				PayerCountLB.Visible = false;
			}
		}

		protected void FindPayerB_Click(object sender, EventArgs e)
		{
			With.Connection(c => {
				var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  DISTINCT PayerID, 
        convert(concat(PayerID, '. ', p.ShortName) using cp1251) PayerName  
FROM clientsdata as cd
	JOIN billing.payers p ON cd.BillingCode = p.PayerId
WHERE   cd.regioncode & ?AdminRegionCode > 0 
        AND firmstatus = 1 
        AND billingstatus = 1 
        AND p.ShortName like ?SearchText  
		{0}
ORDER BY p.shortname;", SecurityContext.Administrator.GetClientFilterByType("cd")), c);
				adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionCode", SecurityContext.Administrator.RegionMask);
				adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", PayerFTB.Text));
				adapter.Fill(data, "Payers");

			});

			PayerDDL.DataSource = data.Tables["Payers"];
			PayerDDL.DataBind();
			PayerCountLB.Text = "[" + PayerDDL.Items.Count + "]";
			PayerCountLB.Visible = true;
			if (PayerDDL.Items.Count > 0)
			{
				PayerDDL.Visible = true;
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
			}
		}

		protected void IncludeCB_CheckedChanged(object sender, EventArgs e)
		{
			EnterBillingInfo.Enabled = !IncludeCB.Checked;
			EnterBillingInfo.Checked = !IncludeCB.Checked;
			PayerPresentCB.Enabled = !IncludeCB.Checked;
			PayerPresentCB.Checked = false;
			CustomerType.SelectedIndex = 0;
			InheritProperties.Visible = false;
			InheritProperties.Checked = false;
			InheritFrom.Visible = false;
			if (IncludeCB.Checked)
			{
				IncludeCB.Text = "Подчинен клиенту:";
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
				RegionDD.Enabled = false;
				TypeDD.Enabled = false;
				TypeDD.SelectedIndex = 0;
				SegmentDD.Enabled = false;
				CustomerType.Enabled = false;
				WRList.Enabled = false;
				PayerDDL.Visible = false;
				PayerCountLB.Visible = false;
				IncludeSTB.Visible = true;
				IncludeSB.Visible = true;
			}
			else
			{
				RegionDD.Enabled = true;
				TypeDD.Enabled = true;
				SegmentDD.Enabled = true;
				CustomerType.Enabled = true;
				WRList.Enabled = true;
				IncludeCB.Text = "Подчиненный клиент";
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
				IncludeSDD.Visible = false;
				IncludeType.Visible = false;
				IncludeCountLB.Visible = false;
			}
		}

		protected void IncludeSB_Click(object sender, EventArgs e)
		{
			With.Connection(c => {
				var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName, 
        cd.RegionCode  
FROM clientsdata as cd
LEFT JOIN includeregulation ir 
        ON ir.includeclientcode = cd.firmcode  
WHERE cd.regioncode & ?AdminRegionMask > 0  
      AND FirmStatus = 1  
      AND billingstatus = 1  
      AND FirmType = 1  
      AND ir.primaryclientcode is null  
	  AND cd.ShortName like ?SearchText 
	  {0}
ORDER BY cd.shortname;", SecurityContext.Administrator.GetClientFilterByType("cd")), c);

				adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionMask", SecurityContext.Administrator.RegionMask);
				adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", IncludeSTB.Text));
				adapter.Fill(data, "Includes");
			});

			IncludeSDD.DataSource = data.Tables["Includes"];
			IncludeSDD.DataBind();
			IncludeCountLB.Text = "[" + IncludeSDD.Items.Count + "]";
			IncludeCountLB.Visible = true;
			if (data.Tables["Includes"].Rows.Count > 0)
			{
				RegionDD.SelectedValue = data.Tables["Includes"].Rows[0]["RegionCode"].ToString();
				SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);

				IncludeType.Visible = true;
				IncludeSDD.Visible = true;
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
				InheritProperties.Visible = true;
			}	
		}

		private uint CreatePayer(MySqlCommand command)
		{
			command.CommandText =
				"insert into billing.payers(OldTariff, OldPayDate, Comment, PayerID, ShortName, JuridicalName, BeforeNamePrefix, ContactGroupOwnerId) values(0, now(), 'Дата регистрации: " +
				DateTime.Now + "', null, ?ShortName, ?fullname, ?BeforeNamePrefix, ?BillingContactGroupOwnerId); ";
			command.CommandText += "SELECT LAST_INSERT_ID()";
			command.Parameters.AddWithValue("?BillingContactGroupOwnerId", CreateContactsForBilling(command.Connection));
			return Convert.ToUInt32(command.ExecuteScalar());
		}

		private uint CreateClient(uint billingCode, MySqlCommand command)
		{
			command.CommandText = @"
INSERT INTO usersettings.clientsdata (
MaskRegion, FullName, ShortName, FirmSegment, RegionCode, Adress, 
FirmType, FirmStatus, registrant, BillingCode, BillingStatus, ContactGroupOwnerId, RegistrationDate) ";
			command.Parameters.AddWithValue("?ClientContactGroupOwnerId",
			                                 CreateContactsForClientsData(command.Connection,
			                                                              TypeDD.SelectedItem.Text == "Аптека"
			                                                              	? ClientType.Drugstore
			                                                              	: ClientType.Supplier));
			if (!IncludeCB.Checked)
			{
				command.CommandText += @" 
Values(?maskregion, ?FullName, ?ShortName, ?FirmSegment, ?RegionCode, ?Adress, 
?FirmType, 1, ?registrant, " + billingCode + ", 1, ?ClientContactGroupOwnerId, now()); ";
			}
			else
			{
				command.CommandText += @"
select maskregion, ?FullName, ?ShortName, FirmSegment, RegionCode, ?Adress, 
FirmType, 1, ?registrant, BillingCode, BillingStatus, ?ClientContactGroupOwnerId, now()
from usersettings.clientsdata where firmcode=" + IncludeSDD.SelectedValue + "; ";
			}
			command.CommandText += "SELECT LAST_INSERT_ID()";
			var clientCode = Convert.ToUInt32(command.ExecuteScalar());
			command.Parameters["?ClientCode"].Value = clientCode;
			CreateUser(command);
			return clientCode;
		}

		private void CreateUser(MySqlCommand command)
		{
			
			command.CommandText = @"
INSERT INTO usersettings.osuseraccessright (ClientCode, OSUserName) Values(?ClientCode, ?OSUserName);
SET @NewUserId = Last_Insert_ID();
insert into logs.AuthorizationDates(ClientCode, UserId) Values(?ClientCode, @NewUserId);";
			if (PermissionsDiv.Visible)
			{
				foreach (ListItem item in Permissions.Items)
				{
					if (item.Selected)
					{
						command.CommandText += String.Format(@"
INSERT INTO usersettings.AssignedPermissions(UserId, PermissionId) 
Values(@NewUserId, {0});", item.Value);
					}
				}
			}
			command.ExecuteNonQuery();
		}

		public void CreateSupplier(MySqlCommand command)
		{
			command.CommandText = @"
INSERT INTO OrderSendRules.order_send_rules(Firmcode, FormaterId, SenderId)
VALUES(?ClientCode,
		(SELECT id FROM OrderSendRules.order_handlers o WHERE ClassName = 'DefaultFormater2'),
		(SELECT id FROM OrderSendRules.order_handlers o WHERE ClassName = 'EmailSender'));

INSERT INTO pricesdata(Firmcode, PriceCode) VALUES(?ClientCode, null);   
SET @NewPriceCode:=Last_Insert_ID(); 

INSERT INTO farm.formrules() VALUES();
SET @NewFormRulesId = Last_Insert_ID();
INSERT INTO farm.sources() VALUES(); 
SET @NewSourceId = Last_Insert_ID();

INSERT INTO usersettings.PriceItems(FormRuleId, SourceId) VALUES(@NewFormRulesId, @NewSourceId);
SET @NewPriceItemId = Last_Insert_ID();

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT @NewPriceCode, 1, @NewPriceItemId;
SET @NewPriceCostId:=Last_Insert_ID(); 

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId; 

INSERT 
INTO    regionaldata
        (
                regioncode, 
                firmcode
        )  
SELECT  DISTINCT regions.regioncode, 
        clientsdata.firmcode  
FROM (clientsdata, farm.regions, pricesdata)  
	LEFT JOIN regionaldata ON regionaldata.firmcode = clientsdata.firmcode AND regionaldata.regioncode = regions.regioncode  
WHERE   pricesdata.firmcode = clientsdata.firmcode  
        AND clientsdata.firmcode = ?ClientCode  
        AND (clientsdata.maskregion & regions.regioncode)>0  
        AND regionaldata.firmcode is null; 

INSERT 
INTO    pricesregionaldata
        (
                regioncode, 
                pricecode
        )  
SELECT  DISTINCT regions.regioncode, 
        pricesdata.pricecode  
FROM    (clientsdata, farm.regions, pricesdata, clientsdata as a)  
LEFT JOIN pricesregionaldata 
        ON pricesregionaldata.pricecode = pricesdata.pricecode 
        AND pricesregionaldata.regioncode = regions.regioncode  
WHERE   pricesdata.firmcode = clientsdata.firmcode  
        AND clientsdata.firmcode = ?ClientCode  
        AND (clientsdata.maskregion & regions.regioncode)>0  
        AND pricesregionaldata.pricecode is null; 


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
FROM pricesdata 
	JOIN clientsdata ON pricesdata.firmcode = clientsdata.firmcode
		JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
			JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmtype = 0
		AND pricesdata.PriceCode = @NewPriceCode
		AND clientsdata2.firmtype = 1;";
			command
				.ExecuteNonQuery();
		}

		private void CreateDrugstore(bool invisible, MySqlCommand command)
		{
			var defaultValues = DefaultValues.Get();
			command.Parameters.AddWithValue("?AnalitFVersion", defaultValues.AnalitFVersion);

			command.CommandText = @"
INSERT INTO usersettings.retclientsset 
		(ClientCode, InvisibleOnFirm, OrderRegionMask, BasecostPassword, ServiceClient, FirmCodeOnly) 
Values  (?ClientCode, ?InvisibleOnFirm, ?OrderMask, GeneratePassword(), ?ServiceClient, ?FirmCodeOnly);

insert into usersettings.ret_save_grids(ClientCode, SaveGridId)
select ?ClientCode, sg.id
from usersettings.save_grids sg
where sg.AssignDefaultValue = 1;

INSERT INTO usersettings.UserUpdateInfo(UserId, AFAppVersion) Values (@NewUserId, ?AnalitFVersion);

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
        AND clientsdata.firmtype = 0
		AND clientsdata2.FirmCode = ?clientCode
		AND clientsdata2.firmtype = 1;";

			if (!invisible)
				command.CommandText += " insert into inscribe(ClientCode) values(?ClientCode); ";
			command.ExecuteNonQuery();
		}

		private void CreateRelationship(Client client, MySqlConnection connection)
		{
			var type = (RelationshipType) Convert.ToUInt32(IncludeType.SelectedValue);
			var parent = Client.FindAndCheck(Convert.ToUInt32(IncludeSDD.SelectedValue));
			client.AddRelationship(parent, type);

			var command = new MySqlCommand(SharedCommands.UpdateWorkRules, connection);
			command.Parameters.AddWithValue("?Parent", parent.Id);
			command.Parameters.AddWithValue("?Child", client.Id);
			command.Parameters.AddWithValue("?IncludeType", (uint)type);
			command.ExecuteNonQuery();
		}

		protected void IncludeSDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			With.Connection(c => {
				var command = new MySqlCommand("SELECT RegionCode FROM clientsdata WHERE firmcode = ?firmCode;", c);
				command.Parameters.AddWithValue("?firmCode", IncludeSDD.SelectedValue);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
						RegionDD.SelectedValue = reader[0].ToString();
				}
			});
			UpdateInherit();
			SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.RegisterDrugstore,
			                                                    PermissionType.RegisterSupplier);

			With.Connection(c => {
				var adapter = new MySqlDataAdapter(@"
SELECT  r.regioncode, 
        r.region
FROM farm.regions as r
WHERE r.regioncode & ?AdminRegioMask > 0 
ORDER BY region;", c);
				adapter.SelectCommand.Parameters.AddWithValue("?AdminRegioMask", SecurityContext.Administrator.RegionMask);
				adapter.Fill(data, "admin");
			});


			if (IsPostBack) 
				return;

			if (SecurityContext.Administrator.HavePermisions(PermissionType.RegisterInvisible))
				CustomerType.Visible = true;

			RegionDD.DataSource = data.Tables["admin"];
			RegionDD.DataBind();

			for (var i = 0; i <= RegionDD.Items.Count - 1; i++)
			{
				if (RegionDD.Items[i].Text == data.Tables["admin"].Rows[0]["region"].ToString())
				{
					RegionDD.SelectedIndex = i;
					break;
				}
			}

			var regionCode = data.Tables["admin"].Rows[0]["regioncode"].ToString();
			SetWorkRegions(regionCode, CheckBox1.Checked);
			if (SecurityContext.Administrator.HavePermisions(PermissionType.RegisterDrugstore))
			{
				TypeDD.Items.Add("Аптека");
				TypeDD.Items[0].Value = "1";
			}
			if (SecurityContext.Administrator.HavePermisions(PermissionType.RegisterSupplier))
			{
				TypeDD.Items.Add("Поставщик");
				TypeDD.Items[TypeDD.Items.Count - 1].Value = "0";
			}
			if (TypeDD.Items.Count == 1)
				TypeDD.Enabled = false;

			if (!SecurityContext.Administrator.HavePermisions(PermissionType.RegisterInvisible))
                CustomerType.Items.Remove(CustomerType.Items[1]);

			SegmentDD.Items.Add("Опт");
			SegmentDD.Items[0].Value = "0";
			SegmentDD.Items.Add("Розница");
			SegmentDD.Items[1].Value = "1";
			ClientTypeChanged(null, EventArgs.Empty);
			
			UpdatePermissions();
		}

		protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			var checkBox = sender as CheckBox;
			SetWorkRegions(RegionDD.SelectedItem.Value, checkBox.Checked);
		}

		private void CreateFtpDirectory(string directory, string userName)
		{
#if !DEBUG
			try
			{
				var supplierDirectory = Directory.CreateDirectory(directory);
				var supplierDirectorySecurity = supplierDirectory.GetAccessControl();
				supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
				                                                                 FileSystemRights.Read,
				                                                                 InheritanceFlags.ContainerInherit |
				                                                                 InheritanceFlags.ObjectInherit,
				                                                                 PropagationFlags.None,
				                                                                 AccessControlType.Allow));
				supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
				                                                                 FileSystemRights.Write,
				                                                                 InheritanceFlags.ContainerInherit |
				                                                                 InheritanceFlags.ObjectInherit,
				                                                                 PropagationFlags.None,
				                                                                 AccessControlType.Allow));
				supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
				                                                                 FileSystemRights.ListDirectory,
				                                                                 InheritanceFlags.ContainerInherit |
				                                                                 InheritanceFlags.ObjectInherit,
				                                                                 PropagationFlags.None,
				                                                                 AccessControlType.Allow));
				supplierDirectory.SetAccessControl(supplierDirectorySecurity);

				var ordersDirectory = Directory.CreateDirectory(directory + "Orders\\");
				var ordersDirectorySecurity = supplierDirectory.GetAccessControl();
				ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
				                                                               FileSystemRights.DeleteSubdirectoriesAndFiles,
				                                                               InheritanceFlags.ContainerInherit |
				                                                               InheritanceFlags.ObjectInherit,
				                                                               PropagationFlags.None,
				                                                               AccessControlType.Allow));
				ordersDirectory.SetAccessControl(ordersDirectorySecurity);

				Directory.CreateDirectory(directory + "Docs\\");
				Directory.CreateDirectory(directory + "Rejects\\");
				Directory.CreateDirectory(directory + "Waybills\\");
			}
			catch(Exception e)
			{
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Order, Docs, Rejects, Waybills
Дать логину {1} право читать, писать и получать список директорий и удалять под директории в папке Orders", directory, userName),
																										  e);
			}
#endif
		}

		protected void LoginValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			var message = ValidateLogin(args.Value);

			if (String.IsNullOrEmpty(message))
			{
				args.IsValid = true;
				LoginValidator.Visible = false;
			}
			else
			{
				args.IsValid = false;
				LoginValidator.Visible = true;
				LoginValidator.ErrorMessage = message;
			}
		}

		public string ValidateLogin(string login)
		{
			if (IsBasicClient())
				return null;

			if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(login.Trim()))
				return "Поле «Имя пользователя» должно быть заполнено";

			var match = new Regex(@"^\s*[a-z|0-9|_]+\s*$").Match(login);
			if (!match.Success)
				return "Имя пользователя должно начинаться с буквы";

			match = new Regex(@"^\s*[a-z|0-9|_]+\s*$").Match(login);
			if (!match.Success)
				return "Имя пользователя может содержать буквы латинского алфавита, цифры и символ подчеркивания, другие символы не допускаются";
			
			var existsInDataBase = false;
			With.Connection(c => {
			                	existsInDataBase = Convert.ToUInt32(new MySqlCommand("select Max(osusername='" + login + "') as Present from (osuseraccessright)", c).ExecuteScalar()) == 1;
			                });
					
			var existsInActiveDirectory = ADHelper.IsLoginExists(login);
			if (existsInActiveDirectory || existsInDataBase)
				return String.Format("Имя пользователя '{0}' существует в системе.", login);

			return null;
		}

		protected void TypeValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = args.Value == "Поставщик" && !IncludeCB.Checked;
		}

		private object CreateContactsForClientsData(MySqlConnection connection, ClientType clientType)
		{
			var contactGroupOwnerId = GetNewContactsGroupOwnerId(connection);

			CreateContactGroup(ContactGroupType.General,
			                   EmailHelper.NormalizeEmailOrPhone(PhoneTB.Text),
			                   EmailHelper.NormalizeEmailOrPhone(EmailTB.Text),
			                   null,
			                   connection,
			                   contactGroupOwnerId);
			CreateContactGroup(ContactGroupType.OrderManagers,
			                   EmailHelper.NormalizeEmailOrPhone(TBOrderManagerPhone.Text),
			                   EmailHelper.NormalizeEmailOrPhone(TBOrderManagerMail.Text),
			                   TBOrderManagerName.Text,
			                   connection,
			                   contactGroupOwnerId);

			if (clientType == ClientType.Supplier)
				CreateContactGroup(ContactGroupType.ClientManagers,
				                   EmailHelper.NormalizeEmailOrPhone(TBClientManagerPhone.Text),
				                   EmailHelper.NormalizeEmailOrPhone(TBClientManagerMail.Text),
				                   TBClientManagerName.Text,
				                   connection,
				                   contactGroupOwnerId);

			return contactGroupOwnerId;
		}


		private static object CreateContactsForBilling(MySqlConnection connection)
		{
			var contactGroupOwnerId = GetNewContactsGroupOwnerId(connection);
			CreateContactGroup(ContactGroupType.Billing,
			                   null,
			                   null,
			                   null,
			                   connection,
			                   contactGroupOwnerId);
			return contactGroupOwnerId;
		}

		private static void CreateContactGroup(ContactGroupType contactGroupType,
		                                       string phone,
		                                       string email,
		                                       string person,
		                                       MySqlConnection connection,
		                                       object contactGroupOwnerId)
		{
			var innerCommand = connection.CreateCommand();
			var contactGroupID = GetNewContactsOwnerId(connection);
			innerCommand.CommandText =
				@"
insert into contacts.contact_groups(Id, Name, Type, ContactGroupOwnerId) 
values(?ID, ?Name, ?Type, ?ContactGroupOwnerId);";

			innerCommand.Parameters.AddWithValue("?Type", contactGroupType);
			innerCommand.Parameters.AddWithValue("?Name", BindingHelper.GetDescription(contactGroupType));
			innerCommand.Parameters.AddWithValue("?ContactGroupOwnerId", contactGroupOwnerId);
			innerCommand.Parameters.AddWithValue("?ID", contactGroupID);
			innerCommand.ExecuteNonQuery();

			innerCommand = connection.CreateCommand();
			innerCommand.CommandText =
				@"
insert into contacts.contacts(Type, ContactText, ContactOwnerId) 
values(?Type, ?Contact, ?ContactOwnerId);";

			innerCommand.Parameters.AddWithValue("?ContactOwnerId", contactGroupID);
			innerCommand.Parameters.AddWithValue("?Contact", phone);
			innerCommand.Parameters.AddWithValue("?Type", 1);
			if (!String.IsNullOrEmpty(phone))
				innerCommand.ExecuteNonQuery();

			if (!String.IsNullOrEmpty(email))
			{
				innerCommand.Parameters["?Contact"].Value = email;
				innerCommand.Parameters["?Type"].Value = 0;
				innerCommand.ExecuteNonQuery();
			}

			if (!String.IsNullOrEmpty(person))
			{
				innerCommand = connection.CreateCommand();
				innerCommand.CommandText =
					@"
insert into contacts.persons(Id, Name, ContactGroupId) 
values(?Id, ?Name, ?ContactGroupId);";

				innerCommand.Parameters.AddWithValue("?ContactGroupId", contactGroupID);
				innerCommand.Parameters.AddWithValue("?Id", GetNewContactsOwnerId(connection));
				innerCommand.Parameters.AddWithValue("?Name", person);
				innerCommand.ExecuteNonQuery();
			}
		}

		private static object GetNewContactsOwnerId(MySqlConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"
insert into contacts.contact_owners values();
select Last_Insert_ID();";
			return command.ExecuteScalar();
		}

		private static object GetNewContactsGroupOwnerId(MySqlConnection connection)
		{
			var innerCommand = connection.CreateCommand();
			innerCommand.CommandText = @"
insert into contacts.contact_group_owners values();
select Last_Insert_ID();";

			return innerCommand.ExecuteScalar();
		}

		protected void ClientTypeChanged(object sender, EventArgs e)
		{
			var isCusomer = TypeDD.SelectedItem.Text == "Аптека";
			CustomerType.Enabled = isCusomer;
			ServiceClient.Enabled = isCusomer;
			IncludeCB.Enabled = isCusomer;
			UpdatePermissions();
			if (!isCusomer)
			{
				IncludeCB.Checked = false;
				OrderList.Visible = false;
				OrderRegionsLabel.Visible = false;
			}
			else
			{
				OrderList.Visible = true;
				OrderRegionsLabel.Visible = true;
			}

			OrderManagerGroupLabel.InnerText = isCusomer
			                                   	? "Ответственный за работу с программой:"
			                                   	: "Ответственный за отправку прайс-листа:";
			ClientManagerGropBlock.Visible = !isCusomer;
		}

		private void UpdatePermissions()
		{
			var clientType = Convert.ToUInt32(TypeDD.SelectedValue);
			var permissionsData = new DataSet();
			var dataAdapter = new MySqlDataAdapter(@"
select id, name, shortcut
from Usersettings.UserPermissions
where AvailableFor = ?ClientType or AvailableFor = 2
order by name", Literals.GetConnectionString());
			dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientType", clientType);
			dataAdapter.Fill(permissionsData);
			Permissions.DataSource = permissionsData.Tables[0];
			Permissions.DataBind();

			if (clientType == 1)
			{
				var afPermissionIndex = 0;
				foreach (DataRow row in permissionsData.Tables[0].Rows)
				{
					if (row["shortcut"].ToString() == "AF")
						break;
					afPermissionIndex++;
				}

				if (afPermissionIndex < Permissions.Items.Count)
					Permissions.Items[afPermissionIndex].Selected = true;
			}

			PermissionsDiv.Visible = permissionsData.Tables[0].Rows.Count > 0;
		}

		protected void InheritProperties_CheckedChanged(object sender, EventArgs e)
		{
			InheritFrom.Visible = InheritProperties.Checked;
			UpdateInherit();
		}

		private void UpdateInherit()
		{
			if (InheritFrom.Visible)
			{
				var dataAdapter = new MySqlDataAdapter(@"
select cd.FirmCode, cd.ShortName as Name
from usersettings.clientsdata cd
where cd.firmcode = ?ClientCode
      or cd.firmcode in (select includeclientcode from includeregulation where primaryclientcode = ?ClientCode)
      or cd.firmcode in (select primaryclientcode from includeregulation where includeclientcode = ?ClientCode);", Literals.GetConnectionString());
				dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", IncludeSDD.SelectedValue);
				var data = new DataSet();
				dataAdapter.Fill(data);
				InheritFrom.DataSource = data;
				InheritFrom.DataBind();
				InheritClientProperties();
			}
		}

		protected void InheritFrom_SelectedIndexChanged(object sender, EventArgs e)
		{
			InheritClientProperties();
		}

		private void InheritClientProperties()
		{
			var dataAdapter = new MySqlDataAdapter(@"
select cd.ShortName, 
	   cd.FullName, 
	   cd.Adress,
	   (select c.ContactText
       from contacts.contact_groups cg
         join contacts.contacts c on cg.id = c.ContactOwnerId
       where c.`Type` = 1 and cg.ContactGroupOwnerId = cd.ContactGroupOwnerId
       limit 1) as Phone,
	   (select c.ContactText
       from contacts.contact_groups cg
         join contacts.contacts c on cg.id = c.ContactOwnerId
       where c.`Type` = 0 and cg.ContactGroupOwnerId = cd.ContactGroupOwnerId
       limit 1) as Email
from usersettings.clientsdata cd
where cd.firmcode = ?ClientCode
", Literals.GetConnectionString());
			dataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", InheritFrom.SelectedValue);
			var data = new DataSet();
			dataAdapter.Fill(data);
			ShortNameTB.Text = data.Tables[0].Rows[0]["ShortName"].ToString();
			FullNameTB.Text = data.Tables[0].Rows[0]["FullName"].ToString();
			AddressTB.Text = data.Tables[0].Rows[0]["Adress"].ToString();
			PhoneTB.Text = data.Tables[0].Rows[0]["Phone"].ToString();
			EmailTB.Text = data.Tables[0].Rows[0]["Email"].ToString();
		}

		protected void SearchSupplierClick(object sender, EventArgs e)
		{
			With.Connection(c => {
				var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  DISTINCT cd.FirmCode SupplierId,
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) SupplierName
FROM clientsdata as cd
WHERE   cd.regioncode & ?AdminRegionCode > 0 
        AND cd.firmstatus = 1 
        AND cd.billingstatus = 1 
		and cd.FirmType = 0
        AND cd.ShortName like ?SearchText  
		{0}
ORDER BY cd.shortname;", SecurityContext.Administrator.GetClientFilterByType("cd")), c);
				adapter.SelectCommand.Parameters.AddWithValue("?AdminRegionCode", SecurityContext.Administrator.RegionMask);
				adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", SupplierSearchText.Text));
				adapter.Fill(data, "Suppliers");
			});

			Suppliers.DataSource = data.Tables["Suppliers"];
			Suppliers.DataBind();
			if (Suppliers.Items.Count > 0)
			{
				Suppliers.Visible = true;
				SupplierSearchText.Visible = false;
				SearchSupplier.Visible = false;
			}
		}

		protected void CustomerTypeChanged(object sender, EventArgs e)
		{
			if (CustomerType.SelectedItem.Text == "Стандартный")
			{
				SupplierSearchText.Visible = false;
				Suppliers.Visible = false;
				SearchSupplier.Visible = false;
				return;
			}

			SupplierSearchText.Visible = true;
			SearchSupplier.Visible = true;
		}

		protected void ValidateSupplier(object source, ServerValidateEventArgs args)
		{
			if (CustomerType.SelectedItem.Text == "Стандартный")
				return;

			args.IsValid = Suppliers.SelectedItem != null;
		}
	}
}