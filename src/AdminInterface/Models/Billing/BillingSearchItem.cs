using System;
using AdminInterface.Helpers;

namespace AdminInterface.Models.Billing
{
	public class BillingSearchItem
	{
		public uint PayerId { get; set; }

		public string ShortName { get; set; }

		public string JuridicalName { get; set; }

		public string Recipient { get; set; }

		public double PaySum { get; set; }

		public decimal Balance {get; set; }

		public DateTime LastClientRegistrationDate { get; set; }
	
		public uint DisabledUsersCount { get; set; }

		public uint EnabledUsersCount { get; set; }

		public uint DisabledAddressesCount { get; set; }

		public uint EnabledAddressesCount { get; set; }

		public uint EnabledClientCount { get; set; }

		public uint EnabledSupplierCount { get; set; }
		
		public string Regions { get; set; }

		public bool HasWholesaleSegment { get; set; }

		public bool HasRetailSegment { get; set; }

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
			get { return (EnabledUsersCount == 0 || EnabledClientCount == 0) && EnabledSupplierCount == 0; }
		}

		public string GetSegments()
		{
			if (HasWholesaleSegment && HasRetailSegment)
				return "Опт, Розница";
			if (HasWholesaleSegment)
				return "Опт";
			if (HasRetailSegment)
				return "Розница";
			return "";
		}
	}
}