using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace AdminInterface.Helpers
{
	public class PropertyComparer<T> : IComparer<T>
	{
		private readonly SortDirection _sortDirection;
		private readonly string _propertyName;

		public PropertyComparer(SortDirection sortDirection, string propertyName)
		{
			_sortDirection = sortDirection;
			_propertyName = propertyName;
		}

		public int Compare(T x, T y)
		{
			if (typeof(T).GetProperty(_propertyName) == null)
				throw new Exception(String.Format("Given property is not part of the type {0}", _propertyName));

			var objX = typeof(T).GetProperty(_propertyName).GetValue(x, null);
			var objY = typeof(T).GetProperty(_propertyName).GetValue(y, null);

			int retVal = ((IComparable)objX).CompareTo(objY);

			if (_sortDirection == SortDirection.Descending)
				retVal = -retVal;

			return retVal;
		}
	}
}
