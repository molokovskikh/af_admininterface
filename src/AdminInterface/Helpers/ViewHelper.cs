using System;

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
			if (size < 1048576)
				return (size / 1024f).ToString("#.##") + " สม";
			if (size < 1073741824)
				return (size / 1048576f).ToString("#.##") + " ฬม";

			return (size / 1073741824f).ToString("#.##") + " รม";
		}

		public static string GetRowStyle(int rowIndex)
		{
			return rowIndex % 2 == 0 ? "EvenRow" : "OddRow";
		}

		public static string FormatMessage(string message)
		{
			return message.Replace(Environment.NewLine, "<br>");
		}
	}
}
