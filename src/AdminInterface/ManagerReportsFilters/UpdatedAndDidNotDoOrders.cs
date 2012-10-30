using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;

namespace AdminInterface.ManagerReportsFilters
{
	public class UpdatedAndDidNotDoOrdersField : BaseItemForTable
	{
		private string _clientId;

		[Display(Name = "Код клиента", Order = 0)]
		public string ClientId
		{
			get { return Link(_clientId, "Clients", new Tuple<string, object>("Id", _clientId)); }
			set { _clientId = value; }
		}

		private string _clientName;

		[Display(Name = "Наименование клиента", Order = 1)]
		public string ClientName
		{
			get { return Link(_clientName, "Clients", new Tuple<string, object>("Id", _clientId)); }
			set { _clientName = value; }
		}

		[Display(Name = "Регион", Order = 4)]
		public string RegionName { get; set; }
		[Display(Name = "Код пользователя", Order = 2)]
		public uint UserId { get; set; }

		private string _userName;

		[Display(Name = "Комментарий пользователя", Order = 3)]
		public string UserName
		{
			get { return Link(_userName, "Users", new Tuple<string, object>("Id", UserId)); }
			set { _userName = value; }
		}

		[Display(Name = "Дата обновления", Order = 6)]
		public string UpdateDate { get; set; }
		[Display(Name = "Регистратор", Order = 5)]
		public string Registrant { get; set; }
		[Display(Name = "Дата последнего заказа", Order = 7)]
		public string LastOrderDate { get; set; }

		[Style]
		public virtual bool IsOldUserUpdate
		{
			get { return (!string.IsNullOrEmpty(UpdateDate) && DateTime.Now.Subtract(DateTime.Parse(UpdateDate)).Days > 7); }
		}
	}

	public class UpdatedAndDidNotDoOrdersFilter : PaginableSortable, IFiltrable<UpdatedAndDidNotDoOrdersField>
	{
		public Region Region { get; set; }
		[Description("Не делались заказы с")]
		public DateTime OrderDate { get; set; }
		public DatePeriod UpdatePeriod { get; set; }

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public UpdatedAndDidNotDoOrdersFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" },
				{ "UserId", "u.Id" },
				{ "UserName", "u.Name" }
			};
			OrderDate = DateTime.Now.AddDays(-7);
			UpdatePeriod = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			SortBy = "c.Name";
		}

		//Здесь join Customers.UserAddresses ua on ua.UserId = u.Id, чтобы отсеять пользователей, у которых нет адресов доставки.
		public IList<UpdatedAndDidNotDoOrdersField> Find()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var result = Session.CreateSQLQuery(string.Format(@"
select
	c.id as ClientId,
	c.Name as ClientName,
	reg.Region as RegionName,
	u.Id as UserId,
	u.Name as UserName,
	if (reg.ManagerName is not null, reg.ManagerName, c.Registrant) as Registrant,
	uu.UpdateDate as UpdateDate,
	max(oh.`WriteTime`) as LastOrderDate
from customers.Clients C
left join accessright.regionaladmins reg on reg.UserName = c.Registrant
left join usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
join customers.Users u on u.ClientId = c.id and u.PayerId <> 921 and u.OrderRegionMask > 0 and u.SubmitOrders = 0
join usersettings.AssignedPermissions ap on ap.UserId = u.Id
join Customers.UserAddresses ua on ua.UserId = u.Id
join usersettings.UserUpdateInfo uu on uu.UserId = u.id
join farm.Regions reg on reg.RegionCode = c.RegionCode
left join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = u.id
where
	(c.regioncode & :regionMask > 0)
	and uu.`Updatedate` > :updateDateStart
	and uu.`Updatedate` < :updateDateEnd
	and rcs.InvisibleOnFirm = 0
	and rcs.ServiceClient = 0
	and ap.PermissionId = 1
group by u.id
having max(oh.`WriteTime`) < :orderDate
order by {0} {1}
;", SortBy, SortDirection))
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