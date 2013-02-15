using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "ReorderingRules", Schema = "usersettings")]
	public class ReorderSchedule
	{
		public ReorderSchedule()
		{
		}

		public ReorderSchedule(RegionalData regionalData, DayOfWeek day)
		{
			RegionalData = regionalData;
			DayOfWeek = day;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("RegionalDataId")]
		public virtual RegionalData RegionalData { get; set; }

		[Property(SqlType = "UnitsEnum", ColumnType = "Common.Web.Ui.Helpers.DayOfWeekEnumMapper, Common.Web.Ui")]
		public virtual DayOfWeek DayOfWeek { get; set; }

		[Property]
		public virtual TimeSpan? TimeOfStopsOrders { get; set; }
	}
}