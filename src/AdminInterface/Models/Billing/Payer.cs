using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Tools;
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
		Month,
		Quarter
	}

	public class InvoiceSettings
	{
		[Property]
		public virtual bool EmailInvoice { get; set; }

		[Property]
		public virtual bool PrintInvoice { get; set; }
	}

	[ActiveRecord(Schema = "billing", Lazy = true)]
	public class Payer : ActiveRecordValidationBase<Payer>
	{
		public Payer()
		{
			InvoiceSettings = new InvoiceSettings();
		}

		[PrimaryKey]
		public virtual uint PayerID { get; set; }

		public virtual uint Id
		{
			get { return PayerID; }
		}

		[Property("ShortName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual string JuridicalName { get; set; }

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

		[BelongsTo(Column = "ContactGroupOwnerId", Lazy = FetchWhen.OnInvoke)]
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
		public virtual int DetailInvoice { get; set; }

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

		[HasMany(typeof(LegalEntity), Lazy = true, Inverse = true, OrderBy = "Name")]
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
and exists (
	select * from Future.Clients c
		join Billing.PayerClients pc on pc.ClientId = c.Id
	where pc.PayerId = {Payer}.PayerId and c.Status = 1 and (c.MaskRegion & :AdminRegionCode > 0) " + filter + @"
)
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

		public virtual DateTime DefaultBeginPeriod()
		{
			if (PayCycle == 0)
				return DateTime.Today.AddMonths(-2);

			return DateTime.Today.AddMonths(-2 * 3);
		}

		public virtual DateTime DefaultEndPeriod()
		{
			return DateTime.Today;
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

		private void UpdateBalance()
		{
			if (this.IsChanged(p => p.BeginBalance))
			{
				var oldBeginBalance = this.OldValue(p => p.BeginBalance);
				Balance += BeginBalance;
				Balance -= oldBeginBalance;
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
	}

	public class DoNotHaveContacts : Exception
	{
		public DoNotHaveContacts(string message) : base(message)
		{
		}
	}
}