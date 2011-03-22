using System;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future", Table = "Suppliers")]
	public class ServiceSupplier : Service
	{
		[JoinedKey]
		public virtual uint SupplierId { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property]
		public virtual ulong RegionMask { get; set; }

		[BelongsTo]
		public virtual Region HomeRegion { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual DateTime? RegistrationDate { get; set; }

		[BelongsTo("ContactGroupOwnerId")]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }
	}
}