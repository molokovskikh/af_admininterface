using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using AdminInterface.Models.Listeners;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Suppliers
{
	public class InvalidateMatrix : TriggerQueryAttribute
	{
		public override void Trigger(ISession session, object entity, object oldState)
		{
			if (oldState == null)
				return;

			session.CreateSQLQuery(@"
delete from Farm.BuyingMatrix where MatrixId = :matrixId and priceId = :priceId;

update Usersettings.AnalitFReplicationInfo r
	join Customers.Users u on u.Id = r.UserId
		join Usersettings.RetClientsSet rcs on rcs.ClientCode = u.ClientId
set r.ForceReplication = 1
where rcs.BuyingMatrix = :matrixId
	or rcs.OfferMatrix = :matrixId;")
				.SetParameter("priceId", ((Price)entity).Id)
				.SetParameter("matrixId", ((Matrix)oldState).Id)
				.ExecuteUpdate();
		}
	}

	public enum PriceType
	{
		Regular,
		Assortment,
		Vip
	}

	[ActiveRecord("matrices", Schema = "Farm")]
	public class Matrix
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }
	}


	[ActiveRecord("PricesData", Schema = "Usersettings", Lazy = true)]
	public class Price
	{
		public Price()
		{
			Costs = new List<Cost>();
			RegionalData = new List<PriceRegionalData>();
		}

		[PrimaryKey("PriceCode")]
		public virtual uint Id { get; set; }

		[Property("PriceName"), Description("Название"), ValidateNonEmpty]
		public virtual string Name { get; set; }

		[Property]
		public virtual PriceType PriceType { get; set; }

		[Property]
		public virtual int? CostType { get; set; }

		[Property, SetForceReplication]
		public virtual bool AgencyEnabled { get; set; }

		[Property, SetForceReplication]
		public virtual bool Enabled { get; set; }

		[Description("Матрица закупок")]
		public virtual bool IsMatrix { get; set; }

		[Property]
		public virtual decimal UpCost { get; set; }

		[Property, Description("Прайс содержит забраковку")]
		public virtual bool IsRejects { get; set; }

		[Property, Description("Прайс содержит разбраковку")]
		public virtual bool IsRejectCancellations { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate), InvalidateMatrix]
		public virtual Matrix Matrix { get; set; }

		[Description("Объединить с матрицей прайс листа")]
		public virtual Price JoinWithPriceInMatrix { get; set; }

		[BelongsTo, Description("Фильтровать по коду ОКП с помощью прайс-листа")]
		public virtual Price CodeOkpFilterPrice { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[BelongsTo("ParentSynonym")]
		public virtual Price ParentSynonym { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Cost> Costs { get; set; }

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<PriceRegionalData> RegionalData { get; set; }

		public virtual Cost AddCost()
		{
			var cost = new Cost {
				Price = this,
				Name = "Базовая",
			};

			if (CostType == 0) {
				cost.PriceItem = Costs.Select(c => c.PriceItem).FirstOrDefault();
			}

			if (cost.PriceItem == null) {
				cost.PriceItem = new PriceItem {
					FormRule = new FormRule(),
					Source = new PriceSource()
				};
			}
			cost.CostFormRule = new CostFormRule { Cost = cost, FieldName = "" };
			Costs.Add(cost);
			return cost;
		}

		public override string ToString()
		{
			if (Supplier == null)
				return Name;
			return String.Format("{0} - {1}", Supplier.Name, Name);
		}

		public virtual void PrepareEdit(ISession dbSession)
		{
			IsMatrix = Matrix != null;
			if (Matrix != null)
				JoinWithPriceInMatrix = dbSession.Query<Price>().FirstOrDefault(p => p.Matrix == Matrix && p != this);
		}

		public virtual void PrepareSave()
		{
			if (IsMatrix) {
				if (JoinWithPriceInMatrix != null)
					Matrix = JoinWithPriceInMatrix.Matrix;

				if (Matrix == null)
					Matrix = new Matrix();
			}
			else {
				Matrix = null;
				JoinWithPriceInMatrix = null;
				CodeOkpFilterPrice = null;
			}
		}
	}

	[ActiveRecord("PricesRegionalData", Schema = "Usersettings")]
	public class PriceRegionalData
	{
		public PriceRegionalData()
		{
		}

		public PriceRegionalData(Price price, Region region)
		{
			Price = price;
			Region = region;
			Enabled = true;
			Cost = price.Costs[0];
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region { get; set; }

		[BelongsTo("BaseCost")]
		public Cost Cost { get; set; }
	}

	[ActiveRecord("PricesCosts", Schema = "Usersettings", Lazy = true)]
	public class Cost
	{
		[PrimaryKey("CostCode")]
		public virtual uint Id { get; set; }

		[Property("CostName")]
		public virtual string Name { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }

		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual CostFormRule CostFormRule { get; set; }

		[BelongsTo("PriceItemId", Cascade = CascadeEnum.All)]
		public virtual PriceItem PriceItem { get; set; }

		public virtual bool IsConfigured
		{
			get { return !String.IsNullOrEmpty(CostFormRule.FieldName); }
		}
	}

	[ActiveRecord("pricefmts", Schema = "Farm", Lazy = true)]
	public class PriceFormat
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string FileExtention { get; set; }
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

		[BelongsTo("SourceId", Cascade = CascadeEnum.All, NotFoundBehaviour = NotFoundBehaviour.Ignore)]
		public virtual PriceSource Source { get; set; }

		[HasMany]
		public virtual IList<DownloadLog> DownloadLogs { get; set; }
	}

	[ActiveRecord("Sources", Schema = "Farm", Lazy = true)]
	public class PriceSource
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }
	}

	[ActiveRecord("FormRules", Schema = "Farm", Lazy = true)]
	public class FormRule
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("PriceFormatId", Cascade = CascadeEnum.All,
			NotFoundBehaviour = NotFoundBehaviour.Ignore, Lazy = FetchWhen.OnInvoke)]
		public virtual PriceFormat Format { get; set; }

		[Property]
		public virtual string FCode { get; set; }
	}

	[ActiveRecord("downlogs", Schema = "logs")]
	public class DownloadLog
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[BelongsTo("PriceItemId")]
		public virtual PriceItem PriceItem { get; set; }

		[Property]
		public virtual string Host { get; set; }

		[Property]
		public virtual string Addition { get; set; }

		[Property]
		public virtual uint ResultCode { get; set; }

		[Property]
		public virtual string ArchFileName { get; set; }

		[Property]
		public virtual string ExtrFileName { get; set; }
	}

	[ActiveRecord("CostFormRules", Schema = "Farm", Lazy = true)]
	public class CostFormRule
	{
		[PrimaryKey("CostCode", Generator = PrimaryKeyType.Foreign)]
		public virtual uint Id { get; set; }

		[OneToOne]
		public virtual Cost Cost { get; set; }

		[Property(NotNull = true)]
		public virtual string FieldName { get; set; }

		[Property]
		public virtual string TxtBegin { get; set; }

		[Property]
		public virtual string TxtEnd { get; set; }
	}
}