using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminInterface.ViewModels
{
	public class DatePeriod
	{
		public DatePeriod()
		{
		}

		public DatePeriod(DateTime dateBegin, DateTime dateEnd)
		{
			DateBegin = dateBegin;
			DateEnd = dateEnd;
		}

		private DateTime _dateBegin { get; set; }
		private DateTime _dateEnd { get; set; }

		[Required(ErrorMessage = "Период задан неверно")]
		[DataType(DataType.DateTime)]
		public DateTime DateBegin
		{
			get { return _dateBegin.Date; }
			set { _dateBegin = value.Date; }
		}

		[Required(ErrorMessage = "Период задан неверно")]
		[DataType(DataType.DateTime)]
		public DateTime DateEnd
		{
			get { return _dateEnd.Date.AddDays(1).AddSeconds(-1); }
			set { _dateEnd = value.Date; }
		}
	}
}