using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("WaybillExcludeFile", Schema = "usersettings")]
	public class WaybillExcludeFile
	{
		public WaybillExcludeFile()
		{
		}

		public WaybillExcludeFile(string mask, Supplier supplier)
		{
			Mask = mask;
			Supplier = supplier;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Mask { get; set; }

		[BelongsTo]
		public virtual Supplier Supplier { get; set; }
	}
}