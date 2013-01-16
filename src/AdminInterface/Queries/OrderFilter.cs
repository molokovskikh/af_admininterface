using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Queries
{
	public class OrderFilter
	{
		public Client Client { get; set; }
		public User User { get; set; }
		public Address Address { get; set; }
		public Supplier Supplier { get; set; }
		public DatePeriod Period { get; set; }
		public bool NotSent { get; set; }

		public IList<User> Users
		{
			get { return Client.Users.OrderBy(u => u.GetLoginOrName()).ToList(); }
		}

		public IList<Address> Addresses
		{
			get { return Client.Addresses.OrderBy(u => u.Name).ToList(); }
		}

		public OrderFilter()
		{
			Period = new DatePeriod {
				Begin = DateTime.Today,
				End = DateTime.Today
			};
		}

		public List<OrderLog> Find()
		{
			return ArHelper.WithSession(s => {
				var sqlFilter = "(oh.writetime >= :FromDate AND oh.writetime <= ADDDATE(:ToDate, INTERVAL 1 DAY))";
				if (NotSent)
					sqlFilter = "oh.Deleted = 0 and oh.Submited = 1 and oh.Processed = 0";

				if (User != null)
					sqlFilter += "and oh.UserId = :UserId ";

				if (Address != null)
					sqlFilter += "and oh.AddressId = :AddressId ";

				if (Client != null)
					sqlFilter += "and oh.ClientCode = :ClientId ";

				if (Supplier != null)
					sqlFilter += "and pd.FirmCode = :SupplierId";

				var query = s.CreateSQLQuery(String.Format(@"
SELECT  oh.rowid as Id,
		oh.WriteTime,
		oh.PriceDate,
		c.Name as Drugstore,
		a.Address,
		a.Id as AddressId,
		u.Id as UserId,
		if (u.Name is not null and length(u.Name) > 0, u.Name, u.Login) as User,
		s.Name as Supplier,
		pd.PriceName,
		pd.PriceCode PriceId,
		oh.RowCount,
		(select oi.ResultCode from Logs.Orders oi where oi.RowId = max(o.RowId)) as ResultCode,
		(select oi.TransportType from Logs.Orders oi where oi.RowId = max(o.RowId)) as TransportType,
		oh.ClientOrderId,
		reg.Region,
		oh.Submited,
		oh.Deleted
FROM orders.ordershead oh
	join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
	join Customers.Suppliers as s on s.Id = pd.firmcode
	join Customers.Clients c on oh.ClientCode = c.Id
	join Customers.Users u on u.Id = oh.UserId
	join Customers.Addresses a on a.Id = oh.AddressId
		left join logs.orders o on oh.rowid = o.orderid
	join Farm.Regions reg on reg.RegionCode = oh.RegionCode
WHERE {0} and oh.RegionCode & :RegionCode > 0
group by oh.rowid
ORDER BY writetime desc", sqlFilter))
					.SetParameter("RegionCode", SecurityContext.Administrator.RegionMask);

				if (!NotSent)
					query
						.SetParameter("FromDate", Period.Begin)
						.SetParameter("ToDate", Period.End);

				if (User != null)
					query.SetParameter("UserId", User.Id);

				if (Address != null)
					query.SetParameter("AddressId", Address.Id);

				if (Client != null)
					query.SetParameter("ClientId", Client.Id);

				if (Supplier != null)
					query.SetParameter("SupplierId", Supplier.Id);

				return query.ToList<OrderLog>();
			}).ToList();
		}
	}
}