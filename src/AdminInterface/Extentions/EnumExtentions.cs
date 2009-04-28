using System;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Extentions
{
	public static class EnumExtentions
	{
		public static string Description(this Enum enumValue)
		{
			return BindingHelper.GetDescription(enumValue);
		}

		public static object IfDbNull(this object value, object newValue)
		{
			if (value == DBNull.Value)
				return newValue;
			return value;
		}

	}
}
