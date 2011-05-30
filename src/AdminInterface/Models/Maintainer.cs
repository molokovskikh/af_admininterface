using System;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Models
{
	public class Maintainer
	{
		public static void MaintainIntersection(Supplier supplier)
		{
			MaintainIntersection("AND s.Id = :supplierId",
				q => q.SetParameter("supplierId", supplier.Id));
		}

		public static void MaintainIntersection(Client client, LegalEntity legalEntity)
		{
			MaintainIntersection("AND drugstore.Id = :clientId AND le.Id = :legalEntityId",
				q => q.SetParameter("clientId", client.Id)
					.SetParameter("legalEntityId", legalEntity.Id));
		}

		public static void MaintainIntersection(string filter, Action<IQuery> prepare)
		{
			ArHelper.WithSession(s => {
				var query = s.CreateSQLQuery(String.Format(@"
set @skip = 0;

INSERT
INTO Future.Intersection (
	ClientId,
	RegionId,
	PriceId,
	LegalEntityId,
	CostId,
	PriceMarkup,
	AgencyEnabled,
	AvailableForClient,
	SupplierClientId,
	SupplierPaymentId
)
SELECT  DISTINCT drugstore.Id,
		regions.regioncode,
		pd.pricecode,
		le.Id,
		ifnull(parent.CostId, (
			SELECT costcode
			FROM pricescosts pcc
			WHERE basecost
				AND pcc.PriceCode = pd.PriceCode
		)),
		ifnull(parent.PriceMarkup, 0),
		ifnull(parent.AgencyEnabled, if(a.IgnoreNewPrices = 1, 0, 1)),
		ifnull(parent.AvailableForClient, if(pd.PriceType = 0, 1, 0)),
		rootIntersection.SupplierClientId,
		rootIntersection.SupplierPaymentId
FROM Future.Clients as drugstore
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	join billing.PayerClients p on p.ClientId = drugstore.Id
		join Billing.LegalEntities le on le.PayerId = p.PayerId
	JOIN future.suppliers s ON s.Segment = drugstore.Segment
		JOIN pricesdata pd ON pd.firmcode = s.Id
	JOIN farm.regions ON (s.RegionMask & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pd.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pd.pricecode and i.RegionId = regions.regioncode and i.ClientId = drugstore.Id and i.LegalEntityId = le.Id
	LEFT JOIN Future.Intersection parent ON parent.PriceId = pd.pricecode and parent.RegionId = regions.regioncode and parent.ClientId = drugstore.Id
	LEFT JOIN pricesdata as rootPrice on rootPrice.PriceCode = (select min(pricecode) from pricesdata as p where p.firmcode = s.Id)
		LEFT JOIN future.intersection as rootIntersection on rootIntersection.PriceId = rootPrice.PriceCode and rootIntersection.RegionId = Regions.RegionCode and rootIntersection.ClientId = drugstore.Id
			and rootIntersection.LegalEntityId = le.Id
WHERE i.Id IS NULL
	{0}
group by pd.pricecode, regions.regioncode, drugstore.Id;
", filter));
					prepare(query);
					query.ExecuteUpdate();
				});
		}

		public static void LegalEntityCreated(LegalEntity legalEntity)
		{
			foreach (var client in legalEntity.Payer.Clients)
			{
				MaintainIntersection(client, legalEntity);
			}
		}
	}
}