using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future", Table = "Suppliers")]
	public class Supplier : Service, IEnablable
	{
		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property(NotNull = true)]
		public override string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property, Description("������� ������")]
		public virtual ulong RegionMask { get; set; }

		[Property]
		public override bool Disabled { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual DateTime? RegistrationDate { get; set; }

		[BelongsTo]
		public virtual Payer Payer { get; set; }

		[BelongsTo]
		public override Region HomeRegion { get; set; }

		[BelongsTo("ContactGroupOwnerId", Cascade = CascadeEnum.All)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

/*
 * [HasMany(ColumnKey = "RootService", Lazy = true, Inverse = true, MapType = typeof(User))]
		public virtual IList<User> Users { get; set; }
 */

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true)]
		public virtual IList<Price> Prices { get; set; }

		public bool Enabled
		{
			get { return !Disabled; }
		}

		public static IList<Supplier> GetByPayerId(uint payerId)
		{
			return Queryable
				.Where(p => p.Payer.PayerID == payerId).OrderBy(s => s.Name)
				.ToList();
		}

		public static IOrderedQueryable<Supplier> Queryable
		{
			get
			{
				return ActiveRecordLinqBase<Supplier>.Queryable;
			}
		}

		public static Supplier Find(uint id)
		{
			return ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
		}

		public void Save()
		{
			ActiveRecordMediator.Save(this);
		}

		public override string ToString()
		{
			return Name;
		}
	}

/*	public class SupplierUser : User
	{
		public Supplier
	}*/
}