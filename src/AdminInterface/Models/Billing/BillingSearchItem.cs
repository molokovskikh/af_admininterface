using System;
using AdminInterface.Helpers;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	public class BillingSearchItem
	{
		public uint PayerId { get; set; }

		public string ShortName { get; set; }

		public string JuridicalName { get; set; }

		public string Recipient { get; set; }

		public double PaySum { get; set; }

		public decimal Balance { get; set; }

		public DateTime LastClientRegistrationDate { get; set; }

		public uint DisabledUsersCount { get; set; }

		public uint EnabledUsersCount { get; set; }

		public uint DisabledAddressesCount { get; set; }

		public uint EnabledAddressesCount { get; set; }

		public uint EnabledClientCount { get; set; }

		public uint EnabledSupplierCount { get; set; }

		public uint EnabledReportsCount { get; set; }

		public string Regions { get; set; }

		public bool ShowPayDate { get; set; }

		public decimal PaymentSum { get; set; }

		[Style]
		public bool IsDebtor
		{
			get { return Balance < 0; }
		}

		[Style]
		public bool IsDisabled
		{
			get
			{
				return EnabledClientCount == 0
					&& EnabledSupplierCount == 0
					&& EnabledReportsCount == 0;
			}
		}

		public override string ToString()
		{
			return PayerId.ToString();
		}
	}
}