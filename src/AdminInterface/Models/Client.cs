using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	public enum ClientType
	{
		[Description("Поставщик")] Supplier = 0,
		[Description("Аптека")] Drugstore = 1,
	}

	public enum ClientStatus
	{
		[Description("Отключен")] Off = 0,
		[Description("Включен")] On = 1,
	}

	public class RegistrationInfo
	{
		public RegistrationInfo()
		{}

		public RegistrationInfo(Administrator administrator)
		{
			Registrant = administrator.UserName;
			RegistrationDate = DateTime.Now;
		}

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		public virtual Administrator GetRegistrant()
		{
			if (String.IsNullOrEmpty(Registrant))
				return null;
			
			return Administrator.GetByName(Registrant);
		}
	}

	[ActiveRecord(Schema = "Customers", Lazy = true), Auditable]
	public class Client : Service
	{
		private ClientStatus _status;

		public Client()
		{
			Type = ServiceType.Drugstore;
			Settings = new DrugstoreSettings(this);
			Registration = new RegistrationInfo();
			Payers = new List<Payer>();
			Users = new List<User>();
			Addresses = new List<Address>();
		}

		public Client(Payer payer, Region homeRegion)
			: this()
		{
			Status = ClientStatus.On;
			HomeRegion = homeRegion;
			JoinPayer(payer);
			Settings.CheckDefaults();
		}

		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property, Description("Краткое наименование"), Auditable, Notify, ValidateNonEmpty]
		public override string Name { get; set; }

		[Property, Description("Полное наименование"), Auditable, Notify, ValidateNonEmpty]
		public virtual string FullName { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Description("Включен"), Auditable]
		public virtual ClientStatus Status
		{
			get
			{
				return _status;
			}

			set
			{
				var updatePayer = _status != value;
				_status = value;
				_disabled = _status == ClientStatus.Off;
				if (updatePayer)
				{
					foreach (var payer in Payers)
						payer.PaymentSum = payer.TotalSum;
				}
			}
		}

		[Style]
		public override bool Disabled
		{
			get
			{
				return _disabled;
			}
			set
			{
				var updatePayer = _disabled != value;
				_disabled = value;
				_status = _disabled ? ClientStatus.Off : ClientStatus.On;

				if (updatePayer)
				{
					foreach (var payer in Payers)
						payer.PaymentSum = payer.TotalSum;
				}
			}
		}

		[Property, Description("Регионы работы"), ValidateGreaterThanZero("Вы не выбрали регионы работы"), Auditable]
		public virtual UInt64 MaskRegion { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set;}

		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual DrugstoreSettings Settings { get; set; }

		[BelongsTo("ContactGroupOwnerId", Lazy = FetchWhen.OnInvoke, Cascade = CascadeEnum.All)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[BelongsTo("RegionCode"), Description("Домашний регион"), Auditable]
		public override Region HomeRegion { get; set; }

		[HasMany(ColumnKey = "ClientId", Lazy = true, Inverse = true, OrderBy = "Address", Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(ColumnKey = "ClientId", Inverse = true, Lazy = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		[HasAndBelongsToMany(typeof (Payer),
			Lazy = true,
			ColumnKey = "ClientId",
			Table = "PayerClients",
			Schema = "Billing",
			ColumnRef = "PayerId")]
		public virtual IList<Payer>  Payers { get; set; }

		public override bool Enabled
		{
			get { return Status == ClientStatus.On; }
		}

		public virtual bool CanChangePayer
		{
			get
			{
				return Payers.Count == 1 && Payers[0].JuridicalOrganizations.Count == 1;
			}
		}

		public static Client FindClietnForBilling(uint clientCode)
		{
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof (Client))
					.Add(Restrictions.Eq("Id", clientCode))
					.SetFetchMode("BillingInstance", FetchMode.Join)
					.SetFetchMode("HomeRegion", FetchMode.Join)
					.SetFetchMode("ContactGroupOwner", FetchMode.Join)
					.UniqueResult<Client>());
		}

		public virtual ContactGroup GetContactGroup(ContactGroupType type)
		{
			return ContactGroupOwner.ContactGroups.FirstOrDefault(contactGroup => contactGroup.Type == type);
		}

		public virtual void ResetUin()
		{
			ArHelper.WithSession(session =>
				session.CreateSQLQuery(@"
update usersettings.UserUpdateInfo uui
	join Customers.Users u on uui.UserId = u.Id
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
	join Customers.Users u on uui.UserId = u.Id
where u.ClientId = :clientcode
group by u.ClientId")
					.SetParameter("clientcode", Id)
					.UniqueResult<long?>());

			return result != null && result.Value == 0;
		}

		public virtual int EnabledUserForPayerCount(Payer payer)
		{
			return Users.Count(u => u.Payer == payer && u.Enabled);
		}

		public virtual int DisabledUserForPayerCount(Payer payer)
		{
			return Users.Count(u => u.Payer == payer && !u.Enabled);
		}

		public virtual int EnabledAddressForPayerCount(Payer payer)
		{
			return Addresses.Count(a => a.Payer == payer && a.Enabled);
		}

		public virtual int DisabledAddressForPayerCount(Payer payer)
		{
			return Addresses.Count(a => a.Payer == payer && !a.Enabled);
		}

		public virtual bool HaveLockedUsers()
		{
			return Users.Any(u => ADHelper.IsLoginExists(u.Login) && ADHelper.IsLocked(u.Login));
		}

		public virtual void MaintainIntersection()
		{
			foreach (var legalEntity in Orgs())
				Maintainer.MaintainIntersection(this, legalEntity);
		}

		public virtual IEnumerable<LegalEntity> Orgs()
		{
			return Payers.SelectMany(p => p.JuridicalOrganizations);
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
			{
				address.LegalEntity = Orgs().Single();
				address.Payer = address.LegalEntity.Payer;
			}
			if (address.Payer == null)
				address.Payer = address.LegalEntity.Payer;

			address.Registration = new RegistrationInfo(SecurityContext.Administrator);
			address.Client = this;
			address.Enabled = true;
			Addresses.Add(address);

			if (address.Accounting == null)
				address.Accounting = new AddressAccount(address);
			return address;
		}

		public virtual bool ShouldSendNotification()
		{
			return !Settings.ServiceClient && Settings.InvisibleOnFirm == DrugstoreType.Standart
				&& Payers.All(p => p.Id != 921)
				&& Addresses.Count > 0
				&& Enabled;
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

		public static Client Find(uint id)
		{
			return ActiveRecordBase<Client>.Find(id);
		}

		public virtual void Save()
		{
			ActiveRecordMediator<Client>.Save(this);
		}

		public virtual void SaveAndFlush()
		{
			ActiveRecordMediator.SaveAndFlush(this);
		}

		public virtual void Refresh()
		{
			ActiveRecordMediator<Client>.Refresh(this);
		}

		public static IOrderedQueryable<Client> Queryable
		{
			get
			{
				return ActiveRecordLinqBase<Client>.Queryable;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual void AddBillingComment(string billingMessage)
		{
			if (String.IsNullOrEmpty(billingMessage))
				return;

			new ClientInfoLogEntity("Сообщение в биллинг: " + billingMessage, this).Save();
			var user = Users.First();
			billingMessage = String.Format("О регистрации клиента: {0} ( {1} ), пользователь: {2} ( {3} ): {4}", Id, Name, user.Id, user.Name, billingMessage);
			Payers.Single().AddComment(billingMessage);
		}

		public virtual void JoinPayer(Payer payer)
		{
			Payers.Add(payer);
		}

		public virtual User AddUser(string name)
		{
			var user = new User(this) {Name = name};
			AddUser(user);
			user.Setup();
			return user;
		}

		public override void AddUser(User user)
		{
			if (user.Payer == null) {
				if (Payers.Count > 1)
					throw new Exception(String.Format("У клиента более одного плательщика {0}", Payers.Implode()));
				user.Payer = Payers.Single();
			}

			if (!user.Payer.Users.Contains(user))
				user.Payer.Users.Add(user);

			if (!Users.Contains(user))
				Users.Add(user);
		}

		public virtual string GetEmailsForBilling()
		{
			return ContactGroupOwner
				.GetEmails(ContactGroupType.Billing)
				.Implode();
		}

		public virtual void ChangePayer(Payer payer, LegalEntity org)
		{
			var oldPayers = Payers.ToArray();
			Payers.Clear();
			Payers.Add(payer);
			foreach (var user in Users)
			{
				user.Payer.Users.Remove(user);
				user.Payer = payer;
				user.Payer.Users.Add(user);
			}

			foreach (var address in Addresses)
			{
				address.Payer.Addresses.Remove(address);
				address.Payer = payer;
				address.LegalEntity = org;
				address.Payer.Addresses.Add(address);
			}

			ArHelper.WithSession(s => s.CreateSQLQuery(@"
update Customers.intersection
set LegalEntityId = :orgId
where ClientId = :clientId")
				.SetParameter("clientId", Id)
				.SetParameter("orgId", org.Id)
				.ExecuteUpdate());

			payer.UpdatePaymentSum();
			foreach (var oldPayer in oldPayers)
				oldPayer.UpdatePaymentSum();
		}

		public virtual void ChangeHomeRegion(Region region)
		{
			HomeRegion = region;
			MaskRegion = region.Id;
			Settings.WorkRegionMask = region.Id;
			Settings.OrderRegionMask = region.Id;
		}

		public virtual bool CanDelete()
		{
			return ClientOrder.Queryable.Count(o => o.Client == this) == 0
				&& Addresses.All(a => a.CanDelete())
				&& Users.All(u => u.CanDelete());
		}

		public virtual void Delete()
		{
			foreach (var user in Users.ToArray()) {
				user.Delete();
			}

			foreach (var address in Addresses.ToArray()) {
				address.Delete();
			}

			var payers = Payers.ToArray();
			Payers.Clear();
			foreach (var payer in payers) {
				//какая то фигня с загрузкой объектов
				payer.Clients.Remove(payer.Clients.First(c => c.Id == Id));
				if (payer.CanDelete())
					payer.Delete();
				else
					payer.UpdatePaymentSum();
			}


			var rule = Settings.SmartOrderRules;
			if (rule != null) {
				var overHaveSameSettins = ActiveRecordLinqBase<DrugstoreSettings>.Queryable.Any(s => s.Id != Id && s.SmartOrderRules == rule);
				if (!overHaveSameSettins) {
					Settings.SmartOrderRules = null;
					rule.Delete();
				}
			}
			ClientInfoLogEntity.DeleteAuditRecords(this);
			ActiveRecordMediator.Delete(this);
		}

		public virtual IEnumerable<ModelAction> Actions
		{
			get
			{
				return new [] {
					new ModelAction(this, "Delete", "Удалить", !CanDelete())
				};
			}
		}

		public virtual IList<LegalEntity> GetLegalEntity()
		{
			return Payers.SelectMany(p => p.JuridicalOrganizations).ToList();
		}
	}
}
