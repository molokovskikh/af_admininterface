using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Tools;
using Common.Tools.Calendar;

namespace AdminInterface.ViewModels.Reports
{
	public class ATableFilter
	{
		protected ATableFilter()
		{
			DateBegin = SystemTime.Now().FirstDayOfMonth();
			DateEnd = SystemTime.Now();
			DataExport = false;
		}

		public DateTime DateBegin { get; set; }
		public DateTime DateEnd { get; set; }

		public bool DataExport { get; set; }
	}
}