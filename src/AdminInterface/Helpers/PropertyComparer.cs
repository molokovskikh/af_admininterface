using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Collections.Generic;

namespace AdminInterface.Helpers
{
	public class PropertyComparer<T> : IComparer<T>
	{
		private SortDirection _sortDirection;
		private string _propertyName;

		public PropertyComparer(SortDirection sortDirection, string propertyName)
		{
			_sortDirection = sortDirection;
			_propertyName = propertyName;
		}

		public int Compare(T x, T y)
		{
			if (typeof(T).GetProperty(_propertyName) == null)
				throw new Exception(String.Format("Given property is not part of the type {0}", _propertyName));

			object objX = typeof(T).GetProperty(_propertyName).GetValue(x, null);
			object objY = typeof(T).GetProperty(_propertyName).GetValue(y, null);

			int retVal = default(int);
			retVal = ((IComparable)objX).CompareTo((IComparable)objY);

			if (_sortDirection == SortDirection.Descending)
				retVal = -retVal;

			return retVal;
		}
	}
}
