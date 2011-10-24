using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	public static class RegionHelper
	{
		public static IList<Region> GetAllRegions()
		{
			return ArHelper.WithSession(session =>
			{
				var regions = session.CreateSQLQuery(@"
select
	(select sum(regioncode) from farm.regions) as {Region.Id},
	'Все' as {Region.Name}, 
	(select sum(DefaultShowRegionMask) from farm.regions) as {Region.DefaultShowRegionMask},
	1 as IsAll,
	0 as {Region.DrugsSearchRegion},
	0 as {Region.AddressPayment},
	0 as {Region.UserPayment},
	0 as {Region.SupplierUserPayment}
union
SELECT  r.RegionCode as {Region.Id},
		r.Region as {Region.Name},
		r.DefaultShowRegionMask as {Region.DefaultShowRegionMask},
		0 as IsAll,
		r.DrugsSearchRegion as {Region.DrugsSearchRegion},
		r.AddressPayment as {Region.AddressPayment},
		r.UserPayment as {Region.UserPayment},
		r.SupplierUserPayment as {Region.SupplierUserPayment}
FROM	farm.regions as r
WHERE	:Mask & r.regioncode > 0
ORDER BY IsAll Desc, {Region.Name};")
					.AddEntity(typeof(Region))
					.SetParameter("Mask", SecurityContext.Administrator.RegionMask)
					.List<Region>();
				regions.Each(session.Evict);
				return regions;
			});
		}
	}

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