using System;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord("RegionalData", Schema = "Usersettings")]
	public class RegionalData
	{
		public RegionalData()
		{
			ContactInfo = "";
			OperativeInfo = "";
		}

		public RegionalData(Region region, Supplier supplier)
			: this()
		{
			Region = region;
			Supplier = supplier;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[Property(NotNull = true)]
		public virtual string ContactInfo { get; set; }

		[Property(NotNull = true)]
		public virtual string OperativeInfo { get; set; }
	}
}