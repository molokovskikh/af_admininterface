using MySql.Data.MySqlClient;

namespace AdminInterface.Helpers
{
	public static class SpesializedQuery
	{
		public static MySqlCommand AppendQueryForCreateNotExistsAnalitFReplicationRecordForDrugstore(this MySqlCommand command)
		{
			const string query = @"
INSERT INTO Usersettings.AnalitFReplicationInfo(UserId, FirmCode)
SELECT ouar.RowId, supplier.FirmCode
FROM usersettings.clientsdata as drugstore
  JOIN usersettings.OsUserAccessRight ouar on ouar.ClientCode = drugstore.FirmCode
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.firmsegment
		JOIN pricesdata ON pricesdata.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Usersettings.AnalitFReplicationInfo ari on ari.UserId = ouar.RowId and ari.FirmCode = supplier.FirmCode
WHERE   ari.UserId IS NULL
        AND supplier.firmtype = 0
		AND drugstore.FirmCode = ?clientCode
		AND drugstore.firmtype = 1
group by ouar.RowId, supplier.FirmCode;
";
			command.CommandText += query;
			return command;
		}

		public static MySqlCommand AppendQueryForCreateNotExistsAnalitFReplicationRecordForSupplier(this MySqlCommand command)
		{
			const string query = @"
INSERT INTO Usersettings.AnalitFReplicationInfo(UserId, FirmCode)
SELECT ouar.RowId, supplier.FirmCode
FROM usersettings.clientsdata as drugstore
  JOIN usersettings.OsUserAccessRight ouar on ouar.ClientCode = drugstore.FirmCode
	JOIN clientsdata supplier ON supplier.firmsegment = drugstore.firmsegment
		JOIN pricesdata ON pricesdata.firmcode = supplier.firmcode
	JOIN farm.regions ON (supplier.maskregion & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Usersettings.AnalitFReplicationInfo ari on ari.UserId = ouar.RowId and ari.FirmCode = supplier.FirmCode
WHERE   ari.UserId IS NULL
        and supplier.firmtype = 0
		and supplier.FirmCode = ?ClientCode 
		and drugstore.firmtype = 1
group by ouar.RowId, supplier.FirmCode;
";
			command.CommandText += query;
			return command;
		}
	}
}
