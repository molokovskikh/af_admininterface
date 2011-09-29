using System;
using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	public enum ServiceType
	{
		[Description("Поставщик")] Supplier = 0,
		[Description("Аптека")] Drugstore = 1
	}

	[ActiveRecord(Schema = "Future", Lazy = true), JoinedBase]
	public class Service : IEnablable
	{
		protected bool _disabled;

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual ServiceType Type { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[BelongsTo]
		public virtual Region HomeRegion { get; set; }

		[Property(Access = PropertyAccess.FieldLowercaseUnderscore)]
		public virtual bool Disabled { get; set; }

		public virtual bool Enabled
		{
			get { return !Disabled; }
		}

		public virtual string FullName
		{
			get
			{
				if (this is Client)
				{
					return ((Client)this).FullName;
				}
				else if (this is Supplier)
				{
					return ((Supplier)this).FullName;
				}
				return "";
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual string GetHumanReadableType()
		{
			return BindingHelper.GetDescription(Type);
		}
	}
}