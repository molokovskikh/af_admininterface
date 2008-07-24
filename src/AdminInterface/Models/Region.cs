using System.Collections.Generic;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models
{
	[ActiveRecord("farm.regions")]
	public class Region : ActiveRecordBase<Region>
	{
		[PrimaryKey("RegionCode")]
		public virtual ulong Id { get; set; }

		[Property("Region")]
		public virtual string Name { get; set; }

		public static IList<Region> GetRegionsForClient(string clientName)
		{
			return ArHelper.WithSession<Region>(
				session =>
				session.CreateSQLQuery(
					@"
select (select sum(regioncode) from farm.regions) as {Region.Id}, 'Все' as {Region.Name}, 1 as IsAll
union
SELECT  r.RegionCode as {Region.Id},
        r.Region as {Region.Name},
        0 as IsAll
FROM    farm.regions as r,
        accessright.regionaladmins as ra
WHERE   ra.username = :UserName
        and ra.RegionMask & r.regioncode > 0
ORDER BY IsAll Desc, {Region.Name};")
					.AddEntity(typeof (Region))
					.SetParameter("UserName", clientName.Replace("ANALIT\\", ""))
					.List<Region>());
		}
	}
}