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
	}
}
