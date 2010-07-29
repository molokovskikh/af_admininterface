using System;
using System.Collections.Generic;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using System.Linq;
using System.Web.Script.Serialization;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;

namespace AdminInterface.Controllers
{
	public class AdditionalSettings
	{
		public bool ShowRegistrationCard { get; set; }
		public bool FillBillingInfo { get; set; }
		public bool IsServiceClient { get; set; }
		public bool PayerExists { get; set; }
		public bool ShowForOneSupplier { get; set; }
		public bool SendRegistrationCard { get; set; }
		public bool IgnoreNewPrices { get; set; }
	}

	[
		Layout("GeneralWithJQuery"),
		Helper(typeof(BindingHelper)), 
		Helper(typeof(ViewHelper)),
		Secure(PermissionType.RegisterDrugstore, PermissionType.RegisterSupplier, Required = Required.AnyOf),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class RegisterController : SmartDispatcherController
	{
		private readonly NotificationService _notificationService = new NotificationService();

		[AccessibleThrough(Verb.Get)]
		public void Register()
		{
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			PropertyBag["regions"] = regions;
		}

		[AccessibleThrough(Verb.Post)]
		public void RegisterClient([DataBind("client")]Client client, 
			ulong homeRegion, 
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("permissions")] UserPermission[] permissions, 
			[DataBind("flags")] AdditionalSettings additionalSettings, 
			string deliveryAddress,
			[DataBind("payer")] Payer payer,
			uint? existingPayerId,
			[DataBind("supplier")] Supplier supplier,
			[DataBind("clientContacts")] Contact[] clientContacts,
			string userName,
			[DataBind("userContacts")] Contact[] userContacts,
			[DataBind("userPersons")] Person[] userPersons,
			string additionalEmailsForSendingCard)
		{
			ulong browseRegionMask = 0;
			ulong orderRegionMask = 0;
			User newUser = null;
			Client newClient = null;
			var password = String.Empty;
			var trimSymbols = new[] {' '};

			if (!String.IsNullOrEmpty(userName))
				userName = userName.Replace("№", "N").Trim(trimSymbols);
			if (!String.IsNullOrEmpty(deliveryAddress))
				deliveryAddress = deliveryAddress.Replace("№", "N").Trim(trimSymbols);

			foreach (var region in regionSettings)
			{
				if (region.IsAvaliableForBrowse)
					browseRegionMask |= region.Id;
				if (region.IsAvaliableForOrder)
					orderRegionMask |= region.Id;
			}

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession(s => {
					var connection = (MySqlConnection) s.Connection;
					var command = new MySqlCommand("", connection);
					DbLogHelper.SetupParametersForTriggerLogging();

					Payer currentPayer = null;
					if (additionalSettings.PayerExists)
					{
						if (payer != null || existingPayerId.HasValue)
						{
							var id = existingPayerId.HasValue ? existingPayerId.Value : payer.PayerID;
							currentPayer = Payer.Find(id);
						}
					}

					newClient = new Client {
						Status = ClientStatus.On,
						Type = client.Type,
						FullName = client.FullName.Replace("№", "N").Trim(trimSymbols),
						Name = client.Name.Replace("№", "N").Trim(trimSymbols),
						HomeRegion = Region.Find(homeRegion),
						Segment = client.Segment,
						MaskRegion = browseRegionMask,
						Registrant = SecurityContext.Administrator.UserName,
						RegistrationDate = DateTime.Now,
					};
					if (currentPayer == null)
						currentPayer = CreatePayer(newClient);
					newClient.BillingInstance = currentPayer;
					newClient.AddDeliveryAddress(deliveryAddress);
					newClient.SaveAndFlush();

					AddContactsToClient(newClient, clientContacts);

					newUser = CreateUser(newClient, userName, permissions, browseRegionMask, orderRegionMask, userPersons);
					password = newUser.CreateInAd();
					var defaults = DefaultValues.Get();
					if (newClient.IsDrugstore())
						CreateDrugstore(command, newClient, additionalSettings, orderRegionMask, supplier);
					else
						CreateSupplier(command, defaults, newClient);

					if (newClient.IsDrugstore() && newClient.Addresses.Count > 0)
					{
						if (newUser.AvaliableAddresses == null)
							newUser.AvaliableAddresses = new List<Address>();
						newUser.AvaliableAddresses.Add(newClient.Addresses.Last());
					}
					newClient.Addresses.Each(a => a.CreateFtpDirectory());
				});
				scope.VoteCommit();
			}
			newUser.UpdateContacts(userContacts);

			Mailer.ClientRegistred(newClient, false);
			Session["DogN"] = newClient.BillingInstance.PayerID;
			Session["Code"] = newClient.Id;
			Session["Name"] = newClient.FullName;
			Session["ShortName"] = newClient.Name;
			Session["Login"] = newUser.Login;
			Session["Password"] = password;
			Session["Tariff"] = newClient.Type.GetDescription();
			Session["Register"] = true;

			var log = new PasswordChangeLogEntity(newUser.Login);
			if (additionalSettings.SendRegistrationCard)
				log = SendRegistrationCard(log, newClient, newUser, password, additionalEmailsForSendingCard);
			log.Save();

			var sendBillingNotificationNow = true;
			string redirectTo;
			var virtualDir = Context.UrlInfo.AppVirtualDir;
			if (!virtualDir.StartsWith("/"))
				virtualDir = "/" + virtualDir;
			if (virtualDir.EndsWith("/"))
				virtualDir = virtualDir.Remove(virtualDir.Length - 1, 1);
			if (additionalSettings.FillBillingInfo)
			{
				sendBillingNotificationNow = false;
				redirectTo = String.Format(virtualDir + "/Register/RegisterPayer.rails?id={0}&clientCode={2}&showRegistrationCard={1}",
					newClient.BillingInstance.PayerID,
					additionalSettings.ShowRegistrationCard,
					newClient.Id);
			}
			else if (additionalSettings.ShowRegistrationCard)
				redirectTo = virtualDir  + "/report.aspx";
			else
				redirectTo = String.Format(virtualDir + "/Client/{0}", newClient.Id);

			if (sendBillingNotificationNow)
				new NotificationService()
					.SendNotificationToBillingAboutClientRegistration(newClient,
						SecurityContext.Administrator.UserName,
						null, NotificationHelper.GetApplicationUrl());

			Flash["Message"] = Message.Notify("Регистрация завершена успешно");
			RedirectToUrl(redirectTo);
		}

