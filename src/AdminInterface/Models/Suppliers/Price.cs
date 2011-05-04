using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	public enum PriceType
	{
		Regular,
		Assortment,
		Vip
	}

	[ActiveRecord("PricesData", Schema = "Usersettings", Lazy = true)]
	public class Price : ActiveRecordLinqBase<Price>
	{
		public Price()
		{
			Costs = new List<Cost>();
			RegionalData = new List<PriceRegionalData>();
		}

		[PrimaryKey("PriceCode")]
		public virtual uint Id { get; set; }

		[Property("PriceName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual PriceType PriceType { get; set; }

		[Property]
		public virtual int? CostType { get; set; }

		[Property]
		public virtual bool AgencyEnabled { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

		[Property]
		public virtual decimal UpCost { get; set; }

		[Property]
		public virtual bool AlowInt { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Cost> Costs { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<PriceRegionalData> RegionalData { get; set; }

		public virtual void AddCost()
		{
			var isBase = Costs.Count == 0;
			Costs.Add(new Cost{ Price = this, BaseCost = isBase });
		}
	}

	[ActiveRecord("PricesRegionalData", Schema = "Usersettings")]
	public class PriceRegionalData
	{
		public PriceRegionalData()
		{}

		public PriceRegionalData(Price price, Region region)
		{
			Price = price;
			Region = region;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region {get; set; }
	}

	[ActiveRecord("PricesCosts", Schema = "Usersettings", Lazy = true)]
	public class Cost
	{
		[PrimaryKey("CostCode")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual bool BaseCost { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }
	}
}