﻿using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "3")]
	public class SupplierAccount : Account
	{
		public SupplierAccount()
		{
		}

		public SupplierAccount(Supplier supplier)
		{
			Supplier = supplier;
			_readyForAccounting = true;
			if (supplier.HomeRegion != null
				&& supplier.HomeRegion.Name.Contains("Справка"))
				_payment = 600;
		}

		[BelongsTo("ObjectId")]
		public virtual Supplier Supplier { get; set; }

		public override Payer Payer
		{
			get { return Supplier.Payer; }
		}

		public override string Name
		{
			get { return Supplier.Name; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.Supplier; }
		}

		public override bool ShouldPay()
		{
			return Supplier.Enabled && base.ShouldPay();
		}

		public override uint ObjectId
		{
			get { return Supplier.Id; }
		}

		public override bool Enabled
		{
			get { return Supplier.Enabled; }
		}

		public override string DefaultDescription
		{
			get
			{
				if (Payer.Recipient != null)
					return Payer.Recipient.SupplierDescription;
				return "";
			}
		}
	}
}