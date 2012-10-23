﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Listeners;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NHibernate.Criterion;
using AdminInterface.Security;
using System.Web;
using Common.Web.Ui.Models;
using System.ComponentModel;
using AdminInterface.Models.Billing;
using NHibernate.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Usersettings")]
	public class AnalitFVersionRule : ActiveRecordLinqBase<AnalitFVersionRule>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint SourceVersion { get; set; }

		[Property]
		public uint DestinationVersion { get; set; }
	}

	public enum UserADStatus
	{
		[Description("")] Ok,
		[Description("Заблокирован")] Locked,
		[Description("Отключен")] Disabled,
		[Description("Не существует")] NotExists,
	}

	public class LoginNotFoundException : Exception
	{
		public LoginNotFoundException(string message) : base(message)
		{
		}
	}

	public class CantChangePassword : Exception
	{
		public CantChangePassword(string message) : base(message)
		{
		}
	}

	public interface IEnablable
	{
		bool Enabled { get; }

		[Style]
		bool Disabled { get; }
	}

	public interface IDisabledByParent
	{
		[Style]
		bool DisabledByParent { get; }
	}

	[ActiveRecord(Schema = "Customers", Lazy = true), Auditable, Description("Пользователь")]
	public class User : IEnablable, IDisabledByParent, IChangesNotificationAware, IMultiAuditable
	{
		private string _name;
		private bool _enabled;

		public User()
		{
			SendWaybills = false;
			SendRejects = true;
			Enabled = true;
			ShowSupplierCost = true;
			AssignedPermissions = new List<UserPermission>();
			AvaliableAddresses = new List<Address>();
		}

		public User(Payer payer, Service rootService)
			: this(rootService)
		{
			Payer = payer;
		}

		public User(Service service)
			: this()
		{
			RootService = service;
			if (service is Client) {
				Client = (Client)RootService;
				WorkRegionMask = Client.MaskRegion;
				OrderRegionMask = Client.Settings.OrderRegionMask;
			}
			else if (service is Supplier) {
				WorkRegionMask = ulong.MaxValue;
			}
			UserUpdateInfo = new UserUpdateInfo(this);
			Logs = new AuthorizationLogEntity(this);
			Accounting = new UserAccount(this);
			Registration = new RegistrationInfo(SecurityContext.Administrator);
		}

		[PrimaryKey(PrimaryKeyType.Native)]
		public virtual uint Id { get; set; }

		[Property(NotNull = true), Description("Имя"), Auditable]
		public virtual string Login { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Description("Комментарий"), Auditable, ValidateNonEmpty]
		public virtual string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				if (!String.IsNullOrEmpty(_name))
					_name = _name.Replace("№", "N").Trim();
			}
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Description("Включен"), Auditable]
		public virtual bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (_enabled != value) {
					_enabled = value;
					if (Payer != null)
						Payer.PaymentSum = Payer.TotalSum;
				}
			}
		}

		[Property, Description("Подтверждать отправку заказов"), Auditable]
		public virtual bool SubmitOrders { get; set; }

		[Property, Description("Игнорировать проверку минимальной суммы заказа у Поставщика"), Auditable]
		public virtual bool IgnoreCheckMinOrder { get; set; }

		[Property, Description("Передавать файлы-НАКЛАДНЫЕ на сторону аптеки"), Auditable]
		public virtual bool SendWaybills { get; set; }

		[Property, Description("Передавать файлы-ОТКАЗЫ на сторону аптеки"), Auditable]
		public virtual bool SendRejects { get; set; }

		[Property, Description("Аудитор"), Auditable]
		public virtual bool Auditor { get; set; }

		[Property, Description("Разрешить обновление до версии"), Auditable]
		public virtual uint? TargetVersion { get; set; }

		[Property, Description("Сохранять подготовленные данные")]
		public virtual bool SaveAFDataFiles { get; set; }

		[Property, Description("Загружать неподтвержденные заказы"), Auditable]
		public virtual bool AllowDownloadUnconfirmedOrders { get; set; }

		[Property, Description("Отображать реальную цену поставщика"), Auditable]
		public virtual bool ShowSupplierCost { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set; }

		[Property, Description("Регионы работы"), Auditable, NotifyBilling, SetForceReplication]
		public virtual ulong WorkRegionMask { get; set; }

		[Property, Description("Регионы заказа"), Auditable]
		public virtual ulong OrderRegionMask { get; set; }

		[Property, Description("Проверять текущие цены и остатки пред отправкой заказов"), Auditable]
		public virtual bool UseAdjustmentOrders { get; set; }

