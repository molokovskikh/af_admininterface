using System.Linq;

namespace AdminInterface.Models
{
	public class RegionSettings
	{
		public ulong Id { get; set; }

		public bool IsAvaliableForOrder { get; set; }

		public bool IsAvaliableForBrowse { get; set; }
	}

	public static class RegionSettingsExtension
	{
		public static ulong GetOrderMask(this RegionSettings[] regions)
		{
			return regions.Where(region => region.IsAvaliableForOrder)
				.Aggregate<RegionSettings, ulong>(0, (current, region) => current | region.Id);
		}

		public static ulong GetBrowseMask(this RegionSettings[] regions)
		{
			return regions.Where(region => region.IsAvaliableForBrowse)
				.Aggregate<RegionSettings, ulong>(0, (current, region) => current | region.Id);
		}
	}
}