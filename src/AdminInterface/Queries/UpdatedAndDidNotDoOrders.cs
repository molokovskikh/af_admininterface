using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;
using Common.Tools;
using AdminInterface.Models.Suppliers;
using NHibernate.Criterion;

namespace AdminInterface.ManagerReportsFilters
{
	public class UpdatedAndDidNotDoOrdersField : BaseItemForTable
	{
		private string _clientId;

		[Display(Name = "Код клиента", Order = 0)]
		public string ClientId
		{
			get
			{
				if (ForExport)
					return _clientId;
				return AppHelper.LinkToNamed(_clientId, "Clients", parameters: new { @params = new { Id = _clientId } });
			}
			set { _clientId = value; }
		}

		private string _clientName;

		[Display(Name = "Наименование клиента", Order = 1)]
		public string ClientName
		{
			get
			{
				if (ForExport)
					return _clientName;
				return AppHelper.LinkToNamed(_clientName, "Clients", parameters: new { @params = new { Id = _clientId } });
			}
			set { _clientName = value; }
		}

		[Display(Name = "Регион", Order = 4)]
		public string RegionName { get; set; }

		private string _userId;

		[Display(Name = "Код пользователя", Order = 2)]
		public string UserId
		{
			get
			{
				if (ForExport)
					return _userId;
				return AppHelper.LinkToNamed(_userId, "Users", parameters: new { @params = new { Id = _userId } });
			}
			set { _userId = value; }
		}

		private string _userName;

		[Display(Name = "Комментарий пользователя", Order = 3)]
		public string UserName
		{
			get
			{
				if (ForExport)
					return _userName;
				return AppHelper.LinkToNamed(_userName, "Users", parameters: new { @params = new { Id = _userId } });
			}
			set { _userName = value; }
		}

		[Display(Name = "Дата обновления", Order = 6)]
		public string UpdateDate { get; set; }

		[Display(Name = "Регистратор", Order = 5)]
		public string Registrant { get; set; }

		[Display(Name = "Дата последнего заказа", Order = 7)]
		public string LastOrderDate { get; set; }

		[Display(Name = "Включенные поставщики", Order = 8)]
		public string EnabledSupCnt { get; set; }

		[Display(Name = "Отключенные поставщики", Order = 9)]
		public string DisabledSupCnt { get; set; }

		[Display(Name = "Список отключенных", Order = 10)]
		public string DisabledSupName { get; set; }

		public bool ForExport;

		[Style]
		public virtual bool IsOldUserUpdate
		{
			get { return (!string.IsNullOrEmpty(UpdateDate) && DateTime.Now.Subtract(DateTime.Parse(UpdateDate)).Days > 7); }
		}
	}

	public class UpdatedAndDidNotDoOrdersFilter : PaginableSortable, IFiltrable<UpdatedAndDidNotDoOrdersField>
	{
		public ulong[] Regions { get; set; }

		public uint[] Suppliers { get; set; }

		public DatePeriod UpdatePeriod { get; set; }

		[Description("Сумма заказов не более")]
		public decimal? Sum { get; set; }

		[Description("Кто не делал заказы вообще")]
		public bool NoOrders { get; set; }

