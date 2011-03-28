using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	public enum ServiceType
	{
		Supplier = 0,
		Drugstore = 1,
		Reference = 2
	}

	[ActiveRecord(Schema = "Future"), JoinedBase]
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
	}
}