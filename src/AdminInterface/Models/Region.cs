﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using System;

namespace AdminInterface.Models
{
	[ActiveRecord("Regions", Schema = "farm")]
	public class Region : ActiveRecordBase<Region>
	{
		[PrimaryKey("RegionCode")]
		public virtual ulong Id { get; set; }

		[Property("Region")]
		public virtual string Name { get; set; }

		[Property("DefaultShowRegionMask")]
		public virtual UInt64 DefaultShowRegionMask { get; set; }

		public bool IsAll { get; set; }

		public static Region[] GetRegionsByMask(ulong mask)
		{
			return FindAll(Expression.Sql(String.Format("(RegionCode & {0}) > 0", mask)));
		}

		public static IList<Region> GetAllRegions()
		{
			return ArHelper.WithSession(session => {
			
				var regions = session.CreateSQLQuery(@"
select
	(select sum(regioncode) from farm.regions) as {Region.Id},
	'Все' as {Region.Name}, 
	(select sum(DefaultShowRegionMask) from farm.regions) as {Region.DefaultShowRegionMask},
	1 as IsAll
union
SELECT  r.RegionCode as {Region.Id},
		r.Region as {Region.Name},
		r.DefaultShowRegionMask as {Region.DefaultShowRegionMask},
		0 as IsAll
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