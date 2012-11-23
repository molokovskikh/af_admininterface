using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("Clients", Schema = "Customers")]
	public class ClientForReading
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region HomeRegion { get; set; }
	}
}