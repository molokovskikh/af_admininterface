using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.UI;
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
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;
using DataBinder = Castle.Components.Binder.DataBinder;

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
			var supplier = new Supplier();
			supplier.RegionMask = regionSettings.GetBrowseMask();
			SetARDataBinder(AutoLoadBehavior.NewRootInstanceIfInvalidKey);
			BindObjectInstance(supplier, "supplier");
			SetBinder(new DataBinder());

			var currentPayer = RegisterPayer(options, payer, existingPayerId, supplier.Name, supplier.FullName);

			supplier.HomeRegion = DbSession.Load<Region>(homeRegion);
			supplier.Payer = currentPayer;
			supplier.Account = new SupplierAccount(supplier);
			supplier.ContactGroupOwner = new ContactGroupOwner(supplier.GetAditionalContactGroups());
			supplier.Registration = new RegistrationInfo(Admin);

			var user = new User(supplier.Payer, supplier);
			BindObjectInstance(user, "user");

			if (!IsValid(supplier, user)) {
				RegisterSupplier();
				PropertyBag["options"] = options;
				PropertyBag["supplier"] = supplier;
				PropertyBag["user"] = user;
				return;
			}
			supplier.ContactGroupOwner.AddContactGroup(new ContactGroup(ContactGroupType.MiniMails));
			currentPayer.Suppliers.Add(supplier);
			currentPayer.UpdatePaymentSum();
			AddContacts(supplier.ContactGroupOwner, supplierContacts);
			supplier.OrderRules.Add(new OrderSendRules(Defaults, supplier));
			DbSession.Save(supplier);

			foreach (var group in supplier.ContactGroupOwner.ContactGroups) {
				var persons = BindObject<List<Person>>(group.Type + "Persons");
				var contacts = BindObject<List<Contact>>(group.Type + "Contacts");

				group.Persons = persons;
				group.Contacts = contacts;
			}

			var groups = BindObject<RegionalDeliveryGroup[]>("orderDeliveryGroup");

			foreach (var group in groups) {
				group.Region = DbSession.Load<Region>(group.Region.Id);
				group.Name = "Доставка заказов " + group.Region.Name;
				group.ContactGroupOwner = supplier.ContactGroupOwner;
				supplier.ContactGroupOwner.ContactGroups.Add(group);

				//повторная валидация, тк когда производился binding валидация не прошла
				//тк не было заполнено поле Name
				Validator.IsValid(group);
			}

			foreach (var group in supplier.ContactGroupOwner.ContactGroups) {
				group.Adopt();
				DbSession.Save(group);
				group.Persons.Each(p => DbSession.Save(p));
			}

			DbSession.Flush();

			DbSession.Query<Region>()
				.Where(r => (r.Id & supplier.RegionMask) > 0)
				.Each(r => supplier.AddRegion(r, DbSession));
			CreateSupplier(supplier);
			Maintainer.MaintainIntersection(supplier, DbSession);

			user.UpdateContacts(userContacts);
			foreach (var person in userPersons)
				user.AddContactPerson(person.Name);
			user.AssignDefaultPermission(DbSession);
			user.Setup();

			var password = user.CreateInAd(Session);
			supplier.AddBillingComment(comment);

			Mailer.SupplierRegistred(supplier, comment);
			//Создание директорий для поставщика на фтп
			supplier.CreateDirs();

			var log = new PasswordChangeLogEntity(user.Login);
			if (options.SendRegistrationCard)
				log = SendRegistrationCard(log, user, password.Password, additionalEmailsForSendingCard);
			DbSession.Save(log);

			if (options.FillBillingInfo) {
				Redirect("Register", "RegisterPayer", new {
					id = supplier.Payer.Id,
					showRegistrationCard = options.ShowRegistrationCard,
					passwordId = password.PasswordId
				});
			}
			else if (supplier.Users.Count > 0 && options.ShowRegistrationCard) {
				Redirect("main", "report", new {
					id = supplier.Users.First().Id,
					passwordId = password.PasswordId
				});
			}
			else {
				Notify("Регистрация завершена успешно");
				Redirect("Suppliers", "Show", new { id = supplier.Id });
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void RegisterClient()
		{
			var regions = Region.All().ToArray();
			var client = new Client(new Payer(""), regions.First());
			var user = new User(client);

			PropertyBag["defaultSettings"] = Defaults;
			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
			PropertyBag["address"] = client.AddAddress("");
			PropertyBag["account"] = user.Accounting;
			PropertyBag["permissions"] = UserPermission.FindPermissionsForDrugstore(DbSession);
			PropertyBag["regions"] = regions;
			PropertyBag["clientContacts"] = new[] { new Contact(ContactType.Phone, string.Empty), new Contact(ContactType.Email, string.Empty) };
			PropertyBag["options"] = new AdditionalSettings();
		}

		[AccessibleThrough(Verb.Post)]
		public void RegisterClient([DataBind("client")] Client client,
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
			PasswordCreation password = null;
			var fullName = client.FullName.Replace("№", "N").Trim();
			var name = client.Name.Replace("№", "N").Trim();
			var currentPayer = RegisterPayer(options, payer, existingPayerId, name, fullName);

			client = new Client(currentPayer, DbSession.Load<Region>(homeRegion)) {
				FullName = fullName,
				Name = name,
				MaskRegion = regionSettings.GetBrowseMask(),
				Registration = new RegistrationInfo(Admin),
				ContactGroupOwner = new ContactGroupOwner()
			};
			Defaults.Apply(client);
			client.Settings.WorkRegionMask = client.MaskRegion;
			client.Settings.OrderRegionMask = regionSettings.GetOrderMask();
			var user = new User(client);
			var address = new Address();
			Account account = user.Accounting;

			BindObjectInstance(client.Settings, "client.Settings");
			BindObjectInstance(user, "user");
			BindObjectInstance(account, "account");
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(address, "address");

			var equalClientInRegion = DbSession.QueryOver<Client>().Where(c => c.HomeRegion.Id == homeRegion && c.Name == name).RowCount() > 0;
			var forValidation = new List<object> {
				client
			};
			if (!options.RegisterEmpty) {
				forValidation.Add(user);
				client.AddUser(user);
			}

			if (!IsValid(forValidation) || equalClientInRegion) {
				DbSession.Delete(currentPayer);
				RegisterClient();
				PropertyBag["clientContacts"] = clientContacts;
				PropertyBag["client"] = client;
				PropertyBag["user"] = user;
				PropertyBag["address"] = address;
				PropertyBag["options"] = options;
				PropertyBag["account"] = account;
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

			CreateDrugstore(client, options, supplier);
			AddContacts(client.ContactGroupOwner, clientContacts);

			if (user != null) {
				CreateUser(user, permissions, userPersons);
				user.UpdateContacts(userContacts);
				user.RegistredWith(client.Addresses.LastOrDefault());

				password = user.CreateInAd(Session);

				var log = new PasswordChangeLogEntity(user.Login);
				if (options.SendRegistrationCard)
					log = SendRegistrationCard(log, user, password.Password, additionalEmailsForSendingCard);
				DbSession.Save(log);
			}

			client.Addresses.Each(a => a.CreateFtpDirectory());
			client.AddBillingComment(comment);
			new Mailer(DbSession).ClientRegistred(client, comment, Defaults);

			if (!options.FillBillingInfo)
				Mail().NotifyBillingAboutClientRegistration(client);

			if (options.FillBillingInfo) {
				Redirect("Register", "RegisterPayer", new {
					id = client.Payers.Single().Id,
					showRegistrationCard = options.ShowRegistrationCard,
					passwordId = password != null ? password.PasswordId : ""
				});
			}
			else if (client.Users.Count > 0 && options.ShowRegistrationCard) {
				Redirect("main", "report", new {
					id = client.Users.First().Id,
					passwordId = password.PasswordId
				});
			}
			else {
				Notify("Регистрация завершена успешно");
				RedirectTo(client);
			}
		}

		private Payer RegisterPayer(AdditionalSettings additionalSettings, Payer payer, uint? existingPayerId, string name, string fullname)
		{
			Payer currentPayer = null;
			if (additionalSettings.PayerExists) {
				if ((payer != null && payer.Id != 0) || existingPayerId.HasValue) {
					var id = existingPayerId.HasValue ? existingPayerId.Value : payer.Id;
					currentPayer = DbSession.Load<Payer>(id);
					if (currentPayer.Orgs.Count == 0) {
						var org = new LegalEntity(currentPayer);
						currentPayer.Orgs.Add(org);
						DbSession.Save(org);
					}
				}
			}
			if (currentPayer == null) {
				currentPayer = new Payer(name, fullname);
				currentPayer.BeforeNamePrefix = "Аптека";
				DbSession.Save(currentPayer);
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
			if (additionalSettings.ShowForOneSupplier) {
				client.Settings.InvisibleOnFirm = DrugstoreType.Hidden;
				client.Settings.NoiseCosts = true;
				client.Settings.NoiseCostExceptSupplier = DbSession.Load<Supplier>(supplier.Id);
			}
			DbSession.Save(client);
			DbSession.Flush();

			client.MaintainIntersection(DbSession);
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

INSERT INTO PricesCosts (PriceCode, PriceItemId) SELECT @NewPriceCode, @NewPriceItemId;
SET @NewPriceCostId = Last_Insert_ID();

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId;

INSERT
INTO pricesregionaldata
	(
		regioncode,
		pricecode,
		basecost
	)
SELECT DISTINCT regions.regioncode,
		pricesdata.pricecode,
		 @NewPriceCostId
FROM (Customers.Suppliers s, farm.regions, pricesdata)
LEFT JOIN pricesregionaldata
		ON pricesregionaldata.pricecode = pricesdata.pricecode
		AND pricesregionaldata.regioncode = regions.regioncode
WHERE   pricesdata.firmcode = s.Id
		AND s.Id = :ClientCode
		AND (s.RegionMask & regions.regioncode) > 0
		AND pricesregionaldata.pricecode is null;";

			DbSession.CreateSQLQuery(command)
				.SetFlushMode(FlushMode.Always)
				.SetParameter("ClientCode", supplier.Id)
				.ExecuteUpdate();
		}

		private void CreateUser(User user, UserPermission[] permissions, Person[] persons)
		{
			if (permissions != null)
				permissions = permissions.Select(i => DbSession.Load<UserPermission>(i.Id)).ToArray();
			user.AssignDefaultPermission(DbSession, permissions);
			user.Setup();
			foreach (var person in persons)
				user.AddContactPerson(person.Name);
			DbSession.Save(user);
		}

		private void AddContacts(ContactGroupOwner owner, Contact[] clientContacts)
		{
			var generalGroup = owner.AddContactGroup(ContactGroupType.General);
			foreach (var contact in clientContacts)
				if (contact.ContactText != null)
					generalGroup.AddContact(contact);
			DbSession.Save(owner);
		}

		public void RegisterPayer(uint id, bool showRegistrationCard, string passwordId)
		{
			var payer = DbSession.Get<Payer>(id);
			if (payer == null) {
				payer = new Payer {
					Orgs = {
						new LegalEntity()
					}
				};
			}

			PropertyBag["Instance"] = payer;
			PropertyBag["payer"] = payer;
			PropertyBag["JuridicalOrganization"] = payer.Orgs.First();
			PropertyBag["showRegistrationCard"] = showRegistrationCard;
			PropertyBag["PaymentOptions"] = new PaymentOptions();
			PropertyBag["passwordId"] = passwordId;
		}

		public void Registered(
			[ARDataBind("Instance", AutoLoadBehavior.NewRootInstanceIfInvalidKey)] Payer payer,
			[DataBind("PaymentOptions")] PaymentOptions paymentOptions,
			bool showRegistrationCard,
			string passwordId)
		{
			if (payer.Id == 0)
				payer.Init(Admin);

			payer.AddComment(paymentOptions.GetCommentForPayer());

			if (payer.Orgs.Count == 0) {
				payer.Orgs.Add(new LegalEntity(payer));
			}
			else {
				var org = payer.Orgs.First();
				org.Name = payer.Name;
				org.FullName = payer.JuridicalName;
			}

			if (string.IsNullOrEmpty(payer.Customer))
				payer.Customer = payer.JuridicalName;

			DbSession.SaveOrUpdate(payer);

			var supplier = payer.Suppliers.FirstOrDefault();
			var client = payer.Clients.FirstOrDefault();
			if (client != null)
				Mail().NotifyBillingAboutClientRegistration(client);
			else
				Mail().PayerRegistred(payer);

			if (showRegistrationCard && client != null && client.Users.Count > 0) {
				Redirect("main", "report", new {
					id = client.Users.First().Id,
					passwordId
				});
			}
			else if (client != null)
				RedirectTo(client);
			else if (supplier != null)
				RedirectTo(supplier);
			else
				RedirectTo(payer);
		}

		public void SearchPayers(string searchPattern)
		{
			if (String.IsNullOrEmpty(searchPattern))
				return;
			PropertyBag["payers"] = Payer.GetLikeAvaliable(DbSession, searchPattern);
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
				suppliers = suppliers.Where(item => item.Payer.Id == payerId.Value);

			PropertyBag["suppliers"] = suppliers.Take(50).ToArray();
			CancelLayout();
		}
	}
}