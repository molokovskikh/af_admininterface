using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using System.Linq;
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
			string additionalEmailsForSendingCard,
			string comment)
		{
			ulong browseRegionMask = 0;
			ulong orderRegionMask = 0;
			User newUser;
			Client newClient;
			string password;
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
				DbLogHelper.SetupParametersForTriggerLogging();
				Payer currentPayer = null;

				if (additionalSettings.PayerExists)
				{
					if (payer != null || existingPayerId.HasValue)
					{
						var id = existingPayerId.HasValue ? existingPayerId.Value : payer.PayerID;
						currentPayer = Payer.Find(id);
						if (currentPayer.JuridicalOrganizations.Count == 0)
						{
							var organization = new LegalEntity();
							organization.Payer = currentPayer;
							organization.Name = currentPayer.Name;
							organization.FullName = currentPayer.JuridicalName;
							currentPayer.JuridicalOrganizations = new List<LegalEntity> {organization};
							organization.Save();
						}
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
				newClient.JoinPayer(currentPayer);
				newClient.AddAddress(deliveryAddress);
				newClient.SaveAndFlush();

				AddContactsToClient(newClient, clientContacts);

				newUser = CreateUser(newClient, userName, permissions, browseRegionMask, orderRegionMask, userPersons);
				password = newUser.CreateInAd();
				var defaults = DefaultValues.Get();
				if (newClient.IsDrugstore())
					CreateDrugstore(newClient, additionalSettings, orderRegionMask, supplier);
				else
					CreateSupplier(defaults, newClient);

				if (newClient.IsDrugstore() && newClient.Addresses.Count > 0)
				{
					if (newUser.AvaliableAddresses == null)
						newUser.AvaliableAddresses = new List<Address>();
					newUser.AvaliableAddresses.Add(newClient.Addresses.Last());
				}
				newClient.Addresses.Each(a => a.CreateFtpDirectory());

				newClient.AddComment(comment);

				scope.VoteCommit();
			}
			newUser.UpdateContacts(userContacts);

			Mailer.ClientRegistred(newClient, false);
			Session["DogN"] = newClient.Payers.Single().Id;
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
			if (additionalSettings.FillBillingInfo)
			{
				sendBillingNotificationNow = false;
				redirectTo = String.Format("/Register/RegisterPayer.rails?id={0}&clientCode={2}&showRegistrationCard={1}",
					newClient.Payers.Single().Id,
					additionalSettings.ShowRegistrationCard,
					newClient.Id);
			}
			else if (additionalSettings.ShowRegistrationCard)
				redirectTo = "/report.aspx";
			else
				redirectTo = String.Format("/Client/{0}", newClient.Id);
			redirectTo = LinkHelper.GetVirtualDir(Context) + redirectTo;

			if (sendBillingNotificationNow)
				this.Mail().NotifyBillingAboutClientRegistration(newClient);

			Flash["Message"] = Message.Notify("Регистрация завершена успешно");
			RedirectToUrl(redirectTo);
		}

		private PasswordChangeLogEntity SendRegistrationCard(PasswordChangeLogEntity log, Client client, User user, string password, string additionalEmails)
		{
			var mailTo = client.GetAddressForSendingClientCard();

			var smtpid = ReportHelper.SendClientCard(user,
				password,
				true,
				mailTo,
				additionalEmails);

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
				Name = client.Name,
				JuridicalName = client.FullName,
				BeforeNamePrefix = prefix,
				ContactGroupOwner = contactGroupOwner,
			};
			payer.Save();

			var organization = new LegalEntity();
			organization.Payer = payer;
			organization.Name = client.Name;
			organization.FullName = client.FullName;
			payer.JuridicalOrganizations = new List<LegalEntity> {organization};
			organization.Save();

			return payer;
		}

		private void CreateDrugstore(Client client, AdditionalSettings additionalSettings, ulong orderMask, Supplier supplier)
		{
			var costCrypKey = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select usersettings.GeneratePassword()")
				.UniqueResult<string>());

			var smartOrder = SmartOrderRules.TestSmartOrder();

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
				EnableSmartOrder = true,
				SmartOrderRules = smartOrder
			};
			
			
			if (additionalSettings.ShowForOneSupplier)
				client.Settings.NoiseCostExceptSupplier = supplier;
			client.Settings.CreateAndFlush();

			client.MaintainIntersection();
			client.Addresses.Each(a => a.MaintainInscribe());
		}

		public void CreateSupplier(DefaultValues defaults, Client client)
		{
			var orderSendRules = new OrderSendRules(defaults, client.Id);
			orderSendRules.Save();

/*			command.CommandText = @"
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
			command.ExecuteNonQuery();*/
		}

		private User CreateUser(Client client, string userName, UserPermission[] permissions,
			ulong workRegionMask, ulong orderRegionMask, Person[] persons)
		{
			var user = new User(client) {
				Name = userName,
				WorkRegionMask = workRegionMask,
				OrderRegionMask = orderRegionMask,
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
				if (contact.ContactText!=null) generalGroup.AddContact(contact);
			owner.Save();
		}

		public void RegisterPayer(uint id, uint clientCode, bool showRegistrationCard)
		{
			var payer = Payer.TryFind(id);
			if (payer == null)
			{
				payer = new Payer {
					JuridicalOrganizations = new List<LegalEntity> {
						new LegalEntity()
					}
				};
			}

			PropertyBag["Instance"] = payer;
			PropertyBag["payer"] = payer;
			PropertyBag["JuridicalOrganization"] = payer.JuridicalOrganizations.First();
			PropertyBag["showRegistrationCard"] = showRegistrationCard;
			PropertyBag["clientCode"] = clientCode;
			PropertyBag["PaymentOptions"] = new PaymentOptions();
			PropertyBag["admin"] = SecurityContext.Administrator;
		}

		public void Registered([ARDataBind("Instance", AutoLoadBehavior.NewRootInstanceIfInvalidKey)] Payer payer,
			[DataBind("JuridicalOrganization")] LegalEntity juridicalOrganization,
			[DataBind("PaymentOptions")] PaymentOptions paymentOptions,
			uint clientCode,
			bool showRegistrationCard)
		{
			Client client;
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				payer.ContactGroupOwner = new ContactGroupOwner();
				payer.ContactGroupOwner.Save();
				payer.AddComment(paymentOptions.GetCommentForPayer());
				payer.Name = juridicalOrganization.Name;
				payer.JuridicalName = juridicalOrganization.FullName;
				payer.Save();

				if (String.IsNullOrEmpty(juridicalOrganization.Name))
				{
					payer.JuridicalOrganizations = new List<LegalEntity> {
						juridicalOrganization
					};
				}
				juridicalOrganization.Payer = payer;
				juridicalOrganization.Save();

				client = Client.TryFind(clientCode);
				if (client != null)
				{
					if (client.Addresses.Count > 0)
					{
						client.Addresses[0].LegalEntity = juridicalOrganization;
						client.Addresses[0].UpdateAndFlush();
					}
				}

				scope.VoteCommit();
			}

			if (client != null)
				this.Mail().NotifyBillingAboutClientRegistration(client);

			string redirectUrl;
			if (showRegistrationCard)
				redirectUrl = "~/report.aspx";
			else if (client != null)
				redirectUrl = String.Format("~/Client/{0}", client.Id);
			else
				redirectUrl = String.Format("~/Payers/{0}", payer.Id);

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
			var allowViewSuppliers = SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers);
			if (!allowViewSuppliers)
				return;
			var suppliers = Supplier.Queryable
					.Where(s => (s.Status == ClientStatus.On) && s.Name.Contains(searchPattern))
					.Take(50);
			if (payerId.HasValue && (payerId.Value > 0))
				suppliers = suppliers.Where(item => item.Payer.PayerID == payerId.Value);
			PropertyBag["suppliers"] = suppliers;
			CancelLayout();
		}
	}
}
