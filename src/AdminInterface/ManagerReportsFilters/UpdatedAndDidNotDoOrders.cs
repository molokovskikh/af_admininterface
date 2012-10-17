using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;

namespace AdminInterface.ManagerReportsFilters
{
	public class UpdatedAndDidNotDoOrdersField : BaseLogsQueryFields
	{
		public uint UserId { get; set; }
		public string UserName { get; set; }
		public string UpdateDate { get; set; }
		public string Registrant { get; set; }
	}

	public class UpdatedAndDidNotDoOrdersFilter : PaginableSortable
	{
		public Region Region { get; set; }
		[Description("Не делались заказы")]
		public DateTime OrderDate { get; set; }
		public DatePeriod UpdatePeriod { get; set; }

		public UpdatedAndDidNotDoOrdersFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" }
			};
			PageSize = 30;
			OrderDate = DateTime.Now.AddDays(-7);
			UpdatePeriod = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			SortBy = "c.Name";
		}

		public IList<UpdatedAndDidNotDoOrdersField> Find(ISession session)
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var result = session.CreateSQLQuery(string.Format(@"
select
	c.id as ClientId,
	c.Name as ClientName,
	reg.Region as RegionName,
	u.Id as UserId,
	u.Name as UserName,
	c.Registrant as Registrant,
	uu.UpdateDate as UpdateDate
from customers.Clients C
join customers.Users u on u.ClientId = c.id
join usersettings.UserUpdateInfo uu on uu.UserId = u.id
join farm.Regions reg on reg.RegionCode = c.RegionCode
left join orders.OrdersHead oh on oh.`WriteTime`> :orderDate and (oh.`regioncode` & :regionMask > 0) and oh.UserId = u.id
where
	(c.regioncode & :regionMask > 0)
	and uu.`Updatedate` > :updateDateStart
	and uu.`Updatedate` < :updateDateEnd
	and oh.clientcode is null
group by u.id
order by {0} {1};", SortBy, SortDirection))
			.SetParameter("orderDate", OrderDate)
			.SetParameter("updateDateStart", UpdatePeriod.Begin)
			.SetParameter("updateDateEnd", UpdatePeriod.End)
			.SetParameter("regionMask", regionMask)
			.ToList<UpdatedAndDidNotDoOrdersField>();

			RowsCount = result.Count;
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}
	}
}