/*
		[Property, Description("Не проверять УИН"), Auditable]
		public virtual bool DoNotCheckUin { get; set; }
*/

		[BelongsTo("ClientId", /*NotNull = true, */Lazy = FetchWhen.OnInvoke), Description("Клиент"), Auditable]
		public virtual Client Client { get; set; }

		[BelongsTo("ContactGroupId", Lazy = FetchWhen.OnInvoke, Cascade = CascadeEnum.All)]
		public virtual ContactGroup ContactGroup { get; set; }

		[BelongsTo("InheritPricesFrom", Lazy = FetchWhen.OnInvoke),
		 Description("Наследовать настройки прайс листов"), Auditable, SetForceReplication]
		public virtual User InheritPricesFrom { get; set; }

		[BelongsTo("PayerId", Lazy = FetchWhen.OnInvoke), Description("Плательщик"), Auditable]
		public virtual Payer Payer { get; set; }

		//не работает какая то фигня в хибере
		//[OneToOne(Cascade = CascadeEnum.All, Constrained = true)]
		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual AuthorizationLogEntity Logs { get; set; }

		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual UserUpdateInfo UserUpdateInfo { get; set; }

		[
			HasAndBelongsToMany(typeof(UserPermission),
				Lazy = true,
				ColumnKey = "UserId",
				Table = "AssignedPermissions",
				Schema = "usersettings",
				ColumnRef = "PermissionId"),
			Auditable("Права доступа")
		]
		public virtual IList<UserPermission> AssignedPermissions { get; set; }

		[
			HasAndBelongsToMany(typeof(Address),
				Lazy = true,
				ColumnKey = "UserId",
				Table = "UserAddresses",
				Schema = "Customers",
				ColumnRef = "AddressId"),
			Auditable("список адресов доставки пользователя")
		]
		public virtual IList<Address> AvaliableAddresses { get; set; }

		[
			HasAndBelongsToMany(typeof(User),
				Lazy = true,
				ColumnKey = "PrimaryUserId",
				Table = "Showusers",
				Schema = "Customers",
				ColumnRef = "ShowUserId"),
			Auditable("Логины в видимости пользователя")
		]
		public virtual IList<User> ShowUsers { get; set; }

		[HasAndBelongsToMany(typeof(User),
			Lazy = true,
			ColumnKey = "ShowUserId",
			Table = "Showusers",
			Schema = "Customers",
			ColumnRef = "PrimaryUserId")]
		public virtual IList<User> RootShowUsers { get; set; }

		[BelongsTo("AccountingId", Cascade = CascadeEnum.All, Lazy = FetchWhen.OnInvoke)]
		public virtual Account Accounting { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public virtual Service RootService { get; set; }

		public virtual List<string> AvalilableAnalitFVersions
		{
			get
			{
				if (Id == 0)
					return null;

				List<AnalitFVersionRule> rules;
				if (TargetVersion != null)
					rules = AnalitFVersionRule.Queryable
						.Where(r => r.SourceVersion == TargetVersion)
						.ToList();
				else
					rules = AnalitFVersionRule.Queryable
						.Where(r => r.SourceVersion == UserUpdateInfo.AFAppVersion)
						.ToList();

				var versions = rules.Select(r => r.DestinationVersion).ToList();
				if (TargetVersion != null)
					versions.Add(TargetVersion.Value);
				versions.Add(UserUpdateInfo.AFAppVersion);
				versions.Sort();
				return new[] { "Любая версия" }.Concat(versions.Distinct().Select(v => v.ToString())).ToList();
			}
		}

		[Style]
		public virtual bool DisabledByParent
		{
			get { return RootService.Disabled; }
		}

		[Style]
		public virtual bool Disabled
		{
			get { return !Enabled; }
		}

		[Style]
		public virtual bool CanNotOrder
		{
			get
			{
				return Client == null ||
					Client.Settings.ServiceClient ||
					OrderRegionMask == 0 ||
					AvaliableAddresses.Count == 0;
			}
		}

		public virtual IList<AuditRecord> GetAuditRecord(ISession session, MessageQuery query)
		{
			var userMessages = query.Execute(this, session);

			return userMessages.OrderByDescending(o => o.WriteTime).ToList();
		}

		public virtual bool SupplierUser()
		{
			return RootService.Type == ServiceType.Supplier;
		}

		public virtual List<Address> GetAvaliableAddresses()
		{
			return Client.Addresses.OrderBy(a => a.LegalEntity.Name).ThenBy(a => a.Name).ToList();
		}

		public virtual string GetLoginOrName()
		{
			if (String.IsNullOrEmpty(Name))
				return Login;
			return Name;
		}

		public virtual string GetLoginWithName()
		{
			if (String.IsNullOrEmpty(_name))
				return Login;
			return String.Format("{0} ({1})", Login, _name);
		}

		public static User GetByLogin(string login)
		{
			return ActiveRecordMediator<User>.FindOne(Restrictions.Eq("Login", login));
		}

		public virtual void CheckLogin()
		{
			if (!ADHelper.IsLoginExists(Login))
				throw new LoginNotFoundException(String.Format("Пользователь {0} не найден", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword("Запрещено изменять пароль для пользователей из офиса.");
		}

		public virtual bool CanChangePassword
		{
			get
			{
				try {
					return !ADHelper.IsBelongsToOfficeContainer(Login);
				}
				catch (Exception) {
					return false;
				}
			}
		}

		public override string ToString()
		{
			return Login;
		}

		public virtual IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties)
		{
			if (properties != null && properties.Any(p => p.Property.Name.Equals("Enabled")))
				return new List<IAuditRecord> {
					new PayerAuditRecord(Payer, "$$$", EditComment) { ShowOnlyPayer = true, ObjectType = LogObjectType.User, ObjectId = Id, Name = Name },
					new AuditRecord(this) { MessageType = LogMessageType.User, Type = LogObjectType.User, Name = Name }
				};
			return new List<IAuditRecord> { new AuditRecord(this) { MessageType = LogMessageType.User, Type = LogObjectType.User, Name = Name } };
		}

		public virtual bool ShouldNotify()
		{
			return Payer.Id != 921;
		}

		public virtual bool IsPermissionAssigned(UserPermission permission)
		{
			return AssignedPermissions.Any(p => permission.Shortcut == p.Shortcut);
		}

		public virtual bool IsChangePasswordByOneself()
		{
			var log = ActiveRecordBase<PasswordChangeLogEntity>.FindFirst(
				DetachedCriteria.For<PasswordChangeLogEntity>()
					.Add(Restrictions.Eq("TargetUserName", Login))
					.AddOrder(Order.Desc("LogTime")));
			if (log == null)
				return false;
			return log.IsChangedByOneSelf();
		}

		public static string GeneratePassword()
		{
			var availableChars = "23456789qwertyupasdfghjkzxcvbnmQWERTYUPASDFGHJKLZXCVBNM";
			var password = String.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}

		public virtual string CreateInAd()
		{
			var password = GeneratePassword();
			ADHelper.CreateUserInAD(Login,
				password,
				RootService.Id);
			return password;
		}

		public static string GetTempLogin()
		{
			return Guid.NewGuid().ToString();
		}

		public virtual void Setup()
		{
			Login = GetTempLogin();
			Enabled = true;
			if (Logs == null)
				Logs = new AuthorizationLogEntity(this);
			if (UserUpdateInfo == null)
				UserUpdateInfo = new UserUpdateInfo(this);

			var defaults = ActiveRecordMediator<DefaultValues>.FindFirst();
			TargetVersion = defaults.AnalitFVersion;
			UserUpdateInfo.AFAppVersion = defaults.AnalitFVersion;
			ActiveRecordMediator.Save(this);
			Login = Id.ToString();
			ActiveRecordMediator.Save(this);

			if (Client != null)
				AddPrices(Client);
		}

		public virtual void AssignDefaultPermission(ISession session, IEnumerable<UserPermission> permissions = null)
		{
			if (permissions == null)
				permissions = new UserPermission[0];

			var availability = UserPermissionAvailability.Supplier;
			if (Client != null)
				availability = UserPermissionAvailability.Drugstore;

			var defaultPermissions = UserPermission.DefaultPermissions(session, availability);
			permissions = permissions.Concat(defaultPermissions)
				.Distinct()
				.Except(AssignedPermissions)
				.ToArray();
			foreach (var permission in permissions) {
				AssignedPermissions.Add(permission);
			}
		}

		public virtual bool IsLocked
		{
			get
			{
				try {
					return (ADHelper.IsLoginExists(Login) && ADHelper.IsLocked(Login));
				}
				catch (Exception) {
					return false;
				}
			}
		}

		public virtual bool IsExists
		{
			get { return (ADHelper.IsLoginExists(Login)); }
		}

		[Style]
		public virtual bool IsOldUserUpdate
		{
			get { return !(Logs.AFTime.HasValue && DateTime.Now.Subtract(Logs.AFTime.Value).Days <= 7); }
		}

		public virtual bool HavePreparedData()
		{
			var files = Directory.GetFiles(Global.Config.UserPreparedDataDirectory)
				.Where(f => Regex.IsMatch(Path.GetFileName(f), string.Format(@"^({0}_)\d+?\.zip", Id))).ToList();
			return files.Count > 0;
		}

		public virtual bool HaveUin()
		{
			return !String.IsNullOrWhiteSpace(UserUpdateInfo.AFCopyId);
		}

		public virtual void ResetUin()
		{
			UserUpdateInfo.AFCopyId = "";
		}

		public virtual void AddContactGroup()
		{
			var groupOwner = ContactGroupoOwner;
			var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
			ContactGroup = group;
		}

		public virtual ContactGroupOwner ContactGroupoOwner
		{
			get
			{
				ContactGroupOwner groupOwner = null;
				if (NHibernateUtil.GetClass(RootService) == typeof(Client))
					groupOwner = ActiveRecordMediator<Client>.FindByPrimaryKey(RootService.Id).ContactGroupOwner;
				else if (NHibernateUtil.GetClass(RootService) == typeof(Supplier))
					groupOwner = Supplier.Find(RootService.Id).ContactGroupOwner;
				return groupOwner;
			}
		}

		public virtual void UpdateContacts(Contact[] contacts)
		{
			UpdateContacts(contacts, null);
		}

		public virtual void UpdateContacts(Contact[] contacts, Contact[] deletedContacts)
		{
			if (ContactGroup == null)
				AddContactGroup();
			ContactGroup.UpdateContacts(contacts, deletedContacts);
		}

		public virtual void UpdatePersons(Person[] persons)
		{
			UpdatePersons(persons, null);
		}

		public virtual void UpdatePersons(Person[] persons, Person[] deletedPersons)
		{
			if (persons.Length == 0)
				return;
			if (ContactGroup == null) {
				AddContactGroup();
				foreach (var person in persons)
					ContactGroup.AddPerson(person.Name);
				return;
			}
			ContactGroup.UpdatePersons(persons, deletedPersons);
		}

		public virtual string GetEmails()
		{
			return ContactGroup.Contacts
				.Where(c => c.Type == ContactType.Email)
				.Select(c => c.ContactText)
				.Distinct()
				.Implode();
		}

		public virtual void AddPrices(Client client)
		{
			var sql = @"
insert into Customers.UserPrices(PriceId, UserId, RegionId)
select i.PriceId, u.Id, i.RegionId
from Customers.Users u
	join Customers.Intersection i on i.ClientId = :ClientId
where u.Id = :UserId
group by i.PriceId, i.RegionId";

			ArHelper.WithSession(session => session.CreateSQLQuery(sql)
				.SetParameter("UserId", Id)
				.SetParameter("ClientId", client.Id)
				.ExecuteUpdate());
		}

		public virtual void AddPrices(Client client, Region region)
		{
			var sql = @"
insert into Customers.UserPrices(PriceId, UserId, RegionId)
select i.PriceId, u.Id, i.RegionId
from Customers.Users u
	join Customers.Intersection i on i.ClientId = :ClientId AND i.RegionId = :RegionId
where u.Id = :UserId
group by i.PriceId, i.RegionId";

			ArHelper.WithSession(session => session.CreateSQLQuery(sql)
				.SetParameter("UserId", Id)
				.SetParameter("ClientId", client.Id)
				.SetParameter("RegionId", region.Id)
				.ExecuteUpdate());
		}

		public virtual bool HavePricesInRegion(Region region)
		{
			var sql = @"
SELECT COUNT(*)
FROM
	Customers.UserPrices
WHERE
	UserId = :UserId AND RegionId = :RegionId
";
			return (Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(sql)
				.SetParameter("UserId", Id)
				.SetParameter("RegionId", region.Id)
				.UniqueResult())) > 0);
		}

		public virtual List<Region> GetRegions()
		{
			return Region.All().Where(r => (r.Id & WorkRegionMask) > 0).ToList();
		}

		public virtual void AddContactPerson(string name)
		{
			if (String.IsNullOrEmpty(name))
				return;
			if (ContactGroup == null) {
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
				group.Save();
				group.AddPerson(name);
				ContactGroup = group;
			}
			else
				ContactGroup.AddPerson(name);
		}

		public virtual AuditRecord MoveToAnotherClient(Client newOwner, LegalEntity legalEntity)
		{
			if (!newOwner.Orgs().Any(o => o.Id == legalEntity.Id))
				throw new Exception(String.Format("Не могу переместить пользователя {0} т.к. юр. лицо {1} не принадлежит клиенту {2}",
					this, legalEntity, newOwner));

			var regions = Region.FindAll();
			// Если маски регионов не совпадают, добавляем записи в UserPrices для тех регионов,
			// которых не было у старого клиента, но они есть у нового клиента
			if (Client.MaskRegion != newOwner.MaskRegion) {
				foreach (var region in regions) {
					// Если этот регион есть у старого клиента, пропускаем его
					if ((region.Id & Client.MaskRegion) > 0)
						continue;
					// Если региона нет у старого клиента, но он есть у нового,
					// и для этого пользователя нет прайсов в этом регионе добавляем прайсы для этого региона
					if ((region.Id & newOwner.MaskRegion) > 0) {
						if (!HavePricesInRegion(region))
							AddPrices(newOwner, region);
					}
				}
			}
			var message = String.Format("Перемещение пользователя от {0} к {1}", Client, newOwner);
			AuditRecord.UpdateLogs(newOwner.Id, Id);
			Client = newOwner;
			RootService = newOwner;
			Payer = legalEntity.Payer;
			InheritPricesFrom = null;
			ActiveRecordMediator.Save(this);

			return new AuditRecord(message, this);
		}

		public virtual string GetEmailForBilling()
		{
			var mails = Payer.ContactGroupOwner.GetEmails(ContactGroupType.Billing);
			if (Client != null)
				mails = Client.ContactGroupOwner.GetEmails(ContactGroupType.Billing).Concat(mails);

			return mails.Distinct().Implode();
		}

		public virtual string GetAddressForSendingClientCard()
		{
			ContactGroupOwner owner = null;
			var groups = new ContactGroupType[0];
			if (RootService is Client) {
				groups = new[] { ContactGroupType.OrderManagers };
				owner = ((Client)RootService).ContactGroupOwner;
			}
			else if (RootService is Supplier) {
				groups = new[] { ContactGroupType.OrderManagers, ContactGroupType.ClientManagers };
				owner = ((Supplier)RootService).ContactGroupOwner;
			}

			if (owner == null)
				return "";

			var emails = owner.GetEmails(groups);
			if (emails.Any())
				return emails.Implode();

			return owner.GetEmails(ContactGroupType.General).Implode();
		}

		public virtual void AddBillingComment(string billingMessage)
		{
			if (String.IsNullOrEmpty(billingMessage))
				return;

			new AuditRecord("Сообщение в биллинг: " + billingMessage, this).Save();

			billingMessage = String.Format("О регистрации Пользователя {0} ( {1} ) для клиента {2} ( {3} ): {4}", Name, Id, Client.Name, Client.Id, billingMessage);
			Payer.AddComment(billingMessage);
		}

		public virtual void RegistredWith(Address address)
		{
			if (address == null)
				return;

			address.Accounting.IsFree = Accounting.IsFree;
			address.Accounting.FreePeriodEnd = Accounting.FreePeriodEnd;

			AvaliableAddresses.Add(address);
		}

		public virtual IEnumerable<ModelAction> Actions
		{
			get
			{
				return ArHelper.WithSession(s => {
					return new[] {
						new ModelAction(this, "Unlock", "Разблокировать", !IsLocked),
						new ModelAction(this, "DeletePreparedData", "Удалить подготовленные данные", !HavePreparedData()),
						new ModelAction(this, "Delete", "Удалить", !CanDelete(s)),
					};
				});
			}
		}

		public virtual bool CanDelete(ISession session)
		{
			var canDelete = ClientOrder.CanDelete(session.Query<ClientOrder>().Where(o => o.User == this));
			return Disabled && canDelete;
		}

		public virtual void CheckBeforeDelete(ISession session)
		{
			if (!Disabled)
				throw new EndUserException(String.Format("Пользователь {0} не отключен", Name));

			var canDelete = ClientOrder.CanDelete(session.Query<ClientOrder>().Where(o => o.User == this));
			if (!canDelete)
				throw new EndUserException(String.Format("Для пользователя {0} есть заказы за интервал больше 14 дней", Name));
		}

		public virtual void Delete()
		{
			Payer.Users.Remove(this);
			if (Client != null)
				Client.Users.Remove(this);
			AuditRecord.DeleteAuditRecords(this);
			PayerAuditRecord.DeleteAuditRecords(Accounting);
			ActiveRecordMediator.Delete(this);
		}

		public virtual string EditComment { get; set; }
	}

	public class ModelAction
	{
		public ModelAction(object entity, string action, string name, bool disabled = false)
		{
			Controller = Common.Web.Ui.Helpers.AppHelper.GetControllerName(entity);
			Id = ((dynamic)entity).Id;
			Action = action;
			Name = name;
			Disabled = disabled;
		}

		public string Controller { get; set; }
		public string Action { get; set; }
		public uint Id { get; set; }
		public string Name { get; set; }
		public bool Disabled { get; set; }
	}
}