using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Security;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class WhoWasNotUpdatedField : BaseLogsQueryFields
	{
		public string Registrant { get; set; }
		public string UpdateDate { get; set; }
		public string LastUpdateDate { get; set; }
		public uint UserId { get; set; }
		public string UserName { get; set; }
	}

	public class WhoWasNotUpdatedFilter : PaginableSortable, IFiltrable<WhoWasNotUpdatedField>
	{
		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public ulong[] Regions { get; set; }

		[Description("Нет обновлений с")]
		public DateTime BeginDate { get; set; }

		public WhoWasNotUpdatedFilter()
		{
			BeginDate = DateTime.Now.AddDays(-14);
			SortBy = "ClientName";
		}

		public string GetRegionNames()
		{
			var result = "";
			if (Regions != null && Regions.Any())
				result = Session.Query<Region>().Where(x => Regions.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList().Implode();
			return result;
		}

		public IList<WhoWasNotUpdatedField> Find()
		{
			return Find(false);
		}

		public IList<WhoWasNotUpdatedField> Find(bool forExcel)
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Regions != null && Regions.Any()) {
				ulong mask = 0;
				foreach (var region in Regions)
					mask |= region;
				regionMask &= mask;
			}

			var result = Session.CreateSQLQuery($@"
drop temporary table if exists Customers.UserSource;
create temporary table Customers.UserSource (
	UserId int unsigned,
	primary key(UserId)
) engine = memory;

INSERT INTO customers.UserSource
select u.Id
from Customers.Users u
	join usersettings.AssignedPermissions ap on ap.UserId = u.id
	join Customers.Clients c on c.Id = u.ClientId
		join Usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
where ap.PermissionId in (1, 81)
	and u.Enabled = 1
	and u.PayerId <> 921
	and u.ExcludeFromManagerReports = 0
	and c.Status = 1
	and rcs.ServiceClient = 0
	and rcs.InvisibleOnFirm = 0
	and rcs.OrderRegionMask & u.OrderRegionMask & :RegionCode > 0
group by u.Id;

drop temporary table if exists Customers.UserSource2;
create temporary table Customers.UserSource2 (
	UserId int unsigned,
	primary key(UserId)
) engine = memory;
insert into Customers.UserSource2
select * from Customers.UserSource;

DROP TEMPORARY TABLE IF EXISTS customers.oneUserDate;
CREATE TEMPORARY TABLE customers.oneUserDate (
UserId INT unsigned,
AddressId INT unsigned) engine=MEMORY;

INSERT
INTO customers.oneUserDate
SELECT u1.id, ua1.AddressId
FROM customers.Users U1
	join Customers.UserSource us on us.UserId = u1.Id
	join customers.UserAddresses ua1 on ua1.UserId = u1.id
	join customers.Addresses a1 on a1.id = ua1.AddressId
	join usersettings.UserUpdateInfo uu1 on uu1.userid = u1.id
where uu1.UpdateDate < :beginDate
group by u1.id
having count(a1.id) = 1;

DROP TEMPORARY TABLE IF EXISTS customers.oneUser;

CREATE TEMPORARY TABLE customers.oneUser (
UserId INT unsigned,
AddressId INT unsigned) engine=MEMORY ;

INSERT
INTO customers.oneUser

SELECT u1.id, ua1.AddressId
FROM customers.Users U1
	join Customers.UserSource us on us.UserId = u1.Id
	join customers.UserAddresses ua1 on ua1.UserId = u1.id
	join customers.Addresses a1 on a1.id = ua1.AddressId
group by u1.id
having count(a1.id) = 1;

SELECT
	c.id as ClientId,
	c.Name as ClientName,
	reg.Region as RegionName,
	u.Id as UserId,
	u.Name as UserName,
	c.Registrant as Registrant,
	uu.UpdateDate as UpdateDate,
	IF(ad.AFTime < ad.AFNetTime, ad.AFNetTime, ad.AFTime) as LastUpdateDate
FROM customers.Users U
	join Customers.UserSource us on us.UserId = u.Id
	join customers.UserAddresses ua on ua.UserId = u.id
	join customers.Addresses a on a.id = ua.AddressId
	join usersettings.UserUpdateInfo uu on uu.userid = u.id
	join customers.Clients c on c.id = u.ClientId and c.Status = 1
	join farm.Regions reg on reg.RegionCode = c.RegionCode
	join logs.authorizationdates ad on ad.UserId = u.Id
	left join Customers.AnalitFNetDatas nd on nd.UserId = u.Id
where uu.UpdateDate < :beginDate
	and ifnull(nd.LastUpdateAt, '2000-01-01') < :beginDate
	and c.RegionCode & :RegionCode > 0
group by u.id
having count(a.id) > 1

union

SELECT
	c.id as ClientId,
	c.Name as ClientName,
	reg.Region as RegionName,
	u.Id as UserId,
	u.Name as UserName,
	if (reg.ManagerName is not null, reg.ManagerName, c.Registrant) as Registrant,
	uu.UpdateDate as UpdateDate,
	IF(ad.AFTime < ad.AFNetTime, ad.AFNetTime, ad.AFTime) as LastUpdateDate
FROM customers.Users U
	join Customers.UserSource2 us on us.UserId = u.Id
	join customers.UserAddresses ua on ua.UserId = u.id
	join customers.Addresses a on a.id = ua.AddressId
	join usersettings.UserUpdateInfo uu on uu.userid = u.id
	join customers.Clients c on c.id = u.ClientId and c.Status = 1
	join logs.authorizationdates ad on ad.UserId = u.Id
	left join accessright.regionaladmins reg on reg.UserName = c.Registrant
	join farm.Regions reg on reg.RegionCode = c.RegionCode
	left join Customers.AnalitFNetDatas nd on nd.UserId = u.Id
where uu.UpdateDate < :beginDate
	and ifnull(nd.LastUpdateAt, '2000-01-01') < :beginDate
	and c.RegionCode & :RegionCode > 0
	and
	u.id in
	(
		select if ((select count(oud.UserId)
				from
					customers.oneUserDate oud
				where
					oud.AddressId = uaddr.AddressId
			) = (
				select count(ou.UserId)
				from
					customers.oneUser ou
				where
					ou.AddressId = uaddr.AddressId)
				, uaddr.UserId, 0
		)
		from customers.UserAddresses as uaddr
		where uaddr.UserId = u.id
	)
group by u.id
having count(a.id) = 1
order by {SortBy} {SortDirection};")
				.SetParameter("beginDate", BeginDate)
				.SetParameter("RegionCode", regionMask)
				.ToList<WhoWasNotUpdatedField>();

			RowsCount = result.Count;

			if (forExcel) {
				return result.ToList();
			}
			else {
				return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
			}
		}
	}
}