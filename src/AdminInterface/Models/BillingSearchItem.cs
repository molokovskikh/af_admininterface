using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Model
{
	[ActiveRecord]
	public class BillingSearchItem : ActiveRecordBase
	{
		private uint		_billingCode;
		private string		_shortName;
		private double		_paySum;
		private DateTime	_payDate;
		private DateTime	_lastClientRegistrationDate;
		private uint		_lastRegistredClientId;
		private uint		_disabledClientCount;
		private uint		_enableClientCount;
		private string		_regions;
		private bool _hasRetailSegment;
		private bool _hasWholesaleSegment;

		[PrimaryKey]
		public uint BillingCode
		{
			get { return _billingCode; }
			set { _billingCode = value; }
		}

		[Property]
		public string ShortName
		{
			get { return _shortName; }
			set { _shortName = value; }
		}

		[Property]
		public double PaySum
		{
			get { return _paySum; }
			set { _paySum = value; }
		}

		[Property]
		public DateTime PayDate
		{
			get { return _payDate; }
			set { _payDate = value; }
		}

		[Property]
		public DateTime LastClientRegistrationDate
		{
			get { return _lastClientRegistrationDate; }
			set { _lastClientRegistrationDate = value; }
		}

		[Property]
		public uint LastRegistredClientId
		{
			get { return _lastRegistredClientId; }
			set { _lastRegistredClientId = value; }
		}

		[Property]
		public uint DisabledClientsCount
		{
			get { return _disabledClientCount; }
			set { _disabledClientCount = value; }
		}

		[Property]
		public uint EnabledClientsCount
		{
			get { return _enableClientCount; }
			set { _enableClientCount = value; }
		}

		[Property]
		public string Regions
		{
			get { return _regions; }
			set { _regions = value; }
		}

		[Property]
		public bool HasWholesaleSegment
		{
			get { return _hasWholesaleSegment; }
			set { _hasWholesaleSegment = value; }
		}

		[Property]
		public bool HasRetailSegment
		{
			get { return _hasRetailSegment; }
			set { _hasRetailSegment = value; }
		}

		public bool IsDebitor()
		{
			return DateTime.Now - _payDate > TimeSpan.FromDays(1);
		}

		public string GetSegments()
		{
			if (_hasWholesaleSegment && _hasRetailSegment)
				return "Опт, Розница";
			if (_hasWholesaleSegment)
				return "Опт";
			if (_hasRetailSegment)
				return "Розница";
			return "";
		}

		public static IList<BillingSearchItem> FindBy(BillingSearchProperties properties)
		{
			ISessionFactoryHolder sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			ISession session = sessionHolder.CreateSession(typeof(BillingSearchItem));
			try
			{
				string debitorFilterBlock = "";
				string segmentFilterBlock = "";
				string typeFilterBlock = "";
				string clientStatusFilterBlock = "";
				switch (properties.PayerState)
				{
					case PayerStateFilter.All:
						break;
					case PayerStateFilter.Debitors:
						debitorFilterBlock = "and p.oldpaydate <= curDate()";
						break;
					case PayerStateFilter.NotDebitors:
						debitorFilterBlock = "and p.oldpaydate > curDate()";
						break;
				}

				switch(properties.Segment)
				{
					case SearchSegment.All:
						break;
                    case SearchSegment.Retail:
						segmentFilterBlock = "where sum(if(cd.firmsegment = 1, 1, 0)) > 0";
						break;
					case SearchSegment.Wholesale:
						segmentFilterBlock = "where sum(if(cd.firmsegment = 0, 1, 0)) > 0";
						break;
				}

				switch(properties.ClientType)
				{
					case SearchClientType.All:
						break;
					case SearchClientType.Drugstore:
						typeFilterBlock = "and sum(if(cd.FirmType = 1, 1, 0)) > 0";
						break;
					case SearchClientType.Supplier:
						typeFilterBlock = "and sum(if(cd.FirmType = 0, 1, 0)) > 0";
						break;
				}

				switch(properties.ClientStatus)
				{
					case SearchClientStatus.All:
						break;
					case SearchClientStatus.Enabled:
						clientStatusFilterBlock = "and sum(if(cd.Firmstatus = 1 and cd.Billingstatus = 1, 1, 0)) > 0";
						break;
					case SearchClientStatus.Disabled:
						clientStatusFilterBlock = "and count(*) = sum(if(cd.Firmstatus = 0 or cd.Billingstatus = 0, 1, 0))";
						break;
				}

				IList<BillingSearchItem> result = session.CreateSQLQuery(String.Format(@"
select p.payerId as {{BillingSearchItem.BillingCode}},
		p.JuridicalName,
        p.shortname as {{BillingSearchItem.ShortName}},
        p.oldpaydate as {{BillingSearchItem.PayDate}},
        p.oldtariff as {{BillingSearchItem.PaySum}},
		max(RegistrationDate) as {{BillingSearchItem.LastClientRegistrationDate}},		
        max(cd.firmcode) as {{BillingSearchItem.LastRegistredClientId}},
        sum(if(cd.FirmStatus = 0, 1, 0)) as {{BillingSearchItem.DisabledClientsCount}},
        sum(if(cd.FirmStatus = 1, 1, 0)) as {{BillingSearchItem.EnabledClientsCount}},

		(select cast(group_concat(r.region order by r.region separator ', ') as char)
		from farm.regions r
		where r.regioncode & bit_or(cd.maskregion) > 0) as {{BillingSearchItem.Regions}},

		sum(if(cd.firmsegment = 1, 1, 0)) > 0 as {{BillingSearchItem.HasRetailSegment}},
		sum(if(cd.firmsegment = 0, 1, 0)) > 0 as {{BillingSearchItem.HasWholesaleSegment}}
from billing.payers p
	join usersettings.clientsdata cd on p.payerid = cd.billingcode
{0}
group by p.payerId
having bit_or(cd.MaskRegion) & :RegionId > 0
		and (p.ShortName like :Name 
			or p.JuridicalName like :Name
			or sum(if(cd.ShortName like :Name or cd.FullName like :Name, 1, 0)) > 0)
		{1}
		{2}
		{3}
order by {{BillingSearchItem.ShortName}}
", debitorFilterBlock, segmentFilterBlock, typeFilterBlock, clientStatusFilterBlock))
													.AddEntity(typeof(BillingSearchItem))
													.SetParameter("RegionId", properties.RegionId)
													.SetParameter("Name", "%" + properties.ShortName + "%")
													.List<BillingSearchItem>();
				ArHelper.Evict(session, result);
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}
	}
}
