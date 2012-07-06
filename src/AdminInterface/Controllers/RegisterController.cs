using System;
using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
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
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class AdditionalSettings
	{
		public AdditionalSettings()
		{
			ShowRegistrationCard = true;
			SendRegistrationCard = true;
			FillBillingInfo = true;
		}

		[Description("Показывать регистрационную карту")]
		public bool ShowRegistrationCard { get; set; }

		[Description("Заполнять информацию для биллинга")]
		public bool FillBillingInfo { get; set; }

		public bool PayerExists { get; set; }

		public bool ShowForOneSupplier { get; set; }

		[Description("Отправлять регистрационную карту клиенту")]
		public bool SendRegistrationCard { get; set; }

		[Description("Регистрировать без адреса доставки и пользователя")]
		public bool RegisterEmpty { get; set; }
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
			var supplier = new Supplier();
			var user = new User();
			supplier.Account = new SupplierAccount(supplier);
			PropertyBag["options"] = new AdditionalSettings();
			PropertyBag["supplier"] = supplier;
			PropertyBag["user"] = user;
			PropertyBag["regions"] = Region.All().ToArray();
			PropertyBag["SingleRegions"] = true;
		}

		[AccessibleThrough(Verb.Post)]
		public void RegisterSupplier(
			[DataBind("supplierContacts")] Contact[] supplierContacts,
			ulong homeRegion, 
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("options")] AdditionalSettings options,

			[DataBind("payer")] Payer payer,
			uint? existingPayerId,

			[DataBind("userContacts")] Contact[] userContacts,
			[DataBind("userPersons")] Person[] userPersons,

			string additionalEmailsForSendingCard,
			string comment)
		{
			User user;
			string password;

			var supplier = new Supplier();
			supplier.RegionMask = regionSettings.GetBrowseMask();
			BindObjectInstance(supplier, "supplier");

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var currentPayer = RegisterPayer(options, payer, existingPayerId, supplier.Name, supplier.FullName);

				supplier.HomeRegion = Region.Find(homeRegion);
				supplier.Payer = currentPayer;
				supplier.Account = new SupplierAccount(supplier);
				supplier.ContactGroupOwner = new ContactGroupOwner(supplier.GetAditionalContactGroups());
				supplier.Registration = new RegistrationInfo(Admin);

				user = new User(supplier.Payer, supplier);
				BindObjectInstance(user, "user");

				if (!IsValid(supplier, user)) {
					RegisterSupplier();
					PropertyBag["options"] = options;
					PropertyBag["supplier"] = supplier;
					PropertyBag["user"] = user;
					return;
				}

				currentPayer.Suppliers.Add(supplier);
				currentPayer.UpdatePaymentSum();
				AddContacts(supplier.ContactGroupOwner, supplierContacts);
				supplier.OrderRules.Add(new OrderSendRules(Defaults, supplier));
				DbSession.Save(supplier);

				foreach (var group in supplier.ContactGroupOwner.ContactGroups)
				{
					var persons = BindObject<List<Person>>(group.Type + "Persons");
					var contacts = BindObject<List<Contact>>(group.Type + "Contacts");

					group.Persons = persons;
					group.Contacts = contacts;
				}

				var groups = BindObject<RegionalDeliveryGroup[]>("orderDeliveryGroup");

				foreach (var group in groups)
				{
					group.Region = Region.Find(group.Region.Id);
					group.Name = "Доставка заказов " + group.Region.Name;
					group.ContactGroupOwner = supplier.ContactGroupOwner;
					supplier.ContactGroupOwner.ContactGroups.Add(group);

					//повторная валидация, тк когда производился binding валидация не прошла
					//тк не было заполнено поле Name
					Validator.IsValid(group);
				}

				foreach (var group in supplier.ContactGroupOwner.ContactGroups)
				{
					group.Adopt();
					group.Save();
					group.Persons.Each(p => p.Save());
				}

				scope.Flush();

				CreateSupplier(supplier);
				Maintainer.MaintainIntersection(supplier);

				user.UpdateContacts(userContacts);
				foreach (var person in userPersons)
					user.AddContactPerson(person.Name);
				user.Setup();
				user.SetupSupplierPermission();
				password = user.CreateInAd();

				supplier.AddBillingComment(comment);

				scope.VoteCommit();
			}

			Mailer.SupplierRegistred(supplier, comment);
			supplier.CreateDirs();

			var log = new PasswordChangeLogEntity(user.Login);
			if (options.SendRegistrationCard)
				log = SendRegistrationCard(log, user, password, additionalEmailsForSendingCard);
			DbSession.Save(log);

			if (options.FillBillingInfo)
			{
				Session["password"] = password;
				Redirect("Register", "RegisterPayer", new {
					id = supplier.Payer.Id,
					showRegistrationCard = options.ShowRegistrationCard
				});
			}
			else if (supplier.Users.Count > 0 && options.ShowRegistrationCard)
			{
				Flash["password"] = password;
				Redirect("main", "report", new{id = supplier.Users.First().Id});
			}
			else
			{
				Notify("Регистрация завершена успешно");
				Redirect("Suppliers", "Show", new{id = supplier.Id});
			}
		}


		[AccessibleThrough(Verb.Get)]
		public void RegisterClient()
		{
			var regions = Region.All().ToArray();
			var client = new Client(new Payer(""), regions.First());
			var user = new User(client);

			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
			PropertyBag["address"] = client.AddAddress("");
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["regions"] = regions;
			PropertyBag["clientContacts"] = new [] { new Contact(ContactType.Phone, string.Empty), new Contact(ContactType.Email, string.Empty) };
			PropertyBag["options"] = new AdditionalSettings();
		}

		[AccessibleThrough(Verb.Post)]
		public void RegisterClient([DataBind("client")]Client client,
			ulong homeRegion,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("permissions")] UserPermission[] permissions,
			[DataBind("options")] AdditionalSettings options,
			[DataBind("payer")] Payer payer,
			uint? existingPayerId,
			[DataBind("supplier")] Supplier supplier,
			[DataBind("clientContacts")] Contact[] clientContacts,
			[DataBind("userContacts")] Contact[] userContacts,
			[DataBind("userPersons")] Person[] userPersons,
			string additionalEmailsForSendingCard,
			string comment)
		{
			var password = "";
			var fullName = client.FullName.Replace("№", "N").Trim();
			var name = client.Name.Replace("№", "N").Trim();
			var currentPayer = RegisterPayer(options, payer, existingPayerId, name, fullName);

			client = new Client(currentPayer,
				Region.Find(homeRegion)) {
					FullName = fullName,
					Name = name,
					MaskRegion = regionSettings.GetBrowseMask(),
					Registration = new RegistrationInfo(Admin),
					ContactGroupOwner = new ContactGroupOwner()
				};
			client.Settings.WorkRegionMask = client.MaskRegion;
			client.Settings.OrderRegionMask = regionSettings.GetOrderMask();
			var user = new User(client);
			var address = new Address();

			BindObjectInstance(client.Settings, "client.Settings");
			BindObjectInstance(user, "user");
			BindObjectInstance(user.Accounting, "user.Accounting");
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(address, "address");

			var equalClientInRegion = DbSession.QueryOver<Client>().Where(c => c.HomeRegion.Id == homeRegion && c.Name == name).RowCount() > 0;
			var forValidation = new List<object> {
				client
			};
			if (!options.RegisterEmpty)
				forValidation.Add(user);

			if (!IsValid(forValidation) || equalClientInRegion) {
				RegisterClient();
				PropertyBag["clientContacts"] = clientContacts;
				PropertyBag["client"] = client;
				PropertyBag["user"] = user;
				PropertyBag["address"] = address;
				PropertyBag["options"] = options;
				if (equalClientInRegion)
					Error(string.Format("В данном регионе уже существует клиент с таким именем {0}", name));
				return;
			}

			if (String.IsNullOrEmpty(address.Value) || options.RegisterEmpty)
				address = null;

			if (options.RegisterEmpty)
				user = null;

			if (address != null) {
				address.Value = address.Value.Replace("№", "N").Trim();
				client.AddAddress(address);
			}

			if (user != null)
				client.AddUser(user);

			CreateDrugstore(client, options, supplier);
			AddContacts(client.ContactGroupOwner, clientContacts);

			if (user != null) {
				CreateUser(user, permissions, userPersons);
				user.UpdateContacts(userContacts);
				user.RegistredWith(client.Addresses.LastOrDefault());

				password = user.CreateInAd();
			}

			client.Addresses.Each(a => a.CreateFtpDirectory());
			client.AddBillingComment(comment);

			if (user != null) {
				var log = new PasswordChangeLogEntity(user.Login);
				if (options.SendRegistrationCard)
					log = SendRegistrationCard(log, user, password, additionalEmailsForSendingCard);
				DbSession.Save(log);
			}

			Mailer.ClientRegistred(client, comment, Defaults);
			if (!options.FillBillingInfo)
				this.Mailer().NotifyBillingAboutClientRegistration(client);

			if (options.FillBillingInfo) {
				Session["password"] = password;
				Redirect("Register", "RegisterPayer", new {
					id = client.Payers.Single().Id,
					showRegistrationCard = options.ShowRegistrationCard
				});
			}
			else if (client.Users.Count > 0 && options.ShowRegistrationCard) {
				Flash["password"] = password;
				Redirect("main", "report", new{id = client.Users.First().Id});
			}
			else {
				Notify("Регистрация завершена успешно");
				RedirectTo(client);
			}
		}

		private static Payer RegisterPayer(AdditionalSettings additionalSettings, Payer payer, uint? existingPayerId, string name, string fullname)
		{
			Payer currentPayer = null;
			if (additionalSettings.PayerExists) {
				if ((payer != null && payer.Id != 0) || existingPayerId.HasValue) {
					var id = existingPayerId.HasValue ? existingPayerId.Value : payer.PayerID;
					currentPayer = Payer.Find(id);
					if (currentPayer.JuridicalOrganizations.Count == 0) {
						var organization = new LegalEntity();
						organization.Payer = currentPayer;
						organization.Name = currentPayer.Name;
						organization.FullName = currentPayer.JuridicalName;
						currentPayer.JuridicalOrganizations = new List<LegalEntity> {organization};
						organization.Save();
					}
				}
			}
			if (currentPayer == null) {
				currentPayer = new Payer(name, fullname);
				currentPayer.BeforeNamePrefix = "Аптека";
				currentPayer.Save();
			}
			return currentPayer;
		}

		private PasswordChangeLogEntity SendRegistrationCard(PasswordChangeLogEntity log, User user, string password, string additionalEmails)
		{
			var mailTo = EmailHelper.JoinMails(user.GetAddressForSendingClientCard(), user.GetEmails(), additionalEmails);
			var smtpid = ReportHelper.SendClientCard(user,
				password,
				true,
				Defaults,
				mailTo,
				additionalEmails);
			log.SetSentTo(smtpid, mailTo);
			return log;
		}

		private void CreateDrugstore(Client client, AdditionalSettings additionalSettings, Supplier supplier)
		{
			var costCrypKey = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select usersettings.GeneratePassword()")
				.UniqueResult<string>());

			var smartOrder = SmartOrderRules.TestSmartOrder();
			client.Settings.InvisibleOnFirm = (Convert.ToUInt32(additionalSettings.ShowForOneSupplier) > 0) ? DrugstoreType.Hidden : DrugstoreType.Standart;
			client.Settings.BasecostPassword = costCrypKey;
			client.Settings.SmartOrderRules = smartOrder;

			if (additionalSettings.ShowForOneSupplier)
			{
				client.Settings.NoiseCosts = true;
				client.Settings.NoiseCostExceptSupplier = Supplier.Find(supplier.Id);
			}
			client.SaveAndFlush();

			client.MaintainIntersection();
			client.Addresses.Each(a => a.MaintainInscribe());
		}

		private void CreateSupplier(Supplier supplier)
		{
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
INTO regionaldata
	(
		regioncode,
		firmcode
	)
SELECT DISTINCT regions.regioncode,
		s.Id
FROM (Customers.Suppliers s, farm.regions, pricesdata)
	LEFT JOIN regionaldata ON regionaldata.firmcode = s.id AND regionaldata.regioncode = regions.regioncode
WHERE   pricesdata.firmcode = s.Id
		AND s.Id = :ClientCode
		AND (s.RegionMask & regions.regioncode)>0
		AND regionaldata.firmcode is null;

INSERT
INTO pricesregionaldata
	(
		regioncode,
		pricecode
	)
SELECT DISTINCT regions.regioncode,
		pricesdata.pricecode
FROM (Customers.Suppliers s, farm.regions, pricesdata)
LEFT JOIN pricesregionaldata
		ON pricesregionaldata.pricecode = pricesdata.pricecode
		AND pricesregionaldata.regioncode = regions.regioncode
WHERE   pricesdata.firmcode = s.Id
		AND s.Id = :ClientCode
		AND (s.RegionMask & regions.regioncode) > 0
		AND pricesregionaldata.pricecode is null;";

			ArHelper.WithSession(s => {
				s.CreateSQLQuery(command)
					.SetParameter("ClientCode", supplier.Id)
					.ExecuteUpdate();
			});
		}

		private void CreateUser(User user, UserPermission[] permissions, Person[] persons)
		{
			if (permissions != null && permissions.Any())
			{
				user.AssignedPermissions = permissions.Select(i => UserPermission.Find(i.Id))
					.Concat(UserPermission.GetDefaultPermissions()).Distinct().ToList();
			}
			user.Setup();
			user.SetupSupplierPermission();
			foreach (var person in persons)
				user.AddContactPerson(person.Name);
			user.Save();
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
				if (payer.Id == 0)
					payer.Init(Admin);

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
				redirectUrl = String.Format("~/main/report?id={0}", client.Users.First().Id);
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
			var suppliers = DbSession.Query<Supplier>()
					.Where(s => !s.Disabled && s.Name.Contains(searchPattern));
			if (payerId.HasValue && payerId.Value > 0)
				suppliers = suppliers.Where(item => item.Payer.PayerID == payerId.Value);

			PropertyBag["suppliers"] = suppliers.Take(50).ToArray();
			CancelLayout();
		}
	}
}
