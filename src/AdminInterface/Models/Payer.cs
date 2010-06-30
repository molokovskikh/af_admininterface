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
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	public enum DiscountType
	{
		[Description("В рублях")]
		Currency,
		[Description("В процентах")]
		Percent,
	}

	[ActiveRecord("billing.payers")]
	public class Payer : ActiveRecordValidationBase<Payer>
	{
		public const decimal CostPerUser = 800;

		public const decimal CostPerAdditionalAddress = 200;

		[PrimaryKey]
		public virtual uint PayerID { get; set; }

		[Property]
		public virtual string ShortName { get; set; }

		[Property]
		public virtual string JuridicalName { get; set; }

		[Property]
		public virtual string JuridicalAddress { get; set; }

		[Property]
		public string ReceiverAddress { get; set; }

		[Property]
		[ValidateRegExp("", "КПП должен содержать 9 цифр")]
		public virtual string KPP { get; set; }

		[Property]
		[ValidateRegExp("", "ИНН может содержать 10 или 12 цифр")]
		public virtual string INN { get; set; }

		[BelongsTo(Column = "ContactGroupOwnerId")]
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
		public virtual int AutoInvoice { get; set; }

		[Property]
		public virtual int PayCycle { get; set; }

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

		[BelongsTo("ReciverId")]
		public virtual Reciver Reciver { get; set; }

		[HasMany(typeof (Client), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<Client> Clients { get; set; }

		[HasMany(typeof(JuridicalOrganization), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<JuridicalOrganization> JuridicalOrganizations { get; set; }

		public virtual float ApplyDiscount(float sum)
		{
			if (DiscountType == DiscountType.Currency)
				return Math.Max(sum - DiscountValue, 0);
			return Math.Max(sum - sum * DiscountValue / 100, 0);
		}

		public virtual bool IsManualPayments()
		{
			return AutoInvoice == 0;
		}

		public static Payer GetByClientCode(uint clientCode)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(Payer));
			try
			{
				return session.CreateSQLQuery(@"
select {Payer.*}
from billing.payers {Payer}
	join Future.Clients cd on {Payer}.PayerID = cd.PayerId
where cd.firmcode = :ClientCode")
					.AddEntity(typeof (Payer))
					.SetParameter("ClientCode", clientCode)
					.UniqueResult<Payer>();
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
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

		public Payment[] FindBills(Period period)
		{
			CheckReciver();

			var bills = Payment.FindChargeOffs(this, period);

			if (bills.Length == 0)
				throw new EndUserException(String.Format("Не могу сформировать документ т.к. у платильщика {0} не было отчислений", ShortName));

			if (DetailInvoice == 1)
			{
				var totalChargeOff = Payment.ChargeOff();
				totalChargeOff.Sum = bills.Sum(b => b.Sum);
				totalChargeOff.PayedOn = bills.Max(b => b.PayedOn);
				totalChargeOff.Name = ChangeServiceNameTo;
				return new[] {totalChargeOff};
			}
			return bills;
		}

		public IList<User> GetAllUsers()
		{
			var allUsers = new List<User>();
			foreach (var client in Clients)
				allUsers.AddRange(client.Users);
			return allUsers;
		}

		public IList<Address> GetAllAddresses()
		{
			var allAddresses = new List<Address>();
			foreach (var client in Clients)
				allAddresses.AddRange(client.Addresses);
			return allAddresses;
		}

		public void CheckReciver()
		{
			if (Reciver == null)
				throw new EndUserException(
					String.Format("Не могу сформировать документ т.к. у платильщика {0} не установлен получатель платежей",
					              ShortName));
		}

		public Payment[] FindPayments(DateTime from, DateTime to)
		{
			CheckReciver();

			var payments = Payment.FindBetwen(this, from, to);

			if (payments.Length == 0)
				throw new EndUserException(
					String.Format("Не могу сформировать документ т.к. с платильщиком {0} не было взаиморасчетов",
								  ShortName));

			return payments;
		}

		public DateTime DefaultBeginPeriod()
		{
			if (PayCycle == 0)
				return DateTime.Today.AddMonths(-2);

			return DateTime.Today.AddMonths(-2 * 3);
		}

		public DateTime DefaultEndPeriod()
		{
			return DateTime.Today;
		}

		public float DebitOn(DateTime on)
		{
			return Payment.DebitOn(this, on);
		}

		public float CreditOn(DateTime on)
		{
			return Payment.CreditOn(this, on);
		}

		public decimal TotalSum
		{
			get
			{
				var countAddresses = Convert.ToDecimal(GetAllAddresses().Where(address => 
					address.Enabled &&	// Адрес включен
					!address.IsFree &&	// НЕ бесплатный
					address.Client.Status == ClientStatus.On &&	// Клиент (владелец адреса) включен
					address.AvaliableForEnabledUsers	// Есть хотя бы один включенный пользоыватель, которому доступен этот адрес
					).Count());
				var countUsers = Convert.ToDecimal(GetAllUsers().Where(user => 
					user.Enabled &&	// Пользователь включен	
					!user.IsFree &&	// НЕ бесплатный
					user.Client.Status == ClientStatus.On // Клиент (владелец пользователя) включен
					).Count());
				var sum = countUsers * CostPerUser;
				if ((countAddresses - countUsers) > 0)
					sum += (countAddresses - countUsers) * CostPerAdditionalAddress;
				return sum;
			}
		}
	}
}