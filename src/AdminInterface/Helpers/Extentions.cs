using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminInterface.Helpers
{
	public static class Extentions
	{
		public static IEnumerable<T> Sort<T>(this IEnumerable<T> collection,
		                                     ref string property,
		                                     ref string direction,
		                                     string defaultProperty)
		{
			if (String.IsNullOrEmpty(direction))
				direction = "ascending";

			direction = direction.ToLower();

			if (direction != "ascending" && direction != "descending")
				direction = "ascending";

			if (String.IsNullOrEmpty(property))
				property = defaultProperty;

			var propertyInfo = typeof (T).GetProperty(property);
			if (propertyInfo == null)
				property = defaultProperty;

			propertyInfo = typeof(T).GetProperty(property);
			if (direction == "ascending")
				return collection.OrderBy(i => propertyInfo.GetValue(i, null));

			return collection.OrderByDescending(i => propertyInfo.GetValue(i, null));
		}
	}
}
