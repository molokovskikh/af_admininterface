using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Tools;
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

	[ActiveRecord("PricesData", Schema = "Usersettings", Lazy = true)]
	public class Price : ActiveRecordLinqBase<Price>
	{
		[PrimaryKey("PriceCode")]
		public virtual uint Id { get; set; }

		[Property("PriceName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual int? PriceType { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }
	}

	[ActiveRecord("ClientsData", Schema = "Usersettings", Where = "(FirmType = 0)", Lazy = true)]
	public class Supplier : ActiveRecordLinqBase<Supplier>
	{
		[PrimaryKey("FirmCode")]
		public virtual uint Id { get; set;}

		[Property("ShortName", NotNull = true)]
		public virtual string Name { get; set; }

		[Property("FirmStatus")]
		public virtual ClientStatus Status { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region HomeRegion { get; set; }

		[BelongsTo("BillingCode")]
		public virtual Payer Payer { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true)]
		public virtual IList<Price> Prices { get; set; }

		public static IList<Supplier> GetByPayerId(uint payerId)
		{
			return Queryable.Where(p => p.Payer.PayerID == payerId).OrderBy(s => s.Name).ToList();
		}

		public override string ToString()
		{
			return Name;
		}
	}

	[ActiveRecord("Clients", Schema = "Future", Lazy = true)]
	public class Client : ActiveRecordLinqBase<Client>, IEnablable
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, Description("Краткое наименование"), Auditable]
		public virtual string Name { get; set; }

		[Property, Description("Полное наименование"), Auditable]
		public virtual string FullName { get; set; }

		[Property, Description("Включен"), Auditable]
		public virtual ClientStatus Status { get; set; }

		[Property("FirmType")]
		public virtual ClientType Type { get; set; }

		[Property]
		public virtual Segment Segment { get; set; }

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property, Description("Регионы работы")]
		public virtual UInt64 MaskRegion { get; set; }

		[OneToOne]
		public virtual DrugstoreSettings Settings { get; set; }

		[BelongsTo("ContactGroupOwnerId")]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }

		[BelongsTo("RegionCode"), Description("Домашний регион"), Auditable]
		public virtual Region HomeRegion { get; set; }

		[HasMany(ColumnKey = "ClientId", Lazy = true, Inverse = true, OrderBy = "Address", Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(ColumnKey = "ClientId", Inverse = true, Lazy = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		public virtual bool Enabled
		{
			get { return Status == ClientStatus.On; }
			set { Status = value ? ClientStatus.On : ClientStatus.Off; }
		}

		public virtual bool IsHiddenForProducer
		{
			get
			{
				if (!IsDrugstore())
					return false;
				return (Settings.InvisibleOnFirm == DrugstoreType.Hidden);
			}
			set
			{
				if (!IsDrugstore())
					return;
				var val = value ? 2 : 0;
				var tmp = Settings.InvisibleOnFirm == DrugstoreType.Hidden;
				if (tmp != value)
				{
					Settings.InvisibleOnFirm = DrugstoreType.Standart;
					var updateSql = @"
update 
	intersection, pricesdata 
set 
	intersection.invisibleonfirm = :InvisibleOnFirm";
					if (value)
					{
						Settings.InvisibleOnFirm = DrugstoreType.Hidden;
						updateSql += ", DisabledByFirm = if(PriceType = 2, 1, 0), InvisibleOnClient = if(PriceType = 2, 1, 0)";
					}
					updateSql += @"
where 
	intersection.pricecode = pricesdata.pricecode and 
	intersection.clientcode = :ClientCode";

					Settings.Update();

					ArHelper.WithSession(session => session.CreateSQLQuery(updateSql)
						.SetParameter("ClientCode", Id)
						.SetParameter("InvisibleOnFirm", val)
						.ExecuteUpdate());
				}
			}
		}

		public virtual bool IsDrugstore()
		{
			return Type == ClientType.Drugstore;
		}

		public virtual bool IsClientActive()
		{
			return Status == ClientStatus.On;
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
			contactGroups = contactGroups.Where(g => g != null).ToArray();
			foreach (var contactGroup in contactGroups)
				foreach (var contact in contactGroup.Contacts)
					if (contact.Type == ContactType.Email && !emails.Contains(contact.ContactText.Trim()))
						emails.Add(contact.ContactText.Trim());
		}

		private string Build(ContactGroup generalGroup, params ContactGroup[] specialGroup)
		{
			return GetEmails(false, generalGroup, specialGroup);
		}

		public virtual string GetEmailsForBilling()
		{
			return GetEmails(true, GetContactGroup(ContactGroupType.Billing), Payer.ContactGroupOwner.ContactGroups.ToArray());
		}

		private string GetEmails(bool unionEmails, ContactGroup generalGroup, params ContactGroup[] specialGroup)
		{
			specialGroup = specialGroup.Where(g => g != null && g.Persons != null).ToArray();
			var emails = new List<string>();
			foreach (var person in specialGroup.SelectMany(g => g.Persons))
				ProcessEmails(emails, person);

			ProcessEmails(emails, specialGroup);

			if ((emails.Count > 0) && !unionEmails)
				return String.Join(", ", emails.ToArray());

			if (generalGroup != null && generalGroup.Persons != null)
				foreach (var person in generalGroup.Persons)
					ProcessEmails(emails, person);

			ProcessEmails(emails, generalGroup);

			return String.Join(", ", emails.ToArray());
		}

		public virtual void ResetUin()
		{
			ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
update usersettings.UserUpdateInfo uui
	join Future.Users u on uui.UserId = u.Id
set uui.AFCopyId = '' 
where u.ClientId = :clientcode")
					.SetParameter("clientcode", Id)
					.ExecuteUpdate());
		}

		public virtual bool HaveUin()
		{
			var result = ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
select sum(length(concat(uui.AFCopyId))) = 0
from usersettings.UserUpdateInfo uui
	join Future.Users u on uui.UserId = u.Id
where u.ClientId = :clientcode
group by u.ClientId")
					.SetParameter("clientcode", Id)
					.UniqueResult<long?>());

			return result != null && result.Value == 0;
		}

		public virtual bool HavePreparedData()
		{
			foreach (var user in Users)
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

		public virtual string GetHumanReadableType()
		{
			return BindingHelper.GetDescription(Type);
		}

		public virtual void UpdateBeAccounted()
		{
			if (Addresses == null)
				Addresses = new List<Address>();
			if (Users == null)
				Users = new List<User>();

			var userCount = Users.Count(user => user.Enabled && !user.IsFree);
			var index = 0;
			foreach (var address in Addresses)
			{
				if (!address.Enabled || address.IsFree)
					address.Accounting.ReadyForAcounting = false;
				else
					address.Accounting.ReadyForAcounting = index++ >= userCount;
			}
		}

		public virtual void MaintainIntersection()
		{
			foreach (var legalEntity in Payer.JuridicalOrganizations)
			{
				Maintainer.MaintainIntersection(this, legalEntity);
			}
		}

		public virtual Address AddAddress(string address)
		{
			return AddAddress(new Address {Value = address});
		}

		public virtual Address AddAddress(Address address)
		{
			if (Addresses == null)
				Addresses = new List<Address>();
			if (address.LegalEntity == null)
				address.LegalEntity = Payer.JuridicalOrganizations.Single();
			if (address.Accounting == null)
				address.Accounting = new AddressAccounting(address);

			address.Payer = Payer;
			address.Client = this;
			address.Enabled = true;
			Addresses.Add(address);

			UpdateBeAccounted();
			return address;
		}

		public virtual bool ShouldSendNotification()
		{
			return !Settings.ServiceClient && Settings.InvisibleOnFirm == DrugstoreType.Standart && Payer.Id != 921;
		}

		public virtual void UpdateRegionSettings(RegionSettings[] regionSettings)
		{
			foreach (var setting in regionSettings)
			{
				if (setting.IsAvaliableForBrowse)
				{
					if ((MaskRegion & setting.Id) == 0)
					{
						MaskRegion |= setting.Id;
						Users.Each(u => u.WorkRegionMask |= setting.Id);
					}
					if ((Settings.WorkRegionMask & setting.Id) == 0)
					{
						Settings.WorkRegionMask |= setting.Id;
					}
				}
				else
				{
					MaskRegion &= ~setting.Id;
					Settings.WorkRegionMask &= ~setting.Id;
					Users.Each(u => u.WorkRegionMask &= ~setting.Id);
				}
				if (setting.IsAvaliableForOrder)
				{
					if ((Settings.OrderRegionMask & setting.Id) == 0)
					{
						Settings.OrderRegionMask |= setting.Id;
						Users.Each(u => u.OrderRegionMask |= setting.Id);
					}
				}
				else
				{
					Settings.OrderRegionMask &= ~setting.Id;
					Users.Each(u => u.OrderRegionMask &= ~setting.Id);
				}
			}
		}

		public virtual object GetRegistrant()
		{
			if (String.IsNullOrEmpty(Registrant))
				return null;

			return Administrator.GetByName(Registrant);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}