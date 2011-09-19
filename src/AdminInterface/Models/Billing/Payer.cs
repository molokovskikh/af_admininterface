using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	public enum DiscountType
	{
		[Description("В рублях")] Currency,
		[Description("В процентах")] Percent,
	}

	public enum InvoiceType
	{
		[Description("Вручную")] Manual = 0,
		[Description("Автоматически")] Auto = 1
	}

	public enum InvoicePeriod
	{
		[Description("Помесячно")] Month,
		[Description("Поквартально")] Quarter
	}

	public class InvoiceSettings
	{
		[Property]
		public virtual bool EmailInvoice { get; set; }

		[Property]
		public virtual bool PrintInvoice { get; set; }

		[Property]
		public virtual bool DocumentsOnLastWorkingDay { get; set; }

		[Property]
		public virtual bool DoNotGroupParts { get; set; }
	}

	[ActiveRecord(Schema = "billing", Lazy = true)]
	public class Payer : ActiveRecordValidationBase<Payer>
	{
		public Payer(string name)
			: this(name, name)
		{}

		public Payer(string name, string fullname)
			: this()
		{
			Name = name;
			JuridicalName = fullname;
			ContactGroupOwner = new ContactGroupOwner();
			JuridicalOrganizations.Add(new LegalEntity(name, JuridicalName, this));
			OldTariff = 0;
			OldPayDate = DateTime.Now;
			Comment = String.Format("Дата регистрации: {0}", DateTime.Now);
		}

		public Payer()
		{
			JuridicalOrganizations = new List<LegalEntity>();
			InvoiceSettings = new InvoiceSettings();
			Users = new List<User>();
			Clients = new List<Client>();
			Addresses = new List<Address>();
			Suppliers = new List<Supplier>();
			Ads = new List<Advertising>();
		}

		[PrimaryKey]
		public virtual uint PayerID { get; set; }

		public virtual uint Id
		{
			get { return PayerID; }
			set { PayerID = value; }
		}

		[Property("ShortName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual string JuridicalName { get; set; }

		[Property]
		public virtual string Customer { get; set; }

		[Property]
		public virtual string JuridicalAddress { get; set; }

		[Property]
		public virtual string ReceiverAddress { get; set; }

		[Property]
		[ValidateRegExp("", "КПП должен содержать 9 цифр")]
		public virtual string KPP { get; set; }

		[Property]
		[ValidateRegExp("", "ИНН может содержать 10 или 12 цифр")]
		public virtual string INN { get; set; }

		[BelongsTo(Column = "ContactGroupOwnerId", Lazy = FetchWhen.OnInvoke, Cascade = CascadeEnum.SaveUpdate)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[Property]
		public virtual string ActualAddressCountry { get; set; }

		[Property]
		public virtual string ActualAddressIndex { get; set; }

		[Property]
		public virtual string ActualAddressProvince { get; set; }

		[Property]
		public virtual string ActualAddressTown { get; set; }

		[Property]
		public virtual string ActualAddressRegion { get; set; }

		[Property]
		public virtual string ActualAddressStreet { get; set; }

		[Property]
		public virtual string ActualAddressHouse { get; set; }

		[Property]
		public virtual string ActualAddressOffice { get; set; }

		[Property]
		public virtual string BeforeNamePrefix { get; set; }

		[Property]
		public virtual string AfterNamePrefix { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		[Property]
		public virtual string ChangeServiceNameTo { get; set; }

		[Property]
		public virtual InvoiceType AutoInvoice { get; set; }

		[Property]
		public virtual InvoicePeriod PayCycle { get; set; }

		[Property]
		public virtual DateTime OldPayDate { get; set;}

		[Property]
		public virtual double OldTariff { get; set; }

		[Property]
		public virtual bool HaveContract { get; set; }

		[Property]
		public virtual bool SendRegisteredLetter { get; set; }

		[Property]
		public virtual bool SendScannedDocuments { get; set; }

		[Property]
		public virtual decimal Balance { get; set; }

		[Property]
		public virtual decimal BeginBalance { get; set; }

		[Property]
		public virtual DateTime? BeginBalanceDate { get; set; }

		[Property(NotNull = true, Default = "0")]
		public virtual bool SendPaymentNotification { get; set; }

		[Nested]
		public virtual InvoiceSettings InvoiceSettings { get; set; }

		[BelongsTo("RecipientId")]
		public virtual Recipient Recipient { get; set; }

		[HasMany(typeof (User), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		[HasMany(typeof (Address), Lazy = true, Inverse = true, OrderBy = "Address")]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(typeof (Supplier), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<Supplier> Suppliers { get; set; }

		[HasMany(typeof(LegalEntity), Lazy = true, Inverse = true, Cascade = ManyRelationCascadeEnum.All, OrderBy = "Name")]
		public virtual IList<LegalEntity> JuridicalOrganizations { get; set; }

		[HasMany(typeof(Report), Lazy = true, Inverse = true, OrderBy = "Comment")]
		public virtual IList<Report> Reports { get; set; }

		[HasMany(typeof(Advertising), Lazy = true, Inverse = true, Cascade = ManyRelationCascadeEnum.SaveUpdate)]
		public virtual IList<Advertising> Ads { get; set; }

		[HasAndBelongsToMany(typeof (Client),
			Lazy = true,
			Inverse = true,
			ColumnKey = "PayerId",
			Table = "PayerClients",
			Schema = "Billing",
			ColumnRef = "ClientId")]
		public virtual IList<Client> Clients { get; set; }

		public virtual string GetMailAddress()
		{
			return String.Join(", ",
				new [] {
					!String.IsNullOrWhiteSpace(ActualAddressStreet) ? "ул. " + ActualAddressStreet : null,
					!String.IsNullOrWhiteSpace(ActualAddressHouse) ? "д. " + ActualAddressHouse: null,
					!String.IsNullOrWhiteSpace(ActualAddressOffice) ? "оф. " + ActualAddressOffice : null,
					!String.IsNullOrWhiteSpace(ActualAddressProvince) ? "обл. " + ActualAddressProvince : null,
					!String.IsNullOrWhiteSpace(ActualAddressTown) ? "г. " + ActualAddressTown : null,
				}.Where(s => !String.IsNullOrWhiteSpace(s)));
		}

		public virtual string ShortName
		{
			get { return Name; }
			set { Name = value; }
		}

		public virtual List<string> Emails()
		{
			return ContactGroupOwner.ContactGroups
				.SelectMany(c => c.Contacts)
				.Union(ContactGroupOwner.ContactGroups.SelectMany(c => c.Persons).SelectMany(p => p.Contacts))
				.Where(c => c.Type == ContactType.Email)
				.Select(c => c.ContactText)
				.Distinct()
				.ToList();
		}

		public virtual string GetNamePrefix()
		{
			if (String.IsNullOrWhiteSpace(BeforeNamePrefix))
				return "для Аптеки";
			return BeforeNamePrefix;
		}

		public virtual bool IsManualPayments()
		{
			return AutoInvoice == InvoiceType.Manual;
		}

		public static IEnumerable<Payer> GetLikeAvaliable(string searchPattern)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(Payer));
			var allowViewSuppliers = SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers);
			var allowViewDrugstore = SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore);
			try
			{
				var filter = String.Empty;
				if (!allowViewDrugstore)
					filter += " and c.FirmType <> 1 ";
				if (!allowViewSuppliers)
					filter += " and c.FirmType <> 0 ";
				var sql = @"
SELECT {Payer.*}
FROM billing.payers {Payer}
WHERE {Payer}.ShortName like :SearchText
and (exists(
		select * from Future.Clients c
			join Billing.PayerClients pc on pc.ClientId = c.Id
		where pc.PayerId = {Payer}.PayerId and c.Status = 1 and (c.MaskRegion & :AdminRegionCode > 0) " + filter + @"
	) or exists(
		select * from Future.Suppliers s
		where s.Payer = {Payer}.PayerId and s.Disabled = 0 and (s.RegionMask & :AdminRegionCode > 0)
	))
ORDER BY {Payer}.shortname;";
				var resultList = session.CreateSQLQuery(sql).AddEntity(typeof(Payer))
					.SetParameter("AdminRegionCode", SecurityContext.Administrator.RegionMask)
					.SetParameter("SearchText", "%" + searchPattern  + "%")
					.List<Payer>().Distinct();
				return resultList;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		public virtual decimal TotalSum
		{
			get
			{
				return GetAccountings().Sum(a => a.Payment);
			}
		}

		public virtual IEnumerable<Accounting> GetAccountings()
		{
			return UsersForInvoice().Concat(AddressesForInvoice());
		}

		private IEnumerable<Accounting> AddressesForInvoice()
		{
			return Addresses.Select(a => a.Accounting).Where(a => a.ShouldPay()).Skip(UsersForInvoice().Count());
		}

		private IEnumerable<Accounting> UsersForInvoice()
		{
			return Users.Select(u => u.Accounting).Where(a => a.ShouldPay());
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual void AddComment(string comment)
		{
			if (String.IsNullOrEmpty(comment))
				return;

			if (!String.IsNullOrEmpty(Comment))
				Comment += "\r\n";
			Comment += comment;
		}

		public virtual string GetInvocesAddress()
		{
			var group = ContactGroupOwner.ContactGroups
				.Where(g => g.Type == ContactGroupType.Invoice)
				.FirstOrDefault();

			if (group == null)
				throw new DoNotHaveContacts(String.Format("Для плательщика {0} - {1} не задана контактрая информаци для от правки счетов", Id, Name));

			var contacts = group
				.Contacts
				.Where(c => c.Type == ContactType.Email)
				.Select(c => c.ContactText)
				.Implode();

			if (String.IsNullOrWhiteSpace(contacts))
				throw new DoNotHaveContacts(String.Format("Для плательщика {0} - {1} не задана контактрая информаци для отправки счетов", Id, Name));

			return contacts;
		}

		public virtual void RecalculateBalance()
		{
			var invoices = Invoice.Queryable.Where(i => i.Payer == this).ToList();
			var payments = Payment.Queryable.Where(p => p.Payer == this).ToList();
			Balance = 0;
			Balance += BeginBalance;
			Balance += payments.Sum(p => p.Sum);
			Balance -= invoices.Sum(i => i.Sum);
		}

		private void UpdateBalance()
		{
			if (this.IsChanged(p => p.BeginBalance))
			{
				var oldBeginBalance = this.OldValue(p => p.BeginBalance);
				Balance += BeginBalance;
				Balance -= oldBeginBalance;
			}
		}

		public virtual List<ContactGroupType> NewGroupTypes
		{
			get
			{
				if (ContactGroupOwner == null)
					return null;
				var types = new List<ContactGroupType>();
				if (!ContactGroupOwner.HaveGroup(ContactGroupType.Billing))
					types.Add(ContactGroupType.Billing);
				if (!ContactGroupOwner.HaveGroup(ContactGroupType.Invoice))
					types.Add(ContactGroupType.Invoice);
				types.Add(ContactGroupType.Custom);
				return types;
			}
		}

		protected override void OnSave()
		{
			UpdateBalance();
			base.OnSave();
		}

		protected override void OnUpdate()
		{
			UpdateBalance();
			base.OnUpdate();
		}

		public virtual void InitGroupOwner()
		{
			if (ContactGroupOwner == null)
			{
				ContactGroupOwner = new ContactGroupOwner();
				ContactGroupOwner.ContactGroups = new List<ContactGroup>();
			}
			if (!ContactGroupOwner.HaveGroup(ContactGroupType.Billing))
				ContactGroupOwner.AddContactGroup(ContactGroupType.Billing);
		}

		public virtual DateTime GetDocumentDate(DateTime date)
		{
			if (!InvoiceSettings.DocumentsOnLastWorkingDay)
			{
				return date;
			}

			var lastDay = date.LastDayOfMonth();
			if (lastDay.DayOfWeek == DayOfWeek.Saturday)
				return lastDay.AddDays(-1);
			else if (lastDay.DayOfWeek == DayOfWeek.Sunday)
				return lastDay.AddDays(-2);
			else
				return lastDay;
		}

		public virtual IEnumerable<Invoice> BuildInvoices(DateTime date, Period period)
		{
			foreach (var invoiceGroup in GetAccountings().GroupBy(a => a.InvoiceGroup))
				yield return new Invoice(this, period, date, invoiceGroup.Key);
		}

		public virtual IEnumerable<IGrouping<int, Accounting>> GetInvoiceGroups()
		{
			return GetAccountings().GroupBy(a => a.InvoiceGroup).OrderBy(g => g.Key);
		}

		public virtual bool ShouldNotify()
		{
			throw new NotImplementedException();
		}
	}

	public class DoNotHaveContacts : Exception
	{
		public DoNotHaveContacts(string message) : base(message)
		{
		}
	}
}