using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future", Table = "Suppliers")]
	public class Supplier : Service, IEnablable
	{
		public Supplier()
		{
			Registration = new RegistrationInfo();
			OrderRules = new List<OrderSendRules>();
			Type = ServiceType.Supplier;
		}

		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property, ValidateNonEmpty]
		public override string Name { get; set; }

		[Property, ValidateNonEmpty]
		public virtual string FullName { get; set; }

		[Property, Description("Регионы работы")]
		public virtual ulong RegionMask { get; set; }

		[Property]
		public override bool Disabled { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set;}

		[BelongsTo(Cascade = CascadeEnum.All)]
		public virtual Payer Payer { get; set; }

		[BelongsTo]
		public override Region HomeRegion { get; set; }

		[BelongsTo("ContactGroupOwnerId", Cascade = CascadeEnum.All)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		public virtual Segment Segment
		{
			get
			{
				return Segment.Wholesale;
			}
		}

/*
	[HasMany(ColumnKey = "RootService", Lazy = true, Inverse = true, MapType = typeof(User))]
	public virtual IList<User> Users { get; set; }

	public class SupplierUser : User
	{
		public Supplier
	}
*/

		[HasMany(ColumnKey = "PriceCode", Inverse = true, Lazy = true)]
		public virtual IList<Price> Prices { get; set; }

		[HasMany]
		public virtual IList<OrderSendRules> OrderRules { get; set; }

		public IList<User> Users
		{
			get
			{
				return User.Queryable.Where(u => u.RootService == this).ToList();
			}
		}

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

		public virtual string GetEmailsForBilling()
		{
			return ContactGroupOwner
				.GetEmails(ContactGroupType.Billing)
				.Implode();
		}

		public virtual void AddComment(string comment)
		{
			if (String.IsNullOrEmpty(comment))
				return;

			Payer.AddComment(comment);
			new ClientInfoLogEntity(comment, this).Save();
		}
	}
}