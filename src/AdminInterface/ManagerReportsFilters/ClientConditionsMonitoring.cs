using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.ManagerReportsFilters
{
	public class MonitoringItem
	{
		private string _supplierDeliveryId;

		public uint PriceCode { get; set; }
		public uint SupplierCode { get; set; }
		public string SupplierName { get; set; }
		public string PriceName { get; set; }
		public string CostName { get; set; }
		public double PriceMarkup { get; set; }
		public string SupplierPaymentId { get; set; }
		public string SupplierClientId { get; set; }

		public string SupplierDeliveryId
		{
			get { return _supplierDeliveryId; }
			set
			{
				if (!string.IsNullOrEmpty(value))
					_supplierDeliveryId = value.Split(',').Where(v => !string.IsNullOrEmpty(v)).Implode();
			}
		}

		public bool BaseCost { get; set; }
		public int PricesCosts { get; set; }

		[Style]
		public bool CostCollumn
		{
			get { return PricesCosts >= 5 && BaseCost; }
		}

		[Style]
		public bool PriceMarkupStyle { get; set; }

		[Style]
		public bool PaymentCodeStyle { get; set; }

		[Style]
		public bool ClientCodeStyle { get; set; }

		[Style]
		public bool DeliveryStyle { get; set; }
	}

	public class AggregateIntersection
	{
		public uint PriceId { get; set; }
		public int AllIntersection { get; set; }
		public int AllAddressIntersection { get; set; }
		public int PriceMarkup { get; set; }
		public int ClientCode { get; set; }
		public int PaymentCode { get; set; }
		public int AddressCode { get; set; }

		public bool PriceMarkupStyle()
		{
			return (double)PriceMarkup / AllIntersection >= 0.5;
		}

		public bool PaymentCodeStyle()
		{
			return (double)PaymentCode / AllIntersection >= 0.5;
		}

		public bool ClientCodeStyle()
		{
			return (double)ClientCode / AllIntersection >= 0.5;
		}

		public bool DeliveryStyle()
		{
			return (double)AddressCode / AllAddressIntersection >= 0.5;
		}
	}

	public class ClientConditionsMonitoringFilter : PaginableSortable
	{
		public ISession Session;
		public uint ClientId { get; set; }
		public Region Region { get; set; }

		public IList<MonitoringItem> Find()
		{
			var aggregates = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	COUNT(*) as AllIntersection,
	COUNT(IF(i.PriceMarkup > 0,1,NULL)) as PriceMarkup,
	COUNT(IF(i.SupplierClientId > 0,1,NULL)) as ClientCode,
	COUNT(IF(i.SupplierPaymentId > 0,1,NULL)) as PaymentCode
FROM customers.Intersection I
	LEFT JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	LEFT JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	LEFT JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	LEFT JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
WHERE supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND pd.agencyenabled = 1
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	and i.AvailableForClient = 1
	and i.AgencyEnabled = 1
	and i.RegionId = :Region
group by PriceId;")
				.SetParameter("Region", Region.Id)
				.ToList<AggregateIntersection>()
				.ToDictionary(a => a.PriceId);

			var aggregateAddresses = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	Count(*) as AllAddressIntersection,
	COUNT(IF(ai.SupplierDeliveryId > 0,1,NULL)) as AddressCode
FROM customers.Intersection I
	LEFT JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	LEFT JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	LEFT JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	LEFT JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
	JOIN customers.AddressIntersection ai on ai.IntersectionId = i.Id
WHERE  supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND pd.agencyenabled = 1
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	and i.AvailableForClient = 1
	and i.AgencyEnabled = 1
	and i.RegionId = :Region
group by PriceId;")
				.SetParameter("Region", Region.Id)
				.ToList<AggregateIntersection>();

			foreach (var aggregateIntersection in aggregateAddresses) {
				aggregates[aggregateIntersection.PriceId].AddressCode = aggregateIntersection.AddressCode;
				aggregates[aggregateIntersection.PriceId].AllAddressIntersection = aggregateIntersection.AllAddressIntersection;
			}

			var result = Session.CreateSQLQuery(@"
SELECT
	s.Id as SupplierCode,
	s.Name as SupplierName,
	i.PriceId as PriceCode,
	pd.PriceName as PriceName,
	pc.CostName as CostName,
	pc.BaseCost as BaseCost,
	i.PriceMarkup as PriceMarkup,
	i.SupplierClientId as SupplierClientId,
	i.SupplierPaymentId as SupplierPaymentId,
	(select count(*) from Usersettings.PricesCosts
	where PriceCode = i.PriceId) as PricesCosts,
	group_concat(ai.SupplierDeliveryId) as SupplierDeliveryId
FROM customers.Intersection I
	join Usersettings.PricesData pd on pd.PriceCode = i.PriceId
	join Customers.Suppliers s on s.Id = pd.FirmCode
	join Usersettings.PricesCosts pc on pc.CostCode = i.CostId
	left join Customers.AddressIntersection ai on ai.IntersectionId = i.id
where
	i.RegionId = :Region
	and i.ClientId = :ClientCode
	and (s.RegionMask & i.RegionId) > 0
	and pd.agencyenabled = 1
	and pd.enabled = 1
	and pd.pricetype <> 1
	and i.AvailableForClient = 1
	and i.AgencyEnabled = 1
	and s.disabled = 0
group by i.id;")
				.SetParameter("Region", Region.Id)
				.SetParameter("ClientCode", ClientId)
				.ToList<MonitoringItem>();

			foreach (var monitoringItem in result) {
				if (Math.Abs(monitoringItem.PriceMarkup) < 0.001
					&& aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.PriceMarkupStyle = aggregates[monitoringItem.PriceCode].PriceMarkupStyle();
				}

				if (string.IsNullOrEmpty(monitoringItem.SupplierClientId)
					&& aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.ClientCodeStyle = aggregates[monitoringItem.PriceCode].ClientCodeStyle();
				}

				if (string.IsNullOrEmpty(monitoringItem.SupplierPaymentId)
					&& aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.PaymentCodeStyle = aggregates[monitoringItem.PriceCode].PaymentCodeStyle();
				}

				if (string.IsNullOrEmpty(monitoringItem.SupplierDeliveryId)
					&& aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.DeliveryStyle = aggregates[monitoringItem.PriceCode].DeliveryStyle();
				}
			}

			return result.ToList();
		}
	}
}