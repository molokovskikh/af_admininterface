using System;

namespace AdminInterface.Models.Billing
{
	public static class DateTimeExtentions
	{
		public static Period ToPeriod(this DateTime dateTime)
		{
			return (Period)dateTime.Month + 3;
		}
	}
}