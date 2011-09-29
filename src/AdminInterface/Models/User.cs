using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Criterion;
using AdminInterface.Security;
using System.Web;
using Common.Web.Ui.Models;
using System.ComponentModel;
using AdminInterface.Models.Billing;

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
		public LoginNotFoundException(string message) : base(message) { }
	}

	public class CantChangePassword : Exception
	{
		public CantChangePassword(string message) : base(message) { }
	}

	public interface IEnablable
	{
		bool Enabled { get; }
	}

	[ActiveRecord(Schema = "future", Lazy = true), Auditable]
	public class User : ActiveRecordLinqBase<User>, IEnablable
	{
		private string _name;
		private bool _enabled;

		public User()
		{
			SendRejects = true;
			SendWaybills = true;
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
			UserUpdateInfo = new UserUpdateInfo(this);
			Logs = new AuthorizationLogEntity(this);
			Accounting = new UserAccount(this);

			RootService = service;
			if (service is Client)
			{
				Client = (Client)RootService;
				WorkRegionMask = Client.MaskRegion;
				OrderRegionMask = Client.Settings.OrderRegionMask;
			}
			else if (service is Supplier)
			{
				WorkRegionMask = ulong.MaxValue;
			}
		}

		public User(Client client)
			: this((Service)client)
		{
			Init(client);
		}

		[PrimaryKey(PrimaryKeyType.Native)]
		public virtual uint Id { get; set; }

		[Property(NotNull = true), Description("Имя"), Auditable]
		public virtual string Login { get; set; }

		[Property(Access = PropertyAccess.FieldLowercaseUnderscore), Description("Комментарий"), Auditable]
		public virtual string Name
		{
			get
			{
				if (String.IsNullOrEmpty(_name))
					return Login;
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		[Property(Access = PropertyAccess.FieldLowercaseUnderscore), Description("Включен"), Auditable]
		public virtual bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					if (Payer != null)
						Payer.PaymentSum = Payer.TotalSum;
				}
			}
		}

		[Property, Description("Подтверждать отправку заказов"), Auditable]
		public virtual bool SubmitOrders { get; set; }

		[Property, Description("Получать накладные"), Auditable]
		public virtual bool SendWaybills { get; set; }

		[Property, Description("Получать отказы"), Auditable]
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

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property, Description("Регионы работы"), Auditable]
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
			Description("Наследовать настройки прайс листов"), Auditable]
		public virtual User InheritPricesFrom { get; set; }

		[BelongsTo("PayerId", Lazy = FetchWhen.OnInvoke), Description("Плательщик"), Auditable]
		public virtual Payer Payer { get; set; }

		//не работает какая то фигня в хибере
		//[OneToOne(Cascade = CascadeEnum.All, Constrained = true)]
		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual AuthorizationLogEntity Logs { get; set; }

		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual UserUpdateInfo UserUpdateInfo { get; set; }

		[HasAndBelongsToMany(typeof (UserPermission),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "AssignedPermissions",
			Schema = "usersettings",
			ColumnRef = "PermissionId")]
		public virtual IList<UserPermission> AssignedPermissions { get; set; }

		[HasAndBelongsToMany(typeof (Address),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "UserAddresses",
			Schema = "future",
			ColumnRef = "AddressId")]
		public virtual IList<Address> AvaliableAddresses { get; set; }

		[BelongsTo("AccountingId", Cascade = CascadeEnum.All, Lazy = FetchWhen.OnInvoke)]
		public virtual Account Accounting { get; set; }

		[BelongsTo(Cascade = CascadeEnum.All)]
		public virtual Service RootService { get; set; }

		public virtual IList<User> ImpersonableUsers { set; get; }

		public virtual List<string> AvalilableAnalitFVersions
		{
			get {
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
				return new [] {"Любая версия"}.Concat(versions.Distinct().Select(v => v.ToString())).ToList();
			}
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

		public virtual void AddPermission(UserPermission permission)
		{
			if (AssignedPermissions == null)
				AssignedPermissions = new List<UserPermission>();
			AssignedPermissions.Add(permission);
		}

		public static User GetByLogin(string login)
		{
			return ActiveRecordMediator<User>.FindOne(Restrictions.Eq("Login", login));
		}

		public static User GetById(uint id)
		{
			return ActiveRecordMediator<User>.FindByPrimaryKey(id);
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
				try
				{
					return !ADHelper.IsBelongsToOfficeContainer(Login);
				}
				catch(Exception)
				{
					return false;
				}
			}
		}

		public override string ToString()
		{
			return Login;
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

		public virtual void Setup()
		{
			Login = "temporary-login";
			Enabled = true;
			if (Logs == null)
				Logs = new AuthorizationLogEntity(this);
			if (UserUpdateInfo == null)
				UserUpdateInfo = new UserUpdateInfo(this);

			var defaults = DefaultValues.Get();
			TargetVersion = defaults.AnalitFVersion;
			UserUpdateInfo.AFAppVersion = defaults.AnalitFVersion;
			if (Client == null)
				AssignedPermissions = UserPermission.FindPermissionsByType(UserPermissionTypes.SupplierInterface).ToList();
			Save();
			Login = Id.ToString();
			Update();

			if (Client != null)
			{
				AddPrices(Client);
			}
		}

		public virtual void Init(Client client)
		{
			Client = client;
			RootService = client;
			if (Payer == null)
			{
				Payer = client.Payers.Single();
				if (Payer.Users == null)
					Payer.Users = new List<User>();

				if (!Payer.Users.Any(u => u == this))
					Payer.Users.Add(this);
			}

			if (!Client.Users.Any(u => u == this))
				Client.Users.Add(this);

			if (Accounting == null)
				Accounting = new UserAccount(this);

			Registrant = SecurityContext.Administrator.UserName;
			RegistrationDate = DateTime.Now;
		}

		public virtual bool IsLocked
		{
			get
			{
				try
				{
					return (ADHelper.IsLoginExists(Login) && ADHelper.IsLocked(Login));
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public virtual bool IsExists
		{
			get
			{
				return (ADHelper.IsLoginExists(Login));
			}
		}

		/// <summary>
		/// Пользователь считается активным, если он получал обновление не более 7 дней назад
		/// </summary>
		public virtual bool IsActive
		{
			get
			{
				return Logs.AFTime.HasValue && DateTime.Now.Subtract(Logs.AFTime.Value).Days <= 7;
			}
		}

		public virtual bool HavePreparedData()
		{
			var file = String.Format(CustomSettings.UserPreparedDataFormatString, Id);
			return (File.Exists(file));
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
			ContactGroupOwner groupOwner = null;
			if (RootService is Client)
				groupOwner = ((Client)RootService).ContactGroupOwner;
			else if (RootService is Supplier)
				groupOwner = ((Supplier)RootService).ContactGroupOwner;

			var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
			ContactGroup = group;
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
			if (ContactGroup == null)
			{
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
insert into Future.UserPrices(PriceId, UserId, RegionId)
select i.PriceId, u.Id, i.RegionId
from Future.Users u
	join Future.Intersection i on i.ClientId = :ClientId
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
insert into Future.UserPrices(PriceId, UserId, RegionId)
select i.PriceId, u.Id, i.RegionId
from Future.Users u
	join Future.Intersection i on i.ClientId = :ClientId AND i.RegionId = :RegionId
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
	Future.UserPrices
WHERE
	UserId = :UserId AND RegionId = :RegionId
";
			return (Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(sql)
				.SetParameter("UserId", Id)
				.SetParameter("RegionId", region.Id)
				.UniqueResult())) > 0);
		}

		public virtual void AddContactPerson(string name)
		{
			if (String.IsNullOrEmpty(name))
				return;
			if (ContactGroup == null)
			{
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
				group.Save();
				group.AddPerson(name);
				ContactGroup = group;
			}
			else
				ContactGroup.AddPerson(name);
		}
		
		public virtual void MoveToAnotherClient(Client newOwner, LegalEntity legalEntity)
		{
			if (!newOwner.Orgs().Any(o => o.Id == legalEntity.Id))
				throw new Exception(String.Format("Не могу переместить пользователя {0} т.к. юр. лицо {1} не принадлежит клиенту {2}",
					this, legalEntity, newOwner));

			var regions = Region.FindAll();
			// Если маски регионов не совпадают, добавляем записи в UserPrices для тех регионов,
			// которых не было у старого клиента, но они есть у нового клиента
			if (Client.MaskRegion != newOwner.MaskRegion)
			{
				foreach (var region in regions)
				{
					// Если этот регион есть у старого клиента, пропускаем его
					if ((region.Id & Client.MaskRegion) > 0)
						continue;
					// Если региона нет у старого клиента, но он есть у нового,
					// и для этого пользователя нет прайсов в этом регионе добавляем прайсы для этого региона
					if ((region.Id & newOwner.MaskRegion) > 0)
					{
						if (!HavePricesInRegion(region))
							AddPrices(newOwner, region);
					}
				}
			}

			ClientInfoLogEntity.UpdateLogs(newOwner.Id, Id);
			Client = newOwner;
			RootService = newOwner;
			Payer = legalEntity.Payer;
			Update();
		}

		public virtual object GetRegistrant()
		{
			if (String.IsNullOrEmpty(Registrant))
				return null;

			return Administrator.GetByName(Registrant);
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
			if (RootService is Client)
			{
				groups = new[] {ContactGroupType.OrderManagers};
				owner = ((Client)RootService).ContactGroupOwner;
			}
			else if (RootService is Supplier)
			{
				groups = new[] {ContactGroupType.OrderManagers, ContactGroupType.ClientManagers};
				owner = ((Supplier)RootService).ContactGroupOwner;
			}

			if (owner == null)
				return "";

			var emails = owner.GetEmails(groups);
			if (emails.Count() > 0)
				return emails.Implode();

			return owner.GetEmails(ContactGroupType.General).Implode();
		}
	}
}