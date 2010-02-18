using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Web.Ui.Helpers
{
	public static class ControllerHelper
	{
		public static void InitParameter<T>(ref Nullable<T> param, string name, T defaultValue, IDictionary propertyBag)
			where T : struct
		{
			if (!param.HasValue)
				param = defaultValue;
			propertyBag[name] = param.Value;
		}
	}
}
