using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Helpers;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;
using Common.Tools;
using AdminInterface.Models.Suppliers;
using ExcelLibrary.SpreadSheet;

namespace AdminInterface.ManagerReportsFilters
{
	public class UpdatedAndDidNotDoOrdersField : BaseItemForTable
	{
		private string _clientId;
		public string InnerUserId;

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

		[Display(Name = "Код пользователя", Order = 2)]
		public string UserId
		{
			get
			{
				if (ForExport)
					return InnerUserId;
				return AppHelper.LinkToNamed(InnerUserId, "Users", parameters: new { @params = new { Id = InnerUserId } });
			}
			set { InnerUserId = value; }
		}

		private string _userName;

		[Display(Name = "Комментарий пользователя", Order = 3)]
		public string UserName
		{
			get
			{
				if (ForExport)
					return _userName;
				return AppHelper.LinkToNamed(_userName, "Users", parameters: new { @params = new { Id = InnerUserId } });
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

		[Display(Name = "Нет заказов на поставщиков", Order = 11)]
		public string NoOrderSuppliers { get; set; }

		public bool ForExport;

		[Style]
		public virtual bool IsOldUserUpdate => (!string.IsNullOrEmpty(UpdateDate) && DateTime.Now.Subtract(DateTime.Parse(UpdateDate)).Days > 7);
	}

	public class UpdatedAndDidNotDoOrdersFilter : PaginableSortable, IFiltrable<UpdatedAndDidNotDoOrdersField>
	{
		public ulong[] Regions { get; set; }

		public uint[] Suppliers { get; set; }

		public DatePeriod UpdatePeriod { get; set; }

		[Description("Сумма заказов не более")]
		public decimal? Sum { get; set; }

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

		public string GetRegionNames()
		{
			var result = "";
			if (Regions != null && Regions.Any())
				result = Session.Query<Region>().Where(x => Regions.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList().Implode();
			return result;
		}

		public string GetSupplierNames()
		{
			var result = "";
			if (Suppliers != null && Suppliers.Any())
				result = Session.Query<Supplier>().Where(x => Suppliers.Contains(x.Id)).Select(x => x.Name + " - " + x.HomeRegion.Name).OrderBy(x => x).ToList().Implode();
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

			sb.AppendLine(@"DROP TEMPORARY TABLE IF EXISTS usr;
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
join customers.Users u on u.ClientId = c.id
	join usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
join usersettings.AssignedPermissions ap on ap.UserId = u.Id
join Customers.UserAddresses ua on ua.UserId = u.Id
join usersettings.UserUpdateInfo uu on uu.UserId = u.id
join farm.Regions reg on reg.RegionCode = c.RegionCode
left join accessright.regionaladmins reg on reg.UserName = c.Registrant
left join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = u.id
where
	(c.regioncode & :regionMask > 0)
	and c.Status = 1
	and uu.`Updatedate` > :updateDateStart
	and uu.`Updatedate` < :updateDateEnd
	and rcs.InvisibleOnFirm = 0
	and rcs.ServiceClient = 0
	and u.Enabled = 1
	and u.PayerId <> 921
	and rcs.OrderRegionMask & u.OrderRegionMask & :regionMask > 0
	and u.SubmitOrders = 0
	and ap.PermissionId in (1, 81)
group by u.id; ");

			// вычисление суммы заказов на адрес
			if (Sum.HasValue && Sum.Value > 0) {
				sb.AppendLine(@"DROP TEMPORARY TABLE IF EXISTS sa;
CREATE TEMPORARY TABLE sa (INDEX idx(UserId) USING HASH) ENGINE MEMORY
select distinct usr.UserId
from usr
left outer join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0) and oh.UserId = usr.UserId and oh.`WriteTime` > :updateDateStart and oh.`WriteTime` < :updateDateEnd
left join orders.orderslist ol on ol.OrderID = oh.RowID
group by usr.UserId, oh.AddressId
having IFNULL(SUM(ol.cost),0) < :sumPerAddress; ");
				query.SetParameter("sumPerAddress", Sum.Value);
			}

			sb.AppendLine(@"
drop temporary table if exists NoOrders;
create temporary table NoOrders (
	UserId int unsigned not null,
	Suppliers varchar(255),
	primary key (UserId)
) engine = memory;
");
			if (Suppliers?.Any() == true) {
				sb.AppendLine(@"
drop temporary table if exists TargetSuppliers;
create temporary table TargetSuppliers (
	SupplierId int unsigned,
	primary key (SupplierId)
) engine = memory;
");
				for(var i = 0; i < Suppliers.Length; i++) {
					sb.AppendLine($"insert into TargetSuppliers(SupplierId) values (:supplier{i});");
					query.SetParameter($"supplier{i}", Suppliers[i]);
				}

				sb.AppendLine(@"
insert into NoOrders(UserId, Suppliers)
select d.UserId, group_concat(d.Name)
from (
		select usr.UserId, s.Name
		from (usr, TargetSuppliers ts)
			join Customers.Suppliers s on s.Id = ts.SupplierId
				join Usersettings.PricesData pd on pd.FirmCode = s.Id
			left outer join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0)
				and oh.UserId = usr.UserId
				and oh.`WriteTime` > :updateDateStart
				and oh.`WriteTime` < :updateDateEnd
				and pd.PriceCode = oh.PriceCode
		group by usr.UserId, s.Id
		having count(oh.RowId) = 0
	) as d
group by d.UserId;");
			} else {
				sb.AppendLine(@"
insert into NoOrders(UserId)
select usr.UserId
from usr
	left outer join orders.OrdersHead oh on (oh.`regioncode` & :regionMask > 0)
		and oh.UserId = usr.UserId
		and oh.`WriteTime` > :updateDateStart
		and oh.`WriteTime` < :updateDateEnd
group by usr.UserId
having count(oh.RowId) = 0;");
			}
			var join = "";
			if (Sum.HasValue && Sum.Value > 0)
				join = "join sa on sa.UserId = usr.UserId ";

			sb.AppendLine($@"
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
	DisabledSuppliers as DisabledSupName,
	no.Suppliers as NoOrderSuppliers
from usr
	join SuppliersStat ss on ss.UserId = usr.UserId
	join NoOrders no on no.UserId = usr.UserId
	{join}
group by usr.UserId
order by {SortBy} {SortDirection}
;");

			query.Sql = sb.ToString();
			var result = query.GetSqlQuery(Session).ToList<UpdatedAndDidNotDoOrdersField>();

			Session.CreateSQLQuery(@"DROP TEMPORARY TABLE IF EXISTS usr;
DROP TEMPORARY TABLE IF EXISTS sa;
drop temporary table if exists NoOrders;
drop temporary table if exists TargetSuppliers;
DROP TEMPORARY TABLE IF EXISTS SuppliersStat;")
				.ExecuteUpdate();

			RowsCount = result.Count;
			if (forExport)
				return result.ToList();
			return result.Skip(CurrentPage * PageSize).Take(PageSize).ToList();
		}

		public byte[] Excel()
		{
			var wb = new Workbook();
			var ws = new Worksheet("Кто обновлялся и не делал заказы");

			var row = 0;
			var colShift = 0;

			ws.Merge(row, 0, row, 6);
			ExcelHelper.WriteHeader1(ws, row, 0, "Кто обновлялся и не делал заказы", false, true);
			row++; // 1

			ws.Merge(row, 1, row, 2);
			ExcelHelper.Write(ws, row, 0, "Регион:", false);
			string regionName;
			if (Regions != null && Regions.Any())
				regionName = GetRegionNames();
			else {
				regionName = "Все";
			}
			ExcelHelper.Write(ws, row, 1, regionName, false);
			row++; // 2

			ws.Merge(row, 1, row, 2);
			ExcelHelper.Write(ws, row, 0, "Период:", false);
			if (UpdatePeriod.Begin != UpdatePeriod.End)
				ExcelHelper.Write(ws, row, 1,
					"С " + UpdatePeriod.Begin.ToString("dd.MM.yyyy") + " по " + UpdatePeriod.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, row, 1, "За " + UpdatePeriod.Begin.ToString("dd.MM.yyyy"), false);
			row++; // 3

			if (Sum.HasValue) {
				ws.Merge(row, 0, row, 3);
				ExcelHelper.Write(ws, row, 0, "Сумма заказов на адрес не более: " + Sum.Value, false);
				row++;
			}

			if (Suppliers != null && Suppliers.Any()) {
				ws.Merge(row, 0, row, 3);
				var supplierName = GetSupplierNames();
				ExcelHelper.Write(ws, row, 0, "Те, у кого нет заказов на поставщиков: " + supplierName, false);
				row++;
			}

			ExcelHelper.WriteHeader1(ws, row, 0, "Код клиента", true, true);
			ExcelHelper.WriteHeader1(ws, row, 1, "Наименование клиента", true, true);
			ExcelHelper.WriteHeader1(ws, row, 2, "Код пользователя", true, true);
			ExcelHelper.WriteHeader1(ws, row, 3, "Комментарий пользователя", true, true);
			ExcelHelper.WriteHeader1(ws, row, 4, "Регион", true, true);
			ExcelHelper.WriteHeader1(ws, row, 5, "Регистратор", true, true);
			ExcelHelper.WriteHeader1(ws, row, 6, "Дата обновления", true, true);
			ExcelHelper.WriteHeader1(ws, row, 7, "Дата последнего заказа", true, true);
			ExcelHelper.WriteHeader1(ws, row, 8, "Включенные поставщики", true, true);
			ExcelHelper.WriteHeader1(ws, row, 9, "Отключенные поставщики", true, true);
			ExcelHelper.WriteHeader1(ws, row, 10, "Список отключенных", true, true);
			ExcelHelper.WriteHeader1(ws, row, 11, "Нет заказов на поставщиков", true, true);

			ws.Cells.ColumnWidth[0] = 4000;
			ws.Cells.ColumnWidth[1] = 12000;
			ws.Cells.ColumnWidth[2] = 4000;
			ws.Cells.ColumnWidth[3] = 12000;
			ws.Cells.ColumnWidth[4] = 6000;
			ws.Cells.ColumnWidth[5] = 8000;
			ws.Cells.ColumnWidth[6] = 6000;
			ws.Cells.ColumnWidth[7] = 6000;
			ws.Cells.ColumnWidth[8] = 6000;
			ws.Cells.ColumnWidth[9] = 6000;
			ws.Cells.ColumnWidth[10] = 6000;
			ws.Cells.ColumnWidth[11] = 6000;

			ws.Cells.Rows[row].Height = 514;
			row++;

			var reportData = Find(true);
			foreach (var item in reportData) {
				item.ForExport = true;
				ExcelHelper.Write(ws, row, colShift + 0, item.ClientId, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.ClientName, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.UserId, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.UserName, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 5, item.Registrant, true);
				ExcelHelper.Write(ws, row, colShift + 6, item.UpdateDate, true);
				ExcelHelper.Write(ws, row, colShift + 7, item.LastOrderDate, true);
				ExcelHelper.Write(ws, row, colShift + 8, item.EnabledSupCnt, true);
				ExcelHelper.Write(ws, row, colShift + 9, item.DisabledSupCnt, true);
				ExcelHelper.Write(ws, row, colShift + 10, item.DisabledSupName, true);
				ExcelHelper.Write(ws, row, colShift + 11, item.NoOrderSuppliers, true);
				row++;
			}

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}
	}
}