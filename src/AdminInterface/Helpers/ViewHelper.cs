using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace AdminInterface.Helpers
{
	public class ViewHelper
	{
		public static string ConvertToUserFriendlySize(ulong size)
		{
			if (size == 0)
				return "-";
			if (size < 1024)
				return size + " ม";
			else if (size < 1048576)
				return (size / 1024f).ToString("#.##") + " สม";
			else if (size < 1073741824)
				return (size / 1048576f).ToString("#.##") + " ฬม";
			else
				return (size / 1073741824f).ToString("#.##") + " รม";
		}

		public static string GetRowStyle(int rowIndex)
		{
			return rowIndex % 2 == 0 ? "EvenRow" : "OddRow";
		}
	}
}
