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
		public string CountAvailableForClient { get; set; }
		public string RegionName { get; set; }
		public string ClientName { get; set; }

		private string _contacts;

		public string Contacts
		{
			get
			{
				var body = string.Format("Просим Вас произвести настроки клиента {0} в регионе {1}, а именно настроить: \r\n", ClientName, RegionName);
				if (PriceMarkupStyle)
					body += "Наценку \r\n";
				if (PaymentCodeStyle)
					body += "Код оплаты \r\n";
				if (ClientCodeStyle)
					body += "Код клиента \r\n";
				if (DeliveryStyle)
					body += "Код доставки \r\n";

				var link = string.Format("mailto:kvasovtest@analit.net?subject={1}, {2} - просьба настроить условия работы&body={3}", _contacts, ClientName, RegionName, body);
				var uri = new Uri(link);
				var data = Encoding.UTF8.GetBytes(uri.PathAndQuery);
				//return urlHelper.Link("Направить уведомление", new Dictionary<string, object> { { "basePath", string.Format("mailto:{0}", _contacts) }, { "params", new Dictionary<string, object> { { "subject", string.Format("{0}, {1} - просьба настроить условия работы", ClientName, RegionName) }, { "body", body } } } });
				return string.Format("mailto:{0}", _contacts) + Encoding.GetEncoding(1251).GetString(data);
			}
			set { _contacts = value; }
		}

		private bool _availableForClient;

		public string AvailableForClient
		{
			get { return _availableForClient ? "Да" : "Нет"; }
			set { _availableForClient = Convert.ToBoolean(int.Parse(value)); }
		}

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

		public bool DeliveryStyle()
		{
			return (double)AddressCode / AllAddressIntersection >= 0.5;
		}
	}

	public class ClientConditionsMonitoringFilter : PaginableSortable, IFind<MonitoringItem>
	{
		public ISession Session { get; set; }
		public int ClientId { get; set; }
		public Region Region { get; set; }

		public ClientConditionsMonitoringFilter()
		{
			SortBy = "SupplierCode";
			SortKeyMap = new Dictionary<string, string> {
				{ "SupplierCode", "s.Id" },
				{ "SupplierName", "s.Name" },
				{ "PriceName", "pd.PriceName" },
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
			if (Region != null)
				regionMask &= Region.Id;

			var aggregates = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	COUNT(*) as AllIntersection,
	COUNT(IF(i.PriceMarkup > 0,1,NULL)) as PriceMarkup,
	COUNT(IF(i.SupplierClientId is not null and i.SupplierClientId != '',1,NULL)) as ClientCode,
	COUNT(IF(i.SupplierPaymentId is not null and i.SupplierPaymentId != '',1,NULL)) as PaymentCode
FROM customers.Intersection I
	LEFT JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	LEFT JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	LEFT JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	LEFT JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
WHERE supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	and i.AgencyEnabled = 1
	and i.RegionId = :Region
group by PriceId;")
				.SetParameter("Region", regionMask)
				.ToList<AggregateIntersection>()
				.ToDictionary(a => a.PriceId);

			var aggregateAddresses = Session.CreateSQLQuery(@"
SELECT
	PriceId,
	Count(*) as AllAddressIntersection,
	COUNT(IF(ai.SupplierDeliveryId is not null and ai.SupplierDeliveryId != '',1,NULL)) as AddressCode
FROM customers.Intersection I
	JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
	JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
	JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
	JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
	JOIN customers.AddressIntersection ai on ai.IntersectionId = i.Id
	join Customers.Addresses ca on ca.id = ai.AddressId
WHERE  supplier.Disabled = 0
	and (supplier.RegionMask & i.RegionId) > 0
	AND (drugstore.maskregion & i.RegionId) > 0
	AND pd.enabled = 1
	AND pd.pricetype <> 1
	AND prd.enabled = 1
	and i.AgencyEnabled = 1
	and i.RegionId = :Region
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
	s.Id as SupplierCode,
	s.Name as SupplierName,
	i.PriceId as PriceCode,
	pd.PriceName as PriceName,
	pc.CostName as CostName,
	pc.BaseCost as BaseCost,
	i.PriceMarkup as PriceMarkup,
	i.SupplierClientId as SupplierClientId,
	i.SupplierPaymentId as SupplierPaymentId,
	i.AvailableForClient as AvailableForClient,
	reg.region as RegionName,
	c.Name as ClientName,

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
	join usersettings.pricesRegionaldata rd on rd.PriceCode = pdj.PriceCode and rd.RegionCode = :Region
	join customers.Intersection II on ii.RegionId = :Region and II.ClientId = :ClientCode and II.PriceId = pdj.PriceCode
where
	pd.PriceCode = I.PriceId
) as CountAvailableForClient,

(
SELECT group_concat(distinct c.ContactText SEPARATOR ', ') as Contacts
FROM customers.suppliers s1
	join contacts.contact_groups cg on cg.ContactGroupOwnerId = s1.ContactGroupOwnerId
	join contacts.persons p on p.ContactGroupId = cg.id
	join contacts.contacts c on c.ContactOwnerId = cg.id || c.ContactOwnerId = p.Id
where s1.id = s.Id
	and cg.type = 1
	and c.Type = 0
) as Contacts,

	group_concat(ai.SupplierDeliveryId) as SupplierDeliveryId
FROM customers.Intersection I
	join Usersettings.PricesData pd on pd.PriceCode = i.PriceId
	join Customers.Suppliers s on s.Id = pd.FirmCode
	join Usersettings.PricesCosts pc on pc.CostCode = i.CostId
	join farm.Regions reg on reg.RegionCode = i.RegionId
	join customers.Clients c on c.id = :ClientCode
	left join Customers.AddressIntersection ai on ai.IntersectionId = i.id
where
	i.RegionId = :Region
	and i.ClientId = :ClientCode
	and (s.RegionMask & i.RegionId) > 0
	and pd.enabled = 1
	and pd.pricetype <> 1
	and i.AgencyEnabled = 1
	and s.disabled = 0
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

				if (string.IsNullOrEmpty(monitoringItem.SupplierDeliveryId)
					&& aggregates.Keys.Contains(monitoringItem.PriceCode)) {
					monitoringItem.DeliveryStyle = aggregates[monitoringItem.PriceCode].DeliveryStyle();
				}
			}

			RowsCount = result.Count;
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}
	}
}