﻿using System;
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

	[ActiveRecord(Schema = "Future", Lazy = true)]
	public class AssignedService
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual ulong RegionMask { get; set; }

		[BelongsTo]
		public virtual User User { get; set; }

		[BelongsTo(Cascade = CascadeEnum.All)]
		public virtual Service Service { set; get; }

		[HasAndBelongsToMany(typeof (UserPermission),
			Lazy = true,
			ColumnKey = "AssignedServiceId",
			ColumnRef = "PermissionId",
			Table = "AssignedServicePermissions",
			Schema = "Future")]
		public virtual IList<UserPermission> Permissions { get; set; }
	}

	[ActiveRecord(Schema = "future", Lazy = true)]
	public class User : ActiveRecordLinqBase<User>, IEnablable
	{
		private UserUpdateInfo _updateInfo;

		public User()
		{
			SendRejects = true;
			SendWaybills = true;
			Enabled = true;
			Services = new List<AssignedService>();
		}

		public User(Client client)
		{
			Init(client);
		}

		[PrimaryKey(PrimaryKeyType.Native)]
		public virtual uint Id { get; set; }

		[Property(NotNull = true), Description("Имя"), Auditable]
		public virtual string Login { get; set; }

		[Property, Description("Комментарий"), Auditable]
		public virtual string Name { get; set; }

		[Property, Description("Включен"), Auditable]
		public virtual bool Enabled { get; set; }

		[Property, Description("Подтверждать отправку заказов"), Auditable]
		public virtual bool SubmitOrders { get; set; }

		[Property, Description("Получать накладные"), Auditable]
		public virtual bool SendWaybills { get; set; }

		[Property, Description("Получать отказы"), Auditable]
		public virtual bool SendRejects { get; set; }

		[Property, Description("Включить автообновление"), Auditable]
		public virtual bool EnableUpdate { get; set; }

		[Property, Description("Аудитор"), Auditable]
		public virtual bool Auditor { get; set; }

		[Property, Description("Разрешить обновление до версии"), Auditable]
		public virtual uint? TargetVersion { get; set; }

		[Property, Description("Сохранять подготовленные данные")]
		public virtual bool SaveAFDataFiles { get; set; }

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
		[Property(Column = "Free")]
		public virtual bool IsFree { get; set; }

		[BelongsTo("ClientId", /*NotNull = true, */Lazy = FetchWhen.OnInvoke), Description("Клиент"), Auditable]
		public virtual Client Client { get; set; }

		[BelongsTo("ContactGroupId", Lazy = FetchWhen.OnInvoke)]
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
		public virtual Accounting Accounting { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<AssignedService> Services { get; set; }

		[BelongsTo]
		public virtual Service RootService { get; set; }

		public virtual IList<User> ImpersonableUsers { set; get; }

		public virtual List<string> AvalilableAnalitFVersions
		{
			get {
				if (_updateInfo == null)
					_updateInfo = UserUpdateInfo.Find(Id);

				List<AnalitFVersionRule> rules;
				if (TargetVersion != null)
					rules = AnalitFVersionRule.Queryable
						.Where(r => r.SourceVersion == TargetVersion)
						.ToList();
				else
					rules = AnalitFVersionRule.Queryable
						.Where(r => r.SourceVersion == _updateInfo.AFAppVersion)
						.ToList();

				var versions = rules.Select(r => r.DestinationVersion).ToList();
				if (TargetVersion != null)
					versions.Add(TargetVersion.Value);
				versions.Add(_updateInfo.AFAppVersion);
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
			if (String.IsNullOrEmpty(Name))
				return Login;
			return String.Format("{0} ({1})", Login, Name);
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

		public override string ToString()
		{
			return Login;
		}

		public virtual bool IsPermissionAssigned(UserPermission permission)
		{
			foreach (var userPermission in AssignedPermissions)
				if (permission.Shortcut == userPermission.Shortcut)
					return true;
			return false;
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
				Client.Id);
			return password;
		}

		public virtual void Setup()
		{
			Login = "temporary-login";
			Enabled = true;
			Logs = new AuthorizationLogEntity();
			Logs.User = this;
			TargetVersion = DefaultValues.Get().AnalitFVersion;
			Save();
			Login = Id.ToString();
			Update();

			if (Client.Users == null)
				Client.Users = new List<User>();
			if (!Client.Users.Any(u => u == this))
				Client.Users.Add(this);

			Client.UpdateBeAccounted();
			new UserUpdateInfo(Id).Create();
		}

		public virtual void Setup(Client client)
		{
			Init(client);
			Setup();
			AddPrices(client);
		}

		public virtual void Init(Client client)
		{
			Client = client;
			if (Payer == null)
				Payer = client.Payers.Single();

			Registrant = SecurityContext.Administrator.UserName;
			RegistrationDate = DateTime.Now;

			Enabled = true;
			SendRejects = true;
			SendWaybills = true;
			Accounting = new UserAccounting(this);
			if (AssignedPermissions == null)
				AssignedPermissions = new List<UserPermission>();
		}

		public virtual bool IsLocked
		{
			get
			{
				return (ADHelper.IsLoginExists(Login) && ADHelper.IsLocked(Login));
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
			var result = ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
select sum(length(concat(uui.AFCopyId))) = 0
from usersettings.UserUpdateInfo uui
where uui.UserId = :userCode
")
					.SetParameter("userCode", Id)
					.UniqueResult<long?>());

			return result != null && result.Value == 0;
		}

		public virtual void ResetUin()
		{
			ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
update usersettings.UserUpdateInfo uui
set uui.AFCopyId = '' 
where uui.UserId = :userCode")
					.SetParameter("userCode", Id)
					.ExecuteUpdate());
		}

		public virtual void AddContactGroup()
		{
			using (var scope = new TransactionScope())
			{
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
				group.Save();
				this.ContactGroup = group;
				scope.VoteCommit();
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
			var emails = String.Empty;
			for (var i = 0; i < ContactGroup.Contacts.Count; i++)
			{
				var contact = ContactGroup.Contacts[i];
				if ((contact.Type == ContactType.Email) && (!emails.Contains(contact.ContactText)))
					emails += (String.IsNullOrEmpty(emails)) ? contact.ContactText : String.Format(", {0}", contact.ContactText);
			}
			return emails;
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
			ArHelper.WithSession(s => s.CreateSQLQuery(@"
update logs.clientsinfo set clientcode = :code
where userid = :userId")
							.SetParameter("code", newOwner.Id)
							.SetParameter("userId", Id)
							.ExecuteUpdate());
			Client = newOwner;
			Payer = legalEntity.Payer;
			Update();
		}

		public virtual object GetRegistrant()
		{
			if (String.IsNullOrEmpty(Registrant))
				return null;

			return Administrator.GetByName(Registrant);
		}

		public virtual void AssignService(ServiceSupplier supplier)
		{
			if (Services.Any(s => s.Service == supplier))
				throw new Exception(String.Format("Услуга {0} уже подключена", supplier));

			Services.Add(new AssignedService{ User = this, Service = supplier});
		}
	}
}