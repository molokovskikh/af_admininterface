using System;
using System.Collections.Generic;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Common.Web.Ui.Queries;
using NHibernate;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Orders", Table = "OrdersHead")]
	public class ClientOrder : ActiveRecordLinqBase<ClientOrder>
	{
		public ClientOrder()
		{}

		public ClientOrder(User user, Price price)
		{
			Client = user.Client;
			Region = Client.HomeRegion;
			Address = user.AvaliableAddresses[0];
			User = user;
			Price = price;
			Submited = true;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client Client { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }

		[Property]
		public virtual bool Submited { get; set; }

		public IList<OrderLineView> Lines(ISession session)
		{
			return session.CreateSQLQuery(String.Format(@"
select
	ifnull(s.Synonym, {0}) as Product,
	ifnull(sfc.Synonym, p.Name) as Producer,
	ol.Quantity,
	ol.Cost
from Orders.OrdersList ol
	left join Farm.Synonym s on s.SynonymCode = ol.SynonymCode
	left join Farm.SynonymFirmCr sfc on sfc.SynonymFirmCrCode = ol.SynonymFirmCrCode
	left join Catalogs.Producers p on p.Id = ol.CodeFirmCr
where ol.OrderId = :id
", String.Format(Constants.CatalogNameSubquery, "ol.ProductId")))
				.SetParameter("id", Id)
				.ToList<OrderLineView>();
		}
	}

	[ActiveRecord(Schema = "Orders", Table = "OrdersList")]
	public class OrderLine
	{
		public OrderLine()
		{}

		public OrderLine(ClientOrder order, Product product, decimal cost, decimal quantity)
		{
			Order = order;
			Product = product;
			Cost = cost;
			Quantity = quantity;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("OrderId")]
		public virtual ClientOrder Order { get; set; }

		[BelongsTo("ProductId")]
		public virtual Product Product { get; set; }

		[Property]
		public virtual decimal Cost { get; set; }

		[Property]
		public virtual decimal Quantity { get; set; }
	}

	public class OrderLineView
	{
		public string Product { get; set; }
		public string Producer { get; set; }
		public uint Quantity { get; set; }
		public decimal Cost { get; set; }
	}
}