using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

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
		Helper(typeof(BindingHelper)), 
		Helper(typeof(ViewHelper)),
		Secure(PermissionType.RegisterDrugstore, PermissionType.RegisterSupplier, Required = Required.AnyOf),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class RegisterController : AdminInterfaceController
	{
		[AccessibleThrough(Verb.Get)]
		public void RegisterSupplier()
		{
			PropertyBag["supplier"] = new Supplier();
			PropertyBag["regions"] = Region.All().ToArray();
			PropertyBag["SingleRegions"] = true;
		}

		[AccessibleThrough(Verb.Post)]
		public void RegisterSupplier(
			[DataBind("supplier")] Supplier supplier,
			[DataBind("supplierContacts")] Contact[] supplierContacts,
			ulong homeRegion, 
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("flags")] AdditionalSettings additionalSettings, 

			[DataBind("payer")] Payer payer,
			uint? existingPayerId,

			string userName,
			[DataBind("userContacts")] Contact[] userContacts,
			[DataBind("userPersons")] Person[] userPersons,

			string additionalEmailsForSendingCard,
			string comment)
		{
			User user;
			string password;

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

				supplier.RegionMask = regionSettings.GetBrowseMask();
				supplier.HomeRegion = Region.Find(homeRegion);
				supplier.ContactGroupOwner = new ContactGroupOwner(supplier.GetAditionalContactGroups());
				supplier.Registration = new RegistrationInfo(Admin);
				if (currentPayer == null)
				{
					currentPayer = new Payer(supplier.Name, supplier.FullName);
					currentPayer.Save();
				}

				supplier.Payer = currentPayer;
				AddContacts(supplier.ContactGroupOwner, supplierContacts);
				supplier.Save();

				foreach (var group in supplier.ContactGroupOwner.ContactGroups)
				{
					var persons = BindObject<List<Person>>(group.Type + "Persons");
					var contacts = BindObject<List<Contact>>(group.Type + "Contacts");

					group.Persons = persons;
					group.Contacts = contacts;
				}

				var groups = BindObject<RegionalDeliveryGroup[]>("orderDeliveryGroup");

				foreach (var deliveryGroup in groups)
				{
					deliveryGroup.Region = Region.Find(deliveryGroup.Region.Id);
					deliveryGroup.Name = "Доставка заказов " + deliveryGroup.Region.Name;
					deliveryGroup.ContactGroupOwner = supplier.ContactGroupOwner;
					supplier.ContactGroupOwner.ContactGroups.Add(deliveryGroup);
				}

				foreach (var group in supplier.ContactGroupOwner.ContactGroups)
				{
					group.Adopt();
					group.Save();
					group.Persons.Each(p => p.Save());
				}

				scope.Flush();

				CreateSupplier(DefaultValues.Get(), supplier);
				Maintainer.MaintainIntersection(supplier);

				user = new User(supplier.Payer, supplier) {
					Name = userName,
				};

				user.UpdateContacts(userContacts);
				foreach (var person in userPersons)
					user.AddContactPerson(person.Name);
				user.Setup();
				user.SaveAndFlush();
				password = user.CreateInAd();

				supplier.AddComment(comment);

				scope.VoteCommit();
			}

			Mailer.SupplierRegistred(supplier);
			supplier.CreateDirs();

			var log = new PasswordChangeLogEntity(user.Login);
			if (additionalSettings.SendRegistrationCard)
				log = SendRegistrationCard(log, user, password, additionalEmailsForSendingCard);
			log.Save();

			if (additionalSettings.FillBillingInfo)
			{
				Session["password"] = password;
				Redirect("Register", "RegisterPayer", new {
					id = supplier.Payer.Id,
					showRegistrationCard = additionalSettings.ShowRegistrationCard
				});
			}
			else if (supplier.Users.Count > 0 && additionalSettings.ShowRegistrationCard)
			{
				Flash["password"] = password;
				Redirect("main", "report", new{id = supplier.Users.First().Id});
			}
			else
			{
				Flash["Message"] = Message.Notify("Регистрация завершена успешно");
				Redirect("Suppliers", "Show", new{id = supplier.Id});
			}
		}


		[AccessibleThrough(Verb.Get)]
		public void RegisterClient()
		{
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["regions"] = Region.All().ToArray();
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
			User newUser;
			Client newClient;
			string password;

			if (!String.IsNullOrEmpty(userName))
				userName = userName.Replace("№", "N").Trim();
			if (!String.IsNullOrEmpty(deliveryAddress))
				deliveryAddress = deliveryAddress.Replace("№", "N").Trim();

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
					FullName = client.FullName.Replace("№", "N").Trim(),
					Name = client.Name.Replace("№", "N").Trim(),
					HomeRegion = Region.Find(homeRegion),
					Segment = client.Segment,
					MaskRegion = regionSettings.GetBrowseMask(),
					Registration = new RegistrationInfo(Admin),
					ContactGroupOwner = new ContactGroupOwner()
				};
				if (currentPayer == null)
				{
					currentPayer = new Payer(newClient.Name, newClient.FullName);
					currentPayer.BeforeNamePrefix = "Аптека";
					currentPayer.Save();
				}
				newClient.JoinPayer(currentPayer);
				if (!String.IsNullOrWhiteSpace(deliveryAddress))
					newClient.AddAddress(deliveryAddress);

				CreateDrugstore(newClient, additionalSettings, regionSettings.GetOrderMask(), supplier);
				AddContacts(newClient.ContactGroupOwner, clientContacts);

				newUser = CreateUser(newClient, userName, permissions, userPersons);
				password = newUser.CreateInAd();
				if (newClient.Addresses.Count > 0)
					newUser.AvaliableAddresses.Add(newClient.Addresses.Last());

				newClient.Addresses.Each(a => a.CreateFtpDirectory());
				newClient.AddComment(comment);

				scope.VoteCommit();
			}
			newUser.UpdateContacts(userContacts);

			Mailer.ClientRegistred(newClient, false);

			var log = new PasswordChangeLogEntity(newUser.Login);
			if (additionalSettings.SendRegistrationCard)
				log = SendRegistrationCard(log, newUser, password, additionalEmailsForSendingCard);
			log.Save();

			if (!additionalSettings.FillBillingInfo)
				this.Mailer().NotifyBillingAboutClientRegistration(newClient);

			if (additionalSettings.FillBillingInfo)
			{
				Session["password"] = password;
				Redirect("Register", "RegisterPayer", new {
					id = newClient.Payers.Single().Id,
					showRegistrationCard = additionalSettings.ShowRegistrationCard
				});
			}
			else if (newClient.Users.Count > 0 && additionalSettings.ShowRegistrationCard)
			{
				Flash["password"] = password;
				Redirect("main", "report", new{id = newClient.Users.First().Id});
			}
			else
			{
				Flash["Message"] = Message.Notify("Регистрация завершена успешно");
				Redirect(newClient);
			}
		}

		private PasswordChangeLogEntity SendRegistrationCard(PasswordChangeLogEntity log, User user, string password, string additionalEmails)
		{
			var mailTo = EmailHelper.JoinMails(user.GetAddressForSendingClientCard(), user.GetEmails(), additionalEmails);
			var smtpid = ReportHelper.SendClientCard(user,
				password,
				true,
				mailTo,
				additionalEmails);
			log.SetSentTo(smtpid, mailTo);
			return log;
		}

		private void CreateDrugstore(Client client, AdditionalSettings additionalSettings, ulong orderMask, Supplier supplier)
		{
			var costCrypKey = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select usersettings.GeneratePassword()")
				.UniqueResult<string>());

			var smartOrder = SmartOrderRules.TestSmartOrder();

			client.Settings = new DrugstoreSettings(client) {
				Id = client.Id,
				InvisibleOnFirm =
					(Convert.ToUInt32(additionalSettings.ShowForOneSupplier) > 0) ? DrugstoreType.Hidden : DrugstoreType.Standart,
				WorkRegionMask = client.MaskRegion,
				OrderRegionMask = orderMask,
				ServiceClient = additionalSettings.IsServiceClient,
				BasecostPassword = costCrypKey,
				IgnoreNewPrices = additionalSettings.IgnoreNewPrices,
				SmartOrderRules = smartOrder
			};

			if (additionalSettings.ShowForOneSupplier)
			{
				client.Settings.NoiseCosts = true;
				client.Settings.NoiseCostExceptSupplier = Supplier.Find(supplier.Id);
			}
			client.SaveAndFlush();

			client.MaintainIntersection();
			client.Addresses.Each(a => a.MaintainInscribe());
		}

		private void CreateSupplier(DefaultValues defaults, Supplier supplier)
		{
			var orderSendRules = new OrderSendRules(defaults, supplier);
			orderSendRules.Save();

			var command = @"
INSERT INTO pricesdata(Firmcode, PriceCode) VALUES(:ClientCode, null);
SET @NewPriceCode = Last_Insert_ID(); 

INSERT INTO farm.formrules() VALUES();
SET @NewFormRulesId = Last_Insert_ID();
INSERT INTO farm.sources() VALUES(); 
SET @NewSourceId = Last_Insert_ID();

INSERT INTO usersettings.PriceItems(FormRuleId, SourceId) VALUES(@NewFormRulesId, @NewSourceId);
SET @NewPriceItemId = Last_Insert_ID();

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT @NewPriceCode, 1, @NewPriceItemId;
SET @NewPriceCostId = Last_Insert_ID(); 

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
		AND clientsdata.firmcode = :ClientCode  
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
		AND clientsdata.firmcode = :ClientCode  
		AND (clientsdata.maskregion & regions.regioncode)>0  
		AND pricesregionaldata.pricecode is null;";

			if (supplier.Segment == Segment.Retail)
			{
				command += @"
INSERT
INTO intersection
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
		FROM pricescosts pcc
		WHERE basecost
			AND pcc.PriceCode = pricesdata.PriceCode
	) as CostCode
FROM pricesdata
	JOIN clientsdata ON clientsdata.FirmCode = pricesdata.FirmCode
	JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
		JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
			JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
				JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
	AND clientsdata.firmtype = 0
	AND pricesdata.PriceCode = @NewPriceCode
	AND clientsdata2.firmtype = 1;";
			}

			ArHelper.WithSession(s => {
				s.CreateSQLQuery(command)
					.SetParameter("ClientCode", supplier.Id)
					.ExecuteUpdate();
			});
		}

		private User CreateUser(Client client, string userName, UserPermission[] permissions, Person[] persons)
		{
			var user = new User(client) {
				Name = userName,
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

		private void AddContacts(ContactGroupOwner owner, Contact[] clientContacts)
		{
			var generalGroup = owner.AddContactGroup(ContactGroupType.General);
			foreach (var contact in clientContacts)
				if (contact.ContactText != null)
					generalGroup.AddContact(contact);
			owner.Save();
		}

		public void RegisterPayer(uint id, bool showRegistrationCard)
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
			PropertyBag["PaymentOptions"] = new PaymentOptions();
		}

		public void Registered(
			[ARDataBind("Instance", AutoLoadBehavior.NewRootInstanceIfInvalidKey)] Payer payer,
			[DataBind("PaymentOptions")] PaymentOptions paymentOptions,
			bool showRegistrationCard)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				if (payer.ContactGroupOwner == null)
					payer.InitGroupOwner();

				payer.AddComment(paymentOptions.GetCommentForPayer());

				if (payer.JuridicalOrganizations == null || payer.JuridicalOrganizations.Count == 0)
				{
					payer.JuridicalOrganizations = new List<LegalEntity> {
						new LegalEntity(payer.Name, payer.JuridicalName, payer)
					};
				}
				else
				{
					var org = payer.JuridicalOrganizations.First();
					org.Name = payer.Name;
					org.FullName = payer.JuridicalName;
				}

				if (string.IsNullOrEmpty(payer.Customer))
					payer.Customer = payer.JuridicalName;

				payer.Save();
				scope.VoteCommit();
			}

			var supplier = payer.Suppliers.FirstOrDefault();
			var client = payer.Clients.FirstOrDefault();
			if (client != null)
				this.Mailer().NotifyBillingAboutClientRegistration(client);

			string redirectUrl;
			if (showRegistrationCard && client != null && client.Users.Count > 0)
				redirectUrl = String.Format("~/main/report?id={0}", client.Users.First());
			else if (client != null)
				redirectUrl = String.Format("~/Client/{0}", client.Id);
			else if (supplier != null)
				redirectUrl = String.Format("~/Suppliers/{0}", supplier.Id);
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
			var allowViewSuppliers = Admin.HavePermisions(PermissionType.ViewSuppliers);
			if (!allowViewSuppliers)
				return;
			var suppliers = Supplier.Queryable
					.Where(s => !s.Disabled && s.Name.Contains(searchPattern))
					.Take(50);
			if (payerId.HasValue && (payerId.Value > 0))
				suppliers = suppliers.Where(item => item.Payer.PayerID == payerId.Value);
			PropertyBag["suppliers"] = suppliers;
			CancelLayout();
		}
	}
}
