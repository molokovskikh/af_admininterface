using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Queries
{
	public class MiniMailFilter : PaginableSortable
	{
		public DatePeriod Period { get; set; }
		public uint SupplierId { get; set; }
		public string SupplierName { get; set; }
	}
}