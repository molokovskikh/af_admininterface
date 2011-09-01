using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.MySql;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Models
{
	[ActiveRecord(SchemaAction = "none")]
	public class BillingSearchItem : ActiveRecordBase
	{
		[PrimaryKey]
		public uint BillingCode { get; set; }

		[Property]
		public string ShortName { get; set; }

		[Property]
		public string Recipient { get; set; }

		[Property]
		public double PaySum { get; set; }

		[Property]
		public decimal Balance {get; set; }

		[Property]
		public DateTime LastClientRegistrationDate { get; set; }
	
		[Property]
		public uint DisabledUsersCount { get; set; }

		[Property]
		public uint EnabledUsersCount { get; set; }

		[Property]
		public uint DisabledAddressesCount { get; set; }

		[Property]
		public uint EnabledAddressesCount { get; set; }

		[Property]
		public uint EnabledClientCount { get; set; }

		[Property]
		public uint EnabledSupplierCount { get; set; }
		
		[Property]
		public string Regions { get; set; }

		[Property]
		public bool HasWholesaleSegment { get; set; }

		[Property]
		public bool HasRetailSegment { get; set; }

		[Property]
		public bool ShowPayDate { get; set; }

		public bool IsDebitor()
		{
			return Balance < 0;
		}

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