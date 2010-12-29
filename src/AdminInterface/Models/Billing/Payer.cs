using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
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

	[ActiveRecord("payers", Schema = "billing", Lazy = true)]
	public class Payer : ActiveRecordValidationBase<Payer>
	{
		[PrimaryKey]
		public virtual uint PayerID { get; set; }

		public virtual uint Id
		{
			get { return PayerID; }
		}

		[Property]
		public virtual string ShortName { get; set; }

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
		public virtual uint DiscountValue { get; set; }

		[Property]
		public virtual DiscountType DiscountType { get; set; }

		[Property]
		public virtual bool ShowDiscount { get; set; }

		[Property]
		public virtual decimal Balance { get; set; }

		[Nested]
		public virtual InvoiceSettings InvoiceSettings { get; set; }

		[BelongsTo("RecipientId")]
		public virtual Recipient Recipient { get; set; }

		[HasMany(typeof (Client), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<Client> Clients { get; set; }

		[HasMany(typeof (User), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		[HasMany(typeof (Address), Lazy = true, Inverse = true, OrderBy = "Address")]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(typeof(LegalEntity), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<LegalEntity> JuridicalOrganizations { get; set; }

		[HasMany(typeof(Report), Lazy = true, Inverse = true, OrderBy = "Comment")]
		public virtual IList<Report> Reports { get; set; }

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

		public virtual string Name
		{
			get { return ShortName; }
		}

		public virtual float ApplyDiscount(float sum)
		{
			if (DiscountType == DiscountType.Currency)
				return Math.Max(sum - DiscountValue, 0);
			return Math.Max(sum - sum * DiscountValue / 100, 0);
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
					filter += " and FirmType <> 1 ";
				if (!allowViewSuppliers)
					filter += " and FirmType <> 0 ";
				var sql = @"
SELECT  {Payer.*}
FROM Future.Clients as cd
	JOIN billing.payers {Payer} ON cd.PayerId = {Payer}.PayerId
WHERE   cd.regioncode & :AdminRegionCode > 0 
		AND Status = 1 
		AND {Payer}.ShortName like :SearchText " + filter + @"  
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
			return ShortName;
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
			return ContactGroupOwner.ContactGroups
				.Where(g => g.Type == ContactGroupType.Invoice)
				.First()
				.Contacts
				.Where(c => c.Type == ContactType.Email)
				.Select(c => c.ContactText)
				.Implode();
		}
	}
}