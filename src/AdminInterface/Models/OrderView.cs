using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Transform;

namespace AdminInterface.Models
{
	[ActiveRecord(SchemaAction = "none")]
	public class OrderView : ActiveRecordBase<OrderView>
	{
		[PrimaryKey]
		public ulong Id { get; set; }

		[Property]
		public DateTime WriteTime { get; set; }

		[Property]
		public DateTime PriceDate { get; set; }

		[Property]
		public string Customer { get; set; }

		[Property]
		public string Supplier { get; set; }

		[Property]
		public string PriceName { get; set; }

		[Property]
		public uint RowCount { get; set; }

		[Property]
		public DateTime SubmitDate { get; set; }

		[Property]
		public int? ClientOrderId { get; set; }

		public static IList<OrderView> FindNotSendedOrders()
		{
			return ArHelper.WithSession(s => s
			                          	.CreateSQLQuery(@"
SELECT  oh.rowid as {OrderView.Id}, 
        oh.WriteTime as {OrderView.WriteTime}, 
        oh.PriceDate as {OrderView.PriceDate}, 
        c.Name as {OrderView.Customer}, 
        s.Name as {OrderView.Supplier}, 
        pd.PriceName as {OrderView.PriceName}, 
        oh.RowCount as {OrderView.RowCount}, 
		oh.SubmitDate as {OrderView.SubmitDate},
		oh.ClientOrderId as {OrderView.ClientOrderId}
FROM    orders.ordershead oh
		join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
			join Customers.Suppliers as s on s.Id = pd.firmcode 
		join Customers.clients as c on oh.clientcode = c.Id
WHERE   oh.RegionCode & :RegionCode > 0
		and oh.Deleted = 0
		and oh.Submited = 1
		and oh.Processed = 0
group by oh.rowid
ORDER BY oh.WriteTime desc;")
										.AddEntity(typeof(OrderView))
										.SetParameter("RegionCode", SecurityContext.Administrator.RegionMask)
										.List<OrderView>());
		}
	}
}