		private PasswordChangeLogEntity SendRegistrationCard(PasswordChangeLogEntity log, Client client, User user, string password, string additionalEmails)
		{
			var mailTo = client.GetAddressForSendingClientCard();

			var smtpid = ReportHelper.SendClientCard(client,
				user.Login,
				password,
				mailTo,
				additionalEmails,
				true);

			log.SetSentTo(smtpid, EmailHelper.JoinMails(mailTo, user.GetEmails(), additionalEmails));
			return log;
		}

		private Payer CreatePayer(Client client)
		{
			var prefix = String.Empty;
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

			var organization = new JuridicalOrganization();
			organization.Payer = payer;
			organization.Name = client.Name;
			organization.FullName = client.FullName;
			payer.JuridicalOrganizations = new List<JuridicalOrganization> {organization};
			organization.Save();

			return payer;
		}

		private void CreateDrugstore(MySqlCommand command, Client client, AdditionalSettings additionalSettings, ulong orderMask, Supplier supplier)
		{
			command.CommandText = "select usersettings.GeneratePassword()";
			var costCrypKey = command.ExecuteScalar().ToString();

			client.Settings = new DrugstoreSettings {
				Id = client.Id,
				InvisibleOnFirm =
					(Convert.ToUInt32(additionalSettings.ShowForOneSupplier) > 0) ? DrugstoreType.Hidden : DrugstoreType.Standart,
				WorkRegionMask = client.MaskRegion,
				OrderRegionMask = orderMask,
				ServiceClient = additionalSettings.IsServiceClient,
				BasecostPassword = costCrypKey,
				IgnoreNewPrices = additionalSettings.IgnoreNewPrices,
				ParseWaybills = true,
				ShowAdvertising = true,
				ShowNewDefecture = true,
			};
			if (additionalSettings.ShowForOneSupplier)
				client.Settings.FirmCodeOnly = supplier.Id;
			client.Settings.CreateAndFlush();

			client.MaintainIntersection();

			if (client.Settings.InvisibleOnFirm == DrugstoreType.Standart)
			{
				command.CommandText = "insert into inscribe(ClientCode) values(?ClientCode); ";
				command.Parameters.AddWithValue("?ClientCode", client.Id);
				command.ExecuteNonQuery();
			}
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

		private User CreateUser(Client client, string userName, UserPermission[] permissions,
			ulong workRegionMask, ulong orderRegionMask, Person[] persons)
		{
			var user = new User
			{
				Enabled = true,
				Client = client,
				Name = userName,
				WorkRegionMask = workRegionMask,
				OrderRegionMask = orderRegionMask,
				SendRejects = true,
				SendWaybills = true,
			};
			if (permissions != null && permissions.Count() > 0)
			{
				user.AssignedPermissions = permissions.Select(i => UserPermission.Find(i.Id))
					.Concat(UserPermission.GetDefaultPermissions()).Distinct().ToList();
			}
			user.Setup();
			client.Users = new List<User> {user};
			foreach (var person in persons)
				user.AddContactPerson(person.Name);
			user.SaveAndFlush();
			return user;
		}		

		private void AddContactsToClient(Client client, Contact[] clientContacts)
		{
			var owner = new ContactGroupOwner();
			client.ContactGroupOwner = owner;

			var generalGroup = owner.AddContactGroup(ContactGroupType.General);
			foreach (var contact in clientContacts)
				generalGroup.AddContact(contact);
			owner.Save();
		}

		public void RegisterPayer(uint id, uint clientCode, bool showRegistrationCard)
		{
			var instance = Payer.Find(id);
			PropertyBag["Instance"] = instance;
			PropertyBag["showRegistrationCard"] = showRegistrationCard;
			PropertyBag["clientCode"] = clientCode;
			PropertyBag["PaymentOptions"] = new PaymentOptions();
			PropertyBag["admin"] = SecurityContext.Administrator;

			var client = Client.Find(clientCode);
			PropertyBag["JuridicalOrganization"] = client.Addresses[0].JuridicalOrganization;
		}

		public void Registered([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer payer,
			[DataBind("JuridicalOrganization")] JuridicalOrganization juridicalOrganization,
			[DataBind("PaymentOptions")] PaymentOptions paymentOptions,
			uint clientCode,
			bool showRegistrationCard)
		{
			var client = Client.Find(clientCode);

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				if (String.IsNullOrEmpty(payer.Comment))
					payer.Comment = paymentOptions.GetCommentForPayer();
				else
					payer.Comment += "\r\n" + paymentOptions.GetCommentForPayer();
				payer.UpdateAndFlush();

				juridicalOrganization.Payer = payer;
				juridicalOrganization.UpdateAndFlush();

				if (client.Addresses.Count > 0)
				{
					client.Addresses[0].JuridicalOrganization = juridicalOrganization;
					client.Addresses[0].UpdateAndFlush();
				}

				scope.VoteCommit();
			}

			_notificationService.SendNotificationToBillingAboutClientRegistration(client,
				SecurityContext.Administrator.UserName,
				paymentOptions, NotificationHelper.GetApplicationUrl());

			var redirectUrl = String.Empty;
			if (showRegistrationCard)
				redirectUrl = "/report.aspx";
			else
				redirectUrl = String.Format("/client/{0}", clientCode);
			RedirectToUrl(redirectUrl);
		}

		public void SearchPayers(string searchPattern)
		{
			if (String.IsNullOrEmpty(searchPattern))
				return;
			PropertyBag["payers"] = Payer.GetLikeAvaliable(searchPattern);
			CancelLayout();
		}

		public void SearchSuppliers(string searchPattern, uint? payerId)
		{
			if (String.IsNullOrEmpty(searchPattern))
				return;
			var suppliers = Supplier.FindAll()
					.Where(item => (item.Status == ClientStatus.On) && item.Name.ToLower().Contains(searchPattern.ToLower()))
					.Take(50);
			var allowViewSuppliers = SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers);
			if (!allowViewSuppliers)
				return;
			if (payerId.HasValue && (payerId.Value > 0))
				suppliers = suppliers.Where(item => item.BillingInstance.PayerID == payerId.Value);
			PropertyBag["suppliers"] = suppliers;
			CancelLayout();
		}
	}
}
