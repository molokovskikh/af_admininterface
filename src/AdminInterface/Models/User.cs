	using System;
using System.Collections.Generic;
using System.IO;
	using System.Linq;
	using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using AdminInterface.Security;
using System.Web;
using Common.Web.Ui.Models;
using Common.Web.Ui.Controllers;

namespace AdminInterface.Models
{
	public class LoginNotFoundException : Exception
	{
		public LoginNotFoundException(string message) : base(message) { }
	}

	public class CantChangePassword : Exception
	{
		public CantChangePassword(string message) : base(message) { }
	}

	[ActiveRecord("Users", Schema = "future", Lazy = true)]
	public class User : ActiveRecordBase<User>
	{
		public User()
		{
			if (HttpContext.Current != null) // ��� ��������� ���. ������������
				Registrant = SecurityContext.Administrator.UserName;
			RegistrationDate = DateTime.Now;
		}

		[PrimaryKey]
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

		[BelongsTo("ContactGroupId")]
		public virtual ContactGroup ContactGroup { get; set; }

		[BelongsTo("InheritPricesFrom", Lazy = FetchWhen.OnInvoke)]
		public virtual User InheritPricesFrom { get; set; }

		[HasAndBelongsToMany(typeof (UserPermission),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "usersettings.AssignedPermissions",
			ColumnRef = "PermissionId")]
		public virtual IList<UserPermission> AssignedPermissions { get; set; }

		[HasAndBelongsToMany(typeof (Address),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "future.UserAddresses",
			ColumnRef = "AddressId")]
		public virtual IList<Address> AvaliableAddresses { get; set; }

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
				throw new LoginNotFoundException(String.Format("������������ {0} �� ������", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword("��������� �������� ������ ��� ������������� �� �����.");
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
			new AuthorizationLogEntity(Id).Create();
			new UserUpdateInfo(Id).Create();
		}

		public virtual void Setup(Client client)
		{
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

		/// <summary>
		/// ������������ ��������� ��������, ���� �� ������� ���������� �� ����� 7 ���� �����
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
			if (persons.Length == 0)
				return;
			if (ContactGroup == null)
				AddContactGroup();
			ContactGroup.UpdatePersons(persons);
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
where u.Id = :UserId";

			ArHelper.WithSession(session => session.CreateSQLQuery(sql)
				.SetParameter("UserId", Id)
				.SetParameter("ClientId", client.Id)
				.ExecuteUpdate());
		}

		public virtual void AddContactPerson(string name)
		{
			if (String.IsNullOrEmpty(name))
				return;
			var groupOwner = Client.ContactGroupOwner;
			var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
			group.Save();
			group.AddPerson(name);
			ContactGroup = group;
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