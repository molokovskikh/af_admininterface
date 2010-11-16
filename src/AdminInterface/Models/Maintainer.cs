using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models
{
	public class Maintainer
	{
		public static void MaintainIntersection(Client client, LegalEntity legalEntity)
		{
			ArHelper.WithSession(
				s => {
					s.CreateSQLQuery(@"
set @skip = 0;

INSERT
INTO Future.Intersection (
	ClientId,
	RegionId,
	PriceId,
	LegalEntityId,
	CostId,
	AvailableForClient
)
SELECT  DISTINCT drugstore.Id,
		regions.regioncode,
		pricesdata.pricecode,
		:legalEntityId,
		(
		  SELECT costcode
		  FROM    pricescosts pcc
		  WHERE   basecost
				  AND pcc.PriceCode = pricesdata.PriceCode
		) as CostCode,
		if(pricesdata.PriceType = 0, 1, 0) as AvailableForClient
FROM Future.Clients as drugstore
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.Segment
		JOIN pricesdata ON pricesdata.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pricesdata.pricecode and i.RegionId = regions.regioncode and i.ClientId = drugstore.Id and i.LegalEntityId = :legalEntityId
WHERE i.Id IS NULL
	AND supplier.firmtype = 0
	AND drugstore.Id = :clientId
	AND drugstore.FirmType = 1;
")
							.SetParameter("clientId", client.Id)
							.SetParameter("legalEntityId", legalEntity.Id)
							.ExecuteUpdate();
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