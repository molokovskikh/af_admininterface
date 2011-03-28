using System;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future", Table = "Suppliers")]
	public class ServiceSupplier : Service
	{
		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property]
		public override string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property]
		public virtual ulong RegionMask { get; set; }

		[Property]
		public override bool Disabled { get; set; }

		[BelongsTo]
		public override Region HomeRegion { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual DateTime? RegistrationDate { get; set; }

		[BelongsTo("ContactGroupOwnerId")]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }
	}
}