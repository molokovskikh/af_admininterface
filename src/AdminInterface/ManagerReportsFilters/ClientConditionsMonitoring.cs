using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.MySql;
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
		public int SupplierCode { get; set; }
		public uint RegionCode { get; set; }
		public string SupplierName { get; set; }
		public string PriceName { get; set; }
		public string CostName { get; set; }
		public double PriceMarkup { get; set; }
		public string SupplierPaymentId { get; set; }
		public string SupplierClientId { get; set; }
		public string CountAvailableForClient { get; set; }
		public string RegionName { get; set; }
		public string ClientName { get; set; }

		private string _priceType;

		public string PriceType
		{
			get { return _priceType; }
			set
			{
				switch (value) {
					case "0":
						_priceType = "Обычный";
						break;
					case "1":
						_priceType = "Ассортиментный";
						break;
					case "2":
						_priceType = "VIP";
						break;
				}
			}
		}

		private string _contacts;

		public string Contacts
		{
			get
			{
				var link = string.Format("mailto:kvasovtest@analit.net?subject={0}, {1} - просьба настроить условия работы", ClientName, RegionName);
				var uri = new Uri(link);
				return string.Format("mailto:{0}{1}", _contacts, uri.PathAndQuery);
			}
			set { _contacts = value; }
		}

		private bool _availableForClient;

		public string AvailableForClient
		{
			get { return _availableForClient ? "Да" : "Нет"; }
			set { _availableForClient = Convert.ToBoolean(int.Parse(value)); }
		}

		public int SupplierDeliveryCount { get; set; }
		public int SupplierDeliveryCountFill { get; set; }

		public string SupplierDeliveryId
		{
			get { return _supplierDeliveryId; }
			set
			{
				if (!string.IsNullOrEmpty(value)) {
					var fillItems = value.Split(',').Where(v => !string.IsNullOrEmpty(v)).ToList();
					SupplierDeliveryCountFill = fillItems.Count;
					_supplierDeliveryId = fillItems.Implode();
				}
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

		[Style]
		public bool NoPriceConnected
		{
			get { return CountAvailableForClient.Split('/').First().Equals("0"); }
		}
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

		public bool DeliveryStyle(MonitoringItem monitoringItem)
		{
			var monitor = monitoringItem.SupplierDeliveryCountFill > 0 && monitoringItem.SupplierDeliveryCountFill < monitoringItem.SupplierDeliveryCount;
			return ((double)AddressCode / AllAddressIntersection >= 0.5) || monitor;
		}
	}

	public class ClientConditionsMonitoringFilter : PaginableSortable, IFiltrable<MonitoringItem>
	{
		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }
		public uint ClientId { get; set; }
		public string ClientName { get; set; }

		public ClientConditionsMonitoringFilter()
		{
			SortBy = "SupplierName";
			SortKeyMap = new Dictionary<string, string> {
				{ "SupplierCode", "s.Id" },
				{ "SupplierName", "supplier.Name" },
				{ "PriceName", "pd.PriceName" },
				{ "PriceType", "pd.PriceType" },
				{ "CostName", "pc.CostName" },
				{ "AvailableForClient", "i.AvailableForClient" },
				{ "CountAvailableForClient", "CountAvailableForClient" },
				{ "PriceMarkup", "i.PriceMarkup" },
				{ "SupplierClientId", "i.SupplierClientId" },
				{ "SupplierDeliveryId", "SupplierDeliveryId" },
				{ "SupplierPaymentId", "i.SupplierPaymentId" }
			};
		}

		public IList<MonitoringItem> Find()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;

			var client = Session.Get<Client>(ClientId);

			if (client == null)
				return new List<MonitoringItem>();

			regionMask &= client.Settings.OrderRegionMask;

			var aggregates = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	COUNT(*) as AllIntersection,
	COUNT(IF(i.PriceMarkup > 0,1,NULL)) as PriceMarkup,
	COUNT(IF(i.SupplierClientId is not null and i.SupplierClientId != '',1,NULL)) as ClientCode,
	COUNT(IF(i.SupplierPaymentId is not null and i.SupplierPaymentId != '',1,NULL)) as PaymentCode
FROM  Customers.Intersection i
	JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	JOIN usersettings.RetClientsSet r ON r.clientcode = drugstore.Id
	JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
	JOIN usersettings.RegionalData rd ON rd.RegionCode = i.RegionId AND rd.FirmCode = pd.firmcode
WHERE   supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND (r.WorkRegionMask & i.RegionId) > 0
	AND pd.agencyenabled = 1
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	AND if(not r.ServiceClient, supplier.Id != 234, 1)
	and (i.RegionId & :Region) > 0
group by PriceId;")
				.SetParameter("Region", regionMask)
				.ToList<AggregateIntersection>()
				.ToDictionary(a => a.PriceId);

			var aggregateAddresses = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	Count(*) as AllAddressIntersection,
	COUNT(IF(ai.SupplierDeliveryId is not null and ai.SupplierDeliveryId != '', 1 ,NULL)) as AddressCode
FROM Customers.Intersection i
	JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	JOIN usersettings.RetClientsSet r ON r.clientcode = drugstore.Id
	JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
	JOIN customers.AddressIntersection ai on ai.IntersectionId = i.Id
	join Customers.Addresses ca on ca.id = ai.AddressId
WHERE supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND (r.WorkRegionMask & i.RegionId) > 0
	AND pd.agencyenabled = 1
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	AND if(not r.ServiceClient, supplier.Id != 234, 1)
	and (i.RegionId & :Region) > 0
	and ca.LegalEntityId = i.LegalEntityId
	and ca.Enabled = true
group by PriceId;")
				.SetParameter("Region", regionMask)
				.ToList<AggregateIntersection>();

			foreach (var aggregateIntersection in aggregateAddresses) {
				aggregates[aggregateIntersection.PriceId].AddressCode = aggregateIntersection.AddressCode;
				aggregates[aggregateIntersection.PriceId].AllAddressIntersection = aggregateIntersection.AllAddressIntersection;
			}

			var result = Session.CreateSQLQuery(string.Format(@"
SELECT
	supplier.Id as SupplierCode,
	supplier.Name as SupplierName,
	i.PriceId as PriceCode,
	pd.PriceName as PriceName,
	pc.CostName as CostName,
	pc.BaseCost as BaseCost,
	i.PriceMarkup as PriceMarkup,
	i.SupplierClientId as SupplierClientId,
	i.SupplierPaymentId as SupplierPaymentId,
	i.AvailableForClient as AvailableForClient,
	reg.RegionCode as RegionCode,
	reg.region as RegionName,
	c.Name as ClientName,
	pd.pricetype as PriceType,

(select
	count(*)
from
	Usersettings.PricesCosts
where
	PriceCode = i.PriceId
) as PricesCosts,

(select
	concat(cast(count(IF(II.AvailableForClient > 0, 1, NULL)) as char(255)), '/', cast(count(*) as char(255))) as CountAvailableForClient
from Usersettings.PricesData pd
	join Usersettings.PricesData pdj on pd.FirmCode = pdj.FirmCode and pdj.Enabled = 1
	join usersettings.pricesRegionaldata rd on rd.PriceCode = pdj.PriceCode and (rd.RegionCode & :Region) > 0
	join customers.Intersection II on (ii.RegionId & :Region) > 0 and II.ClientId = :ClientCode and II.PriceId = pdj.PriceCode
where
	pd.PriceCode = I.PriceId
	and pd.pricetype <> 1
	and pd.enabled = 1
	AND pd.agencyenabled = 1
	and rd.Enabled = 1
) as CountAvailableForClient,

(
SELECT group_concat(distinct c.ContactText SEPARATOR '; ') as Contacts
FROM customers.suppliers s1
	join contacts.contact_groups cg on cg.ContactGroupOwnerId = s1.ContactGroupOwnerId
	join contacts.persons p on p.ContactGroupId = cg.id
	join contacts.contacts c on c.ContactOwnerId = cg.id || c.ContactOwnerId = p.Id
where s1.id = supplier.Id
	and cg.type = 1
	and c.Type = 0
) as Contacts,

	group_concat(ai.SupplierDeliveryId) as SupplierDeliveryId,
	count(i.Id) as SupplierDeliveryCount
FROM Customers.Intersection i
	JOIN Customers.Clients c ON c.Id = i.ClientId
	JOIN usersettings.RetClientsSet r ON r.clientcode = c.Id
	JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
	JOIN usersettings.RegionalData rd ON rd.RegionCode = i.RegionId AND rd.FirmCode = pd.firmcode
	join Usersettings.PricesCosts pc on pc.CostCode = i.CostId
	join farm.Regions reg on reg.RegionCode = i.RegionId
	left join Customers.AddressIntersection ai on ai.IntersectionId = i.id
where
	(i.RegionId & :Region) > 0
	and i.ClientId = :ClientCode
	and supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (c.maskregion & i.RegionId) > 0
	AND (r.WorkRegionMask & i.RegionId) > 0
	AND pd.agencyenabled = 1
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	AND if(not r.ServiceClient, supplier.Id != 234, 1)
	AND if(pd.PriceType = 2, i.AvailableForClient = 1, 1)
group by i.id
order by {0} {1};", SortKeyMap[SortBy], SortDirection))
				.SetParameter("Region", regionMask)
				.SetParameter("ClientCode", (uint)ClientId)
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

				if (aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.DeliveryStyle = aggregates[monitoringItem.PriceCode].DeliveryStyle(monitoringItem);
				}
			}

			RowsCount = result.Count;
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}
	}
}