using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using AdminInterface.Security;
using System.Web;
using Common.Web.Ui.Models;
using System.ComponentModel;
using AdminInterface.Models.Billing;

namespace AdminInterface.Models
{
	public enum UserADStatus
	{
		[Description("")] Ok,
		[Description("«аблокирован")] Locked,
		[Description("ќтключен")] Disabled,
		[Description("Ќе существует")] NotExists,
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

	[ActiveRecord("Users", Schema = "future", Lazy = true)]
	public class User : ActiveRecordLinqBase<User>, IEnablable
	{
		public User()
		{
		}

		public User(Client client)
		{
			if (HttpContext.Current != null) // дл€ поддержки авт. тестировани€
				Registrant = SecurityContext.Administrator.UserName;

			RegistrationDate = DateTime.Now;
			Enabled = true;
			Client = client;
			Payer = client.Payer;
			SendRejects = true;
			SendWaybills = true;
		}

		[PrimaryKey(PrimaryKeyType.Native)]
		public virtual uint Id { get; set; }

		[Property(NotNull = true)]
		public virtual string Login { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

		[Property]
		public virtual bool SubmitOrders { get; set; }

		[Property]
		public virtual bool SendWaybills { get; set; }

		[Property]
		public virtual bool SendRejects { get; set; }

		[Property]
		public virtual bool EnableUpdate { get; set; }

		[Property]
		public virtual bool Auditor { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual ulong WorkRegionMask { get; set; }

		[Property]
		public virtual ulong OrderRegionMask { get; set; }

		[Property(Column = "Free")]
		public virtual bool IsFree { get; set; }

		[BelongsTo("ClientId", NotNull = true, Lazy = FetchWhen.OnInvoke)]
		public virtual Client Client { get; set; }

		[BelongsTo("ContactGroupId", Lazy = FetchWhen.OnInvoke)]
		public virtual ContactGroup ContactGroup { get; set; }

		[BelongsTo("InheritPricesFrom", Lazy = FetchWhen.OnInvoke)]
		public virtual User InheritPricesFrom { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }

		[OneToOne]
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
				throw new LoginNotFoundException(String.Format("ѕользователь {0} не найден", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword("«апрещено измен€ть пароль дл€ пользователей из офиса.");
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
			Save();
			Login = Id.ToString();
			Update();

			if (Client.Users == null)
				Client.Users = new List<User>();
			Accounting = new UserAccounting(this);
			Client.Users.Add(this);
			Client.UpdateBeAccounted();
			new AuthorizationLogEntity(Id).Create();
			new UserUpdateInfo(Id).Create();
		}

		public virtual void Setup(Client client)
		{
			Client = client;
			Payer = client.Payer;
			Setup();
			AddPrices(client);
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
		/// ѕользователь считаетс€ активным, если он получал обновление не более 7 дней назад
		/// </summary>
		public virtual bool IsActive
		{
			get
			{
				var log = AuthorizationLogEntity.TryFind(Id);
				return ((log != null) && (log.AFTime.HasValue) && (DateTime.Now.Subtract(log.AFTime.Value).Days <= 7));
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
		
		public virtual void MoveToAnotherClient(Client newOwner)
		{
			using (var scope = new TransactionScope()) 
			{
				var regions = Region.FindAll();
				// ≈сли маски регионов не совпадают, добавл€ем записи в UserPrices дл€ тех регионов,
				// которых не было у старого клиента, но они есть у нового клиента
				if (Client.MaskRegion != newOwner.MaskRegion)
				{
					foreach (var region in regions)
					{
						// ≈сли этот регион есть у старого клиента, пропускаем его
						if ((region.Id & Client.MaskRegion) > 0)
							continue;
						// ≈сли региона нет у старого клиента, но он есть у нового,
						// и дл€ этого пользовател€ нет прайсов в этом регионе добавл€ем прайсы дл€ этого региона
						if ((region.Id & newOwner.MaskRegion) > 0)
						{
							if (!HavePricesInRegion(region))
								AddPrices(newOwner, region);
						}
					}
				}
				Client = newOwner;
				Update();
				scope.VoteCommit();
			}
		}
	}

	public static class UserExtension
	{
		public static IEnumerable<User> SortBy(this IEnumerable<User> users, string columnName, bool descending)
		{
			if (columnName.Equals("Id", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return users.OrderByDescending(user => user.Id).ToList();
				return users.OrderBy(user => user.Id).ToList();
			}
			if (columnName.Equals("Login", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return users.OrderByDescending(user => user.Login).ToList();
				return users.OrderBy(user => user.Login).ToList();
			}
			return users.OrderBy(user => user.Id).ToList();
		}
	}
}