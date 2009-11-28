using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Common.Tools;
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

			var maskRegion = (ulong) WRList.Items.Cast<ListItem>().Sum(i => Convert.ToInt64(i.Value));
			var orderMask = (ulong) OrderList.Items.Cast<ListItem>().Sum(i => Convert.ToInt64(i.Value));;

			Client client = null;
			User user = null;
			string password = null;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession(s => {
					var connection = (MySqlConnection) s.Connection;
					var command = new MySqlCommand("", connection);
					DbLogHelper.SetupParametersForTriggerLogging<Client>(SecurityContext.Administrator.UserName, HttpContext.Current.Request.UserHostAddress);

					client = new Client {
						Status = ClientStatus.On,
						Type = (ClientType) Convert.ToInt32(TypeDD.SelectedItem.Value),
						Name = ShortNameTB.Text.Replace("№", "N"),
						FullName = FullNameTB.Text,
						Segment = (Segment) Convert.ToInt32(SegmentDD.SelectedItem.Value),
						HomeRegion = Region.Find(Convert.ToUInt64(RegionDD.SelectedItem.Value)),
						MaskRegion = maskRegion,
						Registrant = SecurityContext.Administrator.UserName,
						RegistrationDate = DateTime.Now
					};

					client.AddDeliveryAddress(AddressTB.Text);

					Payer payer;
					if (!PayerPresentCB.Checked || (PayerPresentCB.Checked && PayerDDL.SelectedItem == null))
						payer = CreatePayer(client);
					else
						payer = Payer.Find(Convert.ToUInt32(PayerDDL.SelectedItem.Value));

					client.BillingInstance = payer;

					CreateClient(client);
					client.Save();

					var defaults = DefaultValues.Get();

					if (client.IsDrugstore())
						CreateDrugstore(command, client, orderMask);
					else
						CreateSupplier(command, defaults, client);

					user = CreateUser(client);
					password = user.CreateInAd();

					client.Addresses.Each(a => a.CreateFtpDirectory());
				});
				scope.VoteCommit();
			}

			if (TypeDD.SelectedItem.Text == "Аптека"
				&& !ServiceClient.Checked
				&& CustomerType.SelectedItem.Text == "Стандартный")
				new NotificationService().NotifySupplierAboutDrugstoreRegistration(client);

			if (!client.IsDrugstore())
				Mailer.SupplierRegistred(client.Name, client.HomeRegion.Name);

			NotificationHelper.NotifyAboutRegistration(
				String.Format("\"{0}\" - успешная регистрация", FullNameTB.Text),
				String.Format("Оператор: {0}\nРегион: {1}\nИмя пользователя: {2}\nКод: {3}\n\nСегмент: {4}\nТип: {5}",
					SecurityContext.Administrator.UserName,
					client.HomeRegion.Name,
					user.Login,
					client.Id,
					SegmentDD.SelectedItem.Text,
					TypeDD.SelectedItem.Text));

			Session["DogN"] = client.BillingInstance.PayerID;
			Session["Code"] = client.Id;
			Session["Name"] = client.FullName;
			Session["ShortName"] = client.Name;
			Session["Login"] = user.Login;
			Session["Password"] = password;
			Session["Tariff"] = TypeDD.SelectedItem.Text;
			Session["Register"] = true;

			var log = SendClientCardIfNeeded(client, user.Login, password);
			log.Save();

			var sendBillingNotificationNow = true;
			string redirectTo;
			if (EnterBillingInfo.Checked)
			{
				sendBillingNotificationNow = false;
				redirectTo = String.Format("Register/Register.rails?id={0}&clientCode={2}&showRegistrationCard={1}",
					client.BillingInstance.PayerID,
					ShowRegistrationCard.Checked,
					client.Id);
			}
			else if (ShowRegistrationCard.Checked)
				redirectTo = "report.aspx";
			else
				redirectTo = String.Format("Client/{0}", client.Id);


			if (sendBillingNotificationNow)
				new NotificationService()
					.SendNotificationToBillingAboutClientRegistration(client,
						SecurityContext.Administrator.UserName,
						null);
			Response.Redirect(redirectTo);
		}

		private User CreateUser(Client client)
		{
			var user = new User {
				Client = client, 
				Name = LoginTB.Text.Trim(),
			};

			if (PermissionsDiv.Visible)
			{
				user.AssignedPermissions = Permissions.Items.Cast<ListItem>()
					.Where(i => i.Selected)
					.Select(i => UserPermission.Find(Convert.ToUInt32(i.Value))).ToList();
			}

			user.Setup(false);
			client.Users = new List<User> {user};
			return user;
		}

		private PasswordChangeLogEntity SendClientCardIfNeeded(Client client,
			string username,
			string password)
		{
			var log = new PasswordChangeLogEntity(username);
			if (!SendRegistrationCard.Checked)
				return log;

			var mailTo = client.GetAddressForSendingClientCard();

			var smtpid = ReportHelper.SendClientCard(client,
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
FROM Future.Clients as cd
	JOIN billing.payers p ON cd.PayerId = p.PayerId
WHERE   cd.regioncode & ?AdminRegionCode > 0 
        AND Status = 1 
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

		private Payer CreatePayer(Client client)
		{
			var prefix = "";
			if (client.IsDrugstore())
				prefix = "Аптека";

			var contactGroupOwner = new ContactGroupOwner();
			var group = contactGroupOwner.AddContactGroup(ContactGroupType.Billing);
			contactGroupOwner.Save();
			group.Save();

			var payer = new Payer {
				OldTariff = 0,
				OldPayDate = DateTime.Now,
				Comment = String.Format("Дата регистрации: {0}", DateTime.Now),
				ShortName = client.Name,
				JuridicalName = client.FullName,
				BeforeNamePrefix = prefix,
				ContactGroupOwner = contactGroupOwner,
			};
			payer.Save();
			return payer;
		}

		private void CreateClient(Client client)
		{
			var owner = new ContactGroupOwner();
			client.ContactGroupOwner = owner;

			var generalGroup = owner.AddContactGroup(ContactGroupType.General);
			if (!String.IsNullOrEmpty(PhoneTB.Text) && !String.IsNullOrEmpty(PhoneTB.Text.Trim()))
				generalGroup.AddContact(ContactType.Phone, PhoneTB.Text);
			if (!String.IsNullOrEmpty(EmailTB.Text) && !String.IsNullOrEmpty(EmailTB.Text.Trim()))
				generalGroup.AddContact(ContactType.Email, EmailTB.Text);

			var orderGroup = owner.AddContactGroup(ContactGroupType.OrderManagers);
			if (!String.IsNullOrEmpty(TBOrderManagerPhone.Text) && !String.IsNullOrEmpty(TBOrderManagerPhone.Text.Trim()))
				orderGroup.AddContact(ContactType.Phone, TBOrderManagerPhone.Text);
			if (!String.IsNullOrEmpty(TBOrderManagerMail.Text) && !String.IsNullOrEmpty(TBOrderManagerMail.Text.Trim()))
				orderGroup.AddContact(ContactType.Email, TBOrderManagerMail.Text);
			if (!String.IsNullOrEmpty(TBOrderManagerName.Text) && !String.IsNullOrEmpty(TBOrderManagerName.Text.Trim()))
				orderGroup.AddPerson(TBOrderManagerName.Text);

			if (!client.IsDrugstore())
			{
				var clientsGroup = owner.AddContactGroup(ContactGroupType.ClientManagers);
				if (!String.IsNullOrEmpty(TBClientManagerPhone.Text) && !String.IsNullOrEmpty(TBClientManagerPhone.Text.Trim()))
					clientsGroup.AddContact(ContactType.Phone, TBClientManagerPhone.Text);
				if (!String.IsNullOrEmpty(TBClientManagerPhone.Text) && !String.IsNullOrEmpty(TBClientManagerPhone.Text.Trim()))
					clientsGroup.AddContact(ContactType.Email, TBClientManagerPhone.Text);
				if (!String.IsNullOrEmpty(TBClientManagerMail.Text) && !String.IsNullOrEmpty(TBClientManagerMail.Text.Trim()))
					clientsGroup.AddPerson(TBClientManagerMail.Text);
			}
			owner.Save();
		}

		public void CreateSupplier(MySqlCommand command, DefaultValues defaults, Client client)
		{
			var orderSendRules = new OrderSendRules(defaults, client.Id);
			orderSendRules.Save();

			command.CommandText = @"
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
			command.Parameters.AddWithValue("?ClientCode", client.Id);
			command.ExecuteNonQuery();
		}

		private void CreateDrugstore(MySqlCommand command, Client client, ulong orderMask)
		{
			command.CommandText = "select usersettings.GeneratePassword()";
			var costCrypKey = command.ExecuteScalar().ToString();

			var settings = new DrugstoreSettings {
				Id = client.Id,
				InvisibleOnFirm = (DrugstoreType) Convert.ToUInt32(CustomerType.SelectedItem.Value),
				OrderRegionMask = orderMask,
				ServiceClient = ServiceClient.Checked,
				BasecostPassword = costCrypKey,
			};
			if (!String.IsNullOrEmpty(Suppliers.SelectedValue))
				settings.FirmCodeOnly = Convert.ToUInt32(Suppliers.SelectedValue);

			settings.CreateAndFlush();
			client.MaintainIntersection();
			command.CommandText = @"
insert into usersettings.ret_save_grids(ClientCode, SaveGridId)
select ?ClientCode, sg.id
from usersettings.save_grids sg
where sg.AssignDefaultValue = 1;";
			command.Parameters.AddWithValue("?ClientCode", client.Id);

			if (settings.InvisibleOnFirm == DrugstoreType.Standart)
				command.CommandText += " insert into inscribe(ClientCode) values(?ClientCode); ";

			command.ExecuteNonQuery();
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

		protected void TypeValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = args.Value == "Поставщик";
		}

		protected void ClientTypeChanged(object sender, EventArgs e)
		{
			var isCusomer = TypeDD.SelectedItem.Text == "Аптека";
			CustomerType.Enabled = isCusomer;
			ServiceClient.Enabled = isCusomer;
			UpdatePermissions();
			if (!isCusomer)
			{
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

		protected void SearchSupplierClick(object sender, EventArgs e)
		{
			With.Connection(c => {
				var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  DISTINCT cd.FirmCode SupplierId,
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) SupplierName
FROM clientsdata as cd
WHERE   cd.regioncode & ?AdminRegionCode > 0 
        AND cd.firmstatus = 1 
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