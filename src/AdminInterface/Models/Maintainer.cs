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
	PriceMarkup,
	AgencyEnabled,
	AvailableForClient
)
SELECT  DISTINCT drugstore.Id,
		regions.regioncode,
		pd.pricecode,
		:legalEntityId,
		ifnull(parent.CostId, (
			SELECT costcode
			FROM pricescosts pcc
			WHERE basecost
				AND pcc.PriceCode = pd.PriceCode
		)),
		ifnull(parent.PriceMarkup, 0),
		ifnull(parent.AgencyEnabled, if(a.IgnoreNewPrices = 1, 0, 1)),
		ifnull(parent.AvailableForClient, if(pd.PriceType = 0, 1, 0))
FROM Future.Clients as drugstore
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.Segment
		JOIN pricesdata pd ON pd.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pd.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pd.pricecode and i.RegionId = regions.regioncode and i.ClientId = drugstore.Id and i.LegalEntityId = :legalEntityId
	LEFT JOIN Future.Intersection parent ON parent.PriceId = pd.pricecode and parent.RegionId = regions.regioncode and parent.ClientId = drugstore.Id
WHERE i.Id IS NULL
	AND supplier.firmtype = 0
	AND drugstore.Id = :clientId
	AND drugstore.FirmType = 1
group by pd.pricecode, regions.regioncode, drugstore.Id;
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