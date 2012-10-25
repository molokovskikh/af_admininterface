using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Security;
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
		public uint UserId { get; set; }
		public string UserName { get; set; }
	}

	public class WhoWasNotUpdatedFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }

		public WhoWasNotUpdatedFilter()
		{
			PageSize = 30;
			Period = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			SortBy = "ClientName";
		}

		public IList<WhoWasNotUpdatedField> SqlQuery2(ISession session)
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var result = session.CreateSQLQuery(string.Format(@"
DROP TEMPORARY TABLE IF EXISTS customers.oneUserDate;

CREATE TEMPORARY TABLE customers.oneUserDate (
UserId INT unsigned,
AddressId INT unsigned) engine=MEMORY ;

INSERT
INTO customers.oneUserDate
	SELECT u1.id, ua1.AddressId FROM customers.Users U1
	join customers.UserAddresses ua1 on ua1.UserId = u1.id
	join customers.Addresses a1 on a1.id = ua1.AddressId
	join usersettings.UserUpdateInfo uu1 on uu1.userid = u1.id
	where uu1.UpdateDate >= :beginDate and uu1.UpdateDate <= :endDate

		and (SELECT count(a2.id) FROM customers.Users U2
			join customers.UserAddresses ua2 on ua2.UserId = u2.id
			join customers.Addresses a2 on a2.id = ua2.AddressId
			where ua2.UserId = u1.id) = 1
	group by u1.id
	having count(a1.id) = 1;

DROP TEMPORARY TABLE IF EXISTS customers.oneUser;

CREATE TEMPORARY TABLE customers.oneUser (
UserId INT unsigned,
AddressId INT unsigned) engine=MEMORY ;

INSERT
INTO customers.oneUser

SELECT u1.id, ua1.AddressId FROM customers.Users U1
join customers.UserAddresses ua1 on ua1.UserId = u1.id
join customers.Addresses a1 on a1.id = ua1.AddressId
where
	(SELECT count(a3.id) FROM customers.Users U3
	join customers.UserAddresses ua3 on ua3.UserId = u3.id
	join customers.Addresses a3 on a3.id = ua3.AddressId
	where ua3.UserId = u1.id) = 1
group by u1.id
having count(a1.id) = 1;

SELECT
	c.id as ClientId,
	c.Name as ClientName,
	reg.Region as RegionName,
	u.Id as UserId,
	u.Name as UserName,
	c.Registrant as Registrant,
	uu.UpdateDate as UpdateDate
FROM customers.Users U
	join customers.UserAddresses ua on ua.UserId = u.id
	join customers.Addresses a on a.id = ua.AddressId
	join usersettings.UserUpdateInfo uu on uu.userid = u.id
	join customers.Clients c on c.id = u.ClientId
	join farm.Regions reg on reg.RegionCode = c.RegionCode
where uu.UpdateDate >= :beginDate and uu.UpdateDate <= :endDate
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
	c.Registrant as Registrant,
	uu.UpdateDate as UpdateDate
FROM customers.Users U
	join customers.UserAddresses ua on ua.UserId = u.id
	join customers.Addresses a on a.id = ua.AddressId
	join usersettings.UserUpdateInfo uu on uu.userid = u.id
	join customers.Clients c on c.id = u.ClientId
	join farm.Regions reg on reg.RegionCode = c.RegionCode
where uu.UpdateDate >= :beginDate and uu.UpdateDate <= :endDate
and c.RegionCode & :RegionCode > 0
and
u.id in
(select if (
	(select count(oud.UserId)
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
	, uaddr.UserId, 0)

	from customers.UserAddresses as uaddr
	where uaddr.UserId = u.id
)
group by u.id
having count(a.id) = 1
order by {0} {1}
;", SortBy, SortDirection))
				.SetParameter("beginDate", Period.Begin)
				.SetParameter("endDate", Period.End)
				.SetParameter("RegionCode", regionMask)
				.ToList<WhoWasNotUpdatedField>();

			RowsCount = result.Count;
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}
	}
}