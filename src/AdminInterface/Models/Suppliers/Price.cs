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

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Cost> Costs { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<PriceRegionalData> RegionalData { get; set; }

		public virtual void AddCost()
		{
			var isBase = Costs.Count == 0;
			Costs.Add(new Cost
			{
				Price = this,
				BaseCost = isBase,
				PriceItem = new PriceItem { FormRule = new FormRule() }
			});
		}

		public override string ToString()
		{
			if (Supplier == null)
				return Name;
			return String.Format("{0} - {1}", Supplier.Name, Name);
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
			Enabled = true;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

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

		[BelongsTo("PriceItemId", Cascade = CascadeEnum.All)]
		public virtual PriceItem PriceItem { get; set; }
	}

	[ActiveRecord("PriceItems", Schema = "Usersettings", Lazy = true)]
	public class PriceItem
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime PriceDate { get; set; }

		[BelongsTo("FormRuleId", Cascade = CascadeEnum.All)]
		public virtual FormRule FormRule { get; set; }
	}

	[ActiveRecord("FormRules", Schema = "Farm", Lazy = true)]
	public class FormRule
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }
	}
}