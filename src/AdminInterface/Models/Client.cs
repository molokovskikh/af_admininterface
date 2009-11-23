using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	public enum ClientStatus
	{
		[Description("Отключен")] Off = 0,
		[Description("Включен")] On = 1,
	}

	public enum ClientType
	{
		[Description("Поставщик")] Supplier = 0,
		[Description("Аптека")] Drugstore = 1,
	}

	public enum Segment
	{
		[Description("Опт")] Wholesale = 0,
		[Description("Розница")] Retail = 1,
	}

	[ActiveRecord("Future.Clients", Lazy = true)]
	public class Client : ActiveRecordBase<Client>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property]
		public virtual ClientStatus Status { get; set; }

		[Property("FirmType")]
		public virtual ClientType Type { get; set; }

		[Property]
		public virtual Segment Segment { get; set; }

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual UInt64 MaskRegion { get; set; }

		[BelongsTo("ContactGroupOwnerId")]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer BillingInstance { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region HomeRegion { get; set; }

		[HasMany(ColumnKey = "ClientId", Lazy = true, Inverse = true, OrderBy = "Address", Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(ColumnKey = "ClientId", Inverse = true, Lazy = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		public virtual IEnumerable<User> GetUsers()
		{
			foreach (var user in Users)
				yield return user;
		}

		public virtual bool IsDrugstore()
		{
			return Type == ClientType.Drugstore;
		}

		public virtual bool IsClientActive()
		{
			return Status == ClientStatus.On;
		}

		public virtual float GetPayment(IList<Tariff> tariffs)
		{
			var tariff = tariffs.FirstOrDefault(t => t.Region.Id == HomeRegion.Id);

			if (tariff == null)
				return 0;

			return tariff.Pay;
		}

		public static Client FindAndCheck(uint clientCode)
		{
			var client = Find(clientCode);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);
			return client;
		}

		public static Client FindClietnForBilling(uint clientCode)
		{
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof (Client))
					.Add(Expression.Eq("Id", clientCode))
					.SetFetchMode("BillingInstance", FetchMode.Join)
					//.SetFetchMode("Payer.Clients", FetchMode.Eager)
					.SetFetchMode("HomeRegion", FetchMode.Join)
					.SetFetchMode("ContactGroupOwner", FetchMode.Join)
					//.SetFetchMode("ContactGroupOwner.ContactGroups", FetchMode.Eager)
					//.SetFetchMode("ContactGroupOwner.ContactGroups.Contacts", FetchMode.Eager)
					//.SetFetchMode("ContactGroupOwner.ContactGroups.Persons", FetchMode.Eager)
					.UniqueResult<Client>());
		}

		public virtual string GetAddressForSendingClientCard()
		{
			if (Type == ClientType.Drugstore)
				return Build(GetContactGroup(ContactGroupType.General),
				             GetContactGroup(ContactGroupType.OrderManagers));

			return Build(GetContactGroup(ContactGroupType.General),
			             GetContactGroup(ContactGroupType.OrderManagers),
			             GetContactGroup(ContactGroupType.ClientManagers));
		}

		public virtual ContactGroup GetContactGroup(ContactGroupType type)
		{
			foreach (var contactGroup in ContactGroupOwner.ContactGroups)
				if (contactGroup.Type == type)
					return contactGroup;
			return null;
		}

		public virtual void ProcessEmails(List<string> emails, params ContactOwner[] contactGroups)
		{
			foreach (var contactGroup in contactGroups)
				foreach (var contact in contactGroup.Contacts)
					if (contact.Type == ContactType.Email && !emails.Contains(contact.ContactText.Trim()))
						emails.Add(contact.ContactText.Trim());
		}

		private string Build(ContactGroup generalGroup, params ContactGroup[] specialGroup)
		{
			var emails = new List<string>();
			foreach (var contactGroup in specialGroup)
			{
				if (contactGroup.Persons != null)
				{
					foreach (var person in contactGroup.Persons)
						ProcessEmails(emails, person);
				}
			}

			ProcessEmails(emails, specialGroup);

			if (emails.Count > 0)
				return String.Join(", ", emails.ToArray());

			if (generalGroup.Persons != null)
			{
				foreach (var person in generalGroup.Persons)
					ProcessEmails(emails, person);
			}

			ProcessEmails(emails, generalGroup);
			
			return String.Join(", ", emails.ToArray());
		}

		public virtual void ResetUin()
		{
			ArHelper.WithSession<Client>(session =>
				session.CreateSQLQuery(@"
update usersettings.UserUpdateInfo uui
	join usersettings.OsUserAccessRight ouar on uui.UserId = ouar.RowId
set uui.AFCopyId = '' 
where ouar.clientcode = :clientcode")
					.SetParameter("clientcode", Id)
					.ExecuteUpdate());
		}

		public virtual bool HaveUin()
		{
			var result = ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
select sum(length(concat(uui.AFCopyId))) = 0
from usersettings.UserUpdateInfo uui
	join usersettings.OsUserAccessRight ouar on uui.UserId = ouar.RowId
where ouar.clientcode = :clientcode
group by ouar.clientcode")
					.SetParameter("clientcode", Id)
					.UniqueResult<long?>());

			return result != null && result.Value == 0;
		}

		public virtual bool HavePreparedData()
		{
			foreach (var user in GetUsers())
			{
				var file = String.Format(@"U:\wwwroot\ios\Results\{0}.zip", user.Id);
				if (File.Exists(file))
					return true;
			}
			return false;
		}

		public virtual int? WorkCopyCount()
		{
			if (Type == ClientType.Drugstore)
				return Users.Count;
			return null;
		}

		public virtual bool HaveLockedUsers()
		{
			return Users.Any(u => ADHelper.IsLoginExists(u.Login) && ADHelper.IsLocked(u.Login));
		}

		public virtual void AddDeliveryAddress(string address)
		{
			if (Addresses == null)
				Addresses = new List<Address>();
			var delivery = new Address {Value = address};
			delivery.Client = this;
			Addresses.Add(delivery);
		}

		public virtual string GetHumanReadableType()
		{
			return BindingHelper.GetDescription(Type);
		}
	}
}