namespace AdminInterface.Helpers
{
	public class SharedCommands
	{
		public static string UpdateWorkRules = @"
UPDATE intersection as src
	JOIN intersection as dst on dst.pricecode = src.pricecode and dst.regioncode = src.regioncode
SET dst.CostCode = src.CostCode,
	dst.FirmCostCorr = src.FirmCostCorr,
	dst.PublicCostCorr = src.PublicCostCorr,
	dst.InvisibleOnClient = src.InvisibleOnClient,
	dst.DisabledByClient = src.DisabledByClient,
	dst.DisabledByAgency = src.DisabledByAgency,
	dst.MinReq = src.MinReq,
	dst.ControlMinReq = src.ControlMinReq,

	dst.FirmClientCode = if(?IncludeType = 2, src.FirmClientCode, dst.FirmClientCode),
	dst.FirmClientCode2 = if(?IncludeType = 2, src.FirmClientCode2, dst.FirmClientCode),
	dst.FirmClientCode3 = if(?IncludeType = 2, src.FirmClientCode3, dst.FirmClientCode)
WHERE src.ClientCode = ?Parent and dst.ClientCode = ?Child;
";
	}
}