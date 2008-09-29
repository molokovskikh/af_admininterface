using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	public enum DiscountType
	{
		[Description("� ������")]
		Currency,
		[Description("� ���������")]
		Percent,
	}

	[ActiveRecord("billing.payers")]
	public class Payer : ActiveRecordValidationBase<Payer>
	{
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
		[ValidateRegExp("", "��� ������ ��������� 9 ����")]
		public virtual string KPP { get; set; }

		[Property]
		[ValidateRegExp("", "��� ����� ��������� 10 ��� 12 ����")]
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

		[HasMany(typeof (Client), Lazy = true, Inverse = true, OrderBy = "ShortName")]
		public virtual IList<Client> Clients { get; set; }

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
	join usersettings.clientsdata cd on {Payer}.PayerID = cd.BillingCode
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

		public Payment[] FindBills(Period period)
		{
			CheckReciver();

			var bills = Payment.FindChargeOffs(this, period);

			if (bills.Length == 0)
				throw new EndUserException(String.Format("�� ���� ������������ �������� �.�. � ����������� {0} �� ���� ����������", ShortName));

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

		public void CheckReciver()
		{
			if (Reciver == null)
				throw new EndUserException(
					String.Format("�� ���� ������������ �������� �.�. � ����������� {0} �� ���������� ���������� ��������",
					              ShortName));
		}

		public Payment[] FindPayments(DateTime from, DateTime to)
		{
			CheckReciver();

			var payments = Payment.FindBetwen(this, from, to);

			if (payments.Length == 0)
				throw new EndUserException(
					String.Format("�� ���� ������������ �������� �.�. � ������������ {0} �� ���� ��������������",
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

		public static void tets()
		{
			throw new EndUserException("123");
		}

		public float DebitOn(DateTime on)
		{
			return Payment.DebitOn(this, on);
		}

		public float CreditOn(DateTime on)
		{
			return Payment.CreditOn(this, on);
		}
	}
}