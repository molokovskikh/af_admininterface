using System;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using NHibernate;

namespace AdminInterface.Models
{
	public class Maintainer
	{
		public static void MaintainIntersection(Supplier supplier, ISession session)
		{
			MaintainIntersection(session, "AND s.Id = :supplierId", q => q.SetParameter("supplierId", supplier.Id));
		}

		public static void MaintainIntersection(ISession session, Client client, LegalEntity legalEntity)
		{
			MaintainIntersection(session, "AND drugstore.Id = :clientId AND le.Id = :legalEntityId",
				q => q.SetParameter("clientId", client.Id).SetParameter("legalEntityId", legalEntity.Id));
		}

		public static void MaintainIntersection(ISession session, string filter, Action<IQuery> prepare)
		{
			var query = session.CreateSQLQuery($@"
set @skip = 0;

INSERT
INTO Customers.Intersection (
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
		ifnull(parent.CostId,
			ifnull((select BaseCost
				from pricesregionaldata prd
				where prd.PriceCode = pd.PriceCode
				and prd.RegionCode = regions.regioncode), (
			SELECT costcode
			FROM pricescosts pcc
			WHERE basecost
				AND pcc.PriceCode = pd.PriceCode
		))),
		ifnull(parent.PriceMarkup, 0),
		ifnull(parent.AgencyEnabled, if(a.IgnoreNewPrices = 1, 0, 1)),
		ifnull(parent.AvailableForClient, if(pd.PriceType = 0, 1, 0)),
		rootIntersection.SupplierClientId,
		rootIntersection.SupplierPaymentId
FROM (Customers.Clients as drugstore, Customers.suppliers s)
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	join billing.PayerClients p on p.ClientId = drugstore.Id
		JOIN Billing.LegalEntities le on le.PayerId = p.PayerId
	JOIN pricesdata pd ON pd.firmcode = s.Id
	JOIN farm.regions ON (s.RegionMask & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pd.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Customers.Intersection i ON i.PriceId = pd.pricecode and i.RegionId = regions.regioncode and i.ClientId = drugstore.Id and i.LegalEntityId = le.Id
	LEFT JOIN Customers.Intersection parent ON parent.PriceId = pd.pricecode and parent.RegionId = regions.regioncode and parent.ClientId = drugstore.Id
	LEFT JOIN (
			select min(pricecode) as PriceCode, FirmCode
			from pricesdata
			group by FirmCode
		) as rootPrice on rootPrice.FirmCode = s.Id
		LEFT JOIN Customers.intersection as rootIntersection on rootIntersection.PriceId = rootPrice.PriceCode and rootIntersection.RegionId = Regions.RegionCode and rootIntersection.ClientId = drugstore.Id
			and rootIntersection.LegalEntityId = le.Id
WHERE i.Id IS NULL
	{filter}
group by pd.pricecode, regions.regioncode, drugstore.Id, le.Id;");
			prepare(query);
			query.SetFlushMode(FlushMode.Always);
			query.ExecuteUpdate();
		}

		public static void LegalEntityCreated(ISession session, LegalEntity legalEntity)
		{
			foreach (var client in legalEntity.Payer.Clients) {
				MaintainIntersection(session, client, legalEntity);
			}
		}
	}
}