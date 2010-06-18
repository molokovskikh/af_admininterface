using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
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

	[ActiveRecord("Usersettings.ClientsData", Where = "(FirmType = 0)")]
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
		public virtual Payer BillingInstance { get; set; }

		public static IList<Supplier> GetByPayerId(uint payerId)
		{
			var ids = ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT FirmCode
FROM usersettings.ClientsData
WHERE FIrmType = 0 AND BillingCode = :PayerId")
						   .SetParameter("PayerId", payerId)
						   .List());
			var suppliers = new List<Supplier>(ids.Count);
			foreach (var id in ids)
				suppliers.Add(Supplier.Find(id));
			return suppliers;
		}
	}

	[ActiveRecord("Clients", Schema = "Future", Lazy = true)]
	public class Client : ActiveRecordLinqBase<Client>
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

		public virtual bool IsHiddenForProducer
		{
			get
			{
				if (!IsDrugstore())
					return false;
				var drugstore = DrugstoreSettings.Find(Id);
				return (drugstore.InvisibleOnFirm == DrugstoreType.Hidden);
			}
			set
			{
				if (!IsDrugstore())
					return;
                var val = value ? 2 : 0;
				var drugstore = DrugstoreSettings.Find(Id);
				var tmp = drugstore.InvisibleOnFirm == DrugstoreType.Hidden;				
				if (tmp != value)
				{
					drugstore.InvisibleOnFirm = DrugstoreType.Standart;
					var updateSql = @"
update 
	intersection, pricesdata 
set 
	intersection.invisibleonfirm = :InvisibleOnFirm";
					if (value)
					{
						drugstore.InvisibleOnFirm = DrugstoreType.Hidden;
						updateSql += ", DisabledByFirm = if(PriceType = 2, 1, 0), InvisibleOnClient = if(PriceType = 2, 1, 0)";
					}
					updateSql += @"
where 
	intersection.pricecode = pricesdata.pricecode and 
	intersection.clientcode = :ClientCode";

					drugstore.Update();

					ArHelper.WithSession(session => session.CreateSQLQuery(updateSql)
						.SetParameter("ClientCode", Id)
						.SetParameter("InvisibleOnFirm", val)
						.ExecuteUpdate());
				}
			}
		}

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

		public virtual string GetEmails()
		{
			return GetEmails(true, GetContactGroup(ContactGroupType.General), GetContactGroup(ContactGroupType.OrderManagers));
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
			var delivery = new Address {Value = address, Enabled = true};
			delivery.Client = this;
			Addresses.Add(delivery);
		}

		public virtual string GetHumanReadableType()
		{
			return BindingHelper.GetDescription(Type);
		}

        public virtual void UpdateBeAccounted()
        {
            var users = Users.Where(user => user.Enabled && !user.IsFree);
            var index = 0;
            foreach (var address in Addresses)
            {
                if (!address.Enabled || address.IsFree)
                    address.BeAccounted = false;
                else
                    address.BeAccounted = index++ >= users.Count();
                address.UpdateAndFlush();
            }
        }

	    public virtual void MaintainIntersection()
		{
			ArHelper.WithSession(
				s => {
					var reslt = s.CreateSQLQuery(
							@"
DROP TEMPORARY TABLE IF EXISTS TempIntersection;
CREATE TEMPORARY TABLE TempIntersection
(
ClientId int unsigned,
RegionId BIGINT(20),
PriceId int unsigned,
CostId int unsigned,
AvailableForClient int unsigned
) engine=MEMORY;
INSERT 
INTO TempIntersection
SELECT  DISTINCT drugstore.Id,
        regions.regioncode,
        pricesdata.pricecode,
        (
          SELECT costcode
          FROM    pricescosts pcc
          WHERE   basecost
                  AND pcc.PriceCode = pricesdata.PriceCode
        ) as CostCode,
		if(pricesdata.PriceType = 0, 1, 0) as AvailableForClient
FROM Future.Clients as drugstore
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.Segment
		JOIN pricesdata ON pricesdata.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pricesdata.pricecode AND i.RegionId = regions.regioncode AND i.ClientId = drugstore.Id
WHERE i.Id IS NULL
	AND supplier.firmtype = 0
	AND drugstore.Id = :clientId
	AND drugstore.FirmType = 1;

INSERT
INTO Future.Intersection (
	ClientId,
	RegionId,
	PriceId,
	CostId,
	AvailableForClient
)
SELECT ClientId,
	RegionId,
	PriceId,
	CostId,
	AvailableForClient
FROM TempIntersection;

DROP TEMPORARY TABLE IF EXISTS TempIntersection;
")
							.SetParameter("clientId", Id)
							.ExecuteUpdate();
					reslt++;
				});
		}
	}
}