		public IList<UpdatedAndDidNotDoOrdersField> Find()
		{
			return Find(false);
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public UpdatedAndDidNotDoOrdersFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "" },
				{ "ClientName", "" },
				{ "UserId", "" },
				{ "UserName", "" },
				{ "RegionName", "" },
				{ "Registrant", "" },
				{ "UpdateDate", "" },
				{ "LastOrderDate", "" },
				{ "EnabledSupCnt", "" },
				{ "DisabledSupCnt", "" },
				{ "DisabledSupName", "" }
			};
			UpdatePeriod = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			SortBy = "ClientName";
		}

		public string GetRegionNames(ISession session)
		{
			var result = "";
			if (Regions != null && Regions.Any())
				result = session.Query<Region>().Where(x => Regions.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList().Implode();
			return result;
		}

		public string GetSupplierNames(ISession session)
		{
			var result = "";
			if (Suppliers != null && Suppliers.Any())
				result = session.Query<Supplier>().Where(x => Suppliers.Contains(x.Id)).Select(x => x.Name + " - " + x.HomeRegion.Name).OrderBy(x => x).ToList().Implode();
			return result;
		}

		//Здесь join Customers.UserAddresses ua on ua.UserId = u.Id, чтобы отсеять пользователей, у которых нет адресов доставки.

		public IList<UpdatedAndDidNotDoOrdersField> Find(bool forExport)
		{
			var query = new DetachedSqlQuery();
			var sb = new StringBuilder();

			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Regions?.Any() == true)
				regionMask &= Regions.Aggregate(0ul, (current, region) => current | region);

			query.SetParameter("regionMask", regionMask);
			query.SetParameter("updateDateStart", UpdatePeriod.Begin);
			query.SetParameter("updateDateEnd", UpdatePeriod.End);

			sb.Append(@"DROP TEMPORARY TABLE IF EXISTS usr;
CREATE TEMPORARY TABLE usr (INDEX idx(UserId) USING HASH) ENGINE MEMORY
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
join customers.Users u on u.ClientId = c.id and u.PayerId <> 921 and u.OrderRegionMask > 0 and u.SubmitOrders = 0
join usersettings.AssignedPermissions ap on ap.UserId = u.Id
join Customers.UserAddresses ua on ua.UserId = u.Id
join usersettings.UserUpdateInfo uu on uu.UserId = u.id
join farm.Regions reg on reg.RegionCode = c.RegionCode
left join accessright.regionaladmins reg on reg.UserName = c.Registrant
left join usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
left join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = u.id
where
	(c.regioncode & :regionMask > 0)
	and uu.`Updatedate` > :updateDateStart
	and uu.`Updatedate` < :updateDateEnd
	and rcs.InvisibleOnFirm = 0
	and rcs.ServiceClient = 0
	and ap.PermissionId = 1
group by u.id; ");

			// вычисление суммы заказов на адрес
			if (Sum.HasValue && Sum.Value > 0) {
				sb.Append(@"DROP TEMPORARY TABLE IF EXISTS sa;
CREATE TEMPORARY TABLE sa (INDEX idx(UserId) USING HASH) ENGINE MEMORY
select distinct usr.UserId
from usr
left outer join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = usr.UserId and oh.`WriteTime` > :updateDateStart and oh.`WriteTime` < :updateDateEnd
left join orders.orderslist ol on ol.OrderID = oh.RowID
group by usr.UserId, oh.AddressId
having IFNULL(SUM(ol.cost),0) < :sumPerAddress; ");
				query.SetParameter("sumPerAddress", Sum.Value);
			}

			sb.Append(@"
create temporary table SuppliersStat (
	UserId int unsigned not null,
	EnabledSupCnt int not null,
	DisabledSupCnt int not null,
	DisabledSuppliers varchar(255),
	primary key(UserId)
) engine = memory;

insert into SuppliersStat(UserId, EnabledSupCnt, DisabledSupCnt, DisabledSuppliers)
select d.UserId,
	sum(not d.DisabledByUser) as EnabledSupCnt,
	sum(d.DisabledByUser) as DisabledSupCnt,
	d.DisabledSuppliers
from (
	SELECT u.Id as UserId,
		supplier.Id as SupplierId,
		if(up.UserId is null, 1, 0) as DisabledByUser,
		group_concat(if(up.UserId is null, supplier.Name, null)) as DisabledSuppliers
	FROM Customers.Users u
		join usr on usr.UserId = u.Id
		join Customers.Intersection i on i.ClientId = u.ClientId and i.AgencyEnabled = 1
		JOIN Customers.Clients drugstore ON drugstore.Id = i.ClientId
		JOIN usersettings.RetClientsSet r ON r.clientcode = drugstore.Id
		JOIN usersettings.PricesData pd ON pd.pricecode = i.PriceId
		JOIN Customers.Suppliers supplier ON supplier.Id = pd.firmcode
			JOIN usersettings.RegionalData rd ON rd.RegionCode = i.RegionId AND rd.FirmCode = pd.firmcode
		JOIN usersettings.PricesRegionalData prd ON prd.regioncode = i.RegionId AND prd.pricecode = pd.pricecode
		left join Customers.UserPrices up on up.PriceId = i.PriceId and up.UserId = ifnull(u.InheritPricesFrom, u.Id) and up.RegionId = i.RegionId
	WHERE supplier.Disabled = 0
			and (supplier.RegionMask & i.RegionId) > 0
			AND (drugstore.maskregion & i.RegionId & u.WorkRegionMask) > 0
			AND (r.WorkRegionMask & i.RegionId) > 0
			AND pd.agencyenabled = 1
			AND pd.enabled = 1
			AND pd.pricetype <> 1
			AND prd.enabled = 1
			AND if(not r.ServiceClient, supplier.Id != 234, 1)
			and i.AvailableForClient = 1
	group by u.Id, supplier.Id
) as d
group by d.UserId;

select usr.ClientId,
	usr.ClientName,
	usr.RegionName,
	usr.UserId,
	usr.UserName,
	usr.Registrant,
	usr.UpdateDate,
	usr.LastOrderDate,
	ss.EnabledSupCnt,
	ss.DisabledSupCnt,
	DisabledSuppliers as DisabledSupName
from usr
	join SuppliersStat ss on ss.UserId = usr.UserId
	left join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = usr.UserId and oh.`WriteTime` > :updateDateStart and oh.`WriteTime` < :updateDateEnd
		left join usersettings.pricesdata pd on pd.PriceCode = oh.PriceCode
			left join customers.Suppliers s on s.Id = pd.FirmCode
");

			if (Sum.HasValue && Sum.Value > 0)
				sb.AppendLine("join sa on sa.UserId = usr.UserId ");

			var whre = new List<string>();
			if (Suppliers != null && Suppliers.Any()) {
				whre.Add("IFNULL(s.Id,0) not in (:suppliers) ");
				query.SetParameter("suppliers", Suppliers.Implode());
			}
			if (whre.Count > 0)
				sb.AppendLine($"where {whre.Implode(" and ")} ");

			sb.AppendLine("group by usr.UserId ");
			if (NoOrders)
				sb.AppendLine("having COUNT(oh.RowID) = 0 ");
			sb.AppendLine($"order by {SortBy} {SortDirection};");

			query.Sql = sb.ToString();
			var result = query.GetSqlQuery(Session).ToList<UpdatedAndDidNotDoOrdersField>();

			Session.CreateSQLQuery(@"DROP TEMPORARY TABLE IF EXISTS usr;
DROP TEMPORARY TABLE IF EXISTS sa;
DROP TEMPORARY TABLE IF EXISTS SuppliersStat;")
				.ExecuteUpdate();

			RowsCount = result.Count;
			if (forExport)
				return result.ToList();
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}
	}
}