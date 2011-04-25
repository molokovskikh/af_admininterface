using System;
using System.ComponentModel;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	public enum ServiceType
	{
		[Description("���������")] Supplier = 0,
		[Description("������")] Drugstore = 1,
		[Description("�������")] Reference = 2
	}

	[ActiveRecord(Schema = "Future", Lazy = true), JoinedBase]
	public class Service
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual ServiceType Type { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[BelongsTo]
		public virtual Region HomeRegion  { get; set; }

		[Property]
		public virtual bool Disabled { get; set; }

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