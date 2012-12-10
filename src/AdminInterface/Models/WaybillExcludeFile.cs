using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.Components.Validator;

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

		[Property, ValidateNonEmpty("Введите маску файла")]
		public virtual string Mask { get; set; }

		[BelongsTo]
		public virtual Supplier Supplier { get; set; }
	}
}