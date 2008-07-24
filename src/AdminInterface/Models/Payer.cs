using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
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
		public virtual int AutoInvoice { get; set; }

		[Property]
		public virtual int PayCycle { get; set; }

		[Property]
		public DateTime OldPayDate { get; set;}

		[Property]
		public double OldTariff { get; set; }

		[Property]
		public bool HaveContract { get; set; }

		[Property]
		public bool SendRegisteredLetter { get; set; }

		[Property]
		public bool SendScannedDocuments { get; set; }

		[HasMany(typeof (Client), Lazy = true, Inverse = true, OrderBy = "ShortName")]
		public virtual IList<Client> Clients { get; set; }

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
	}
}