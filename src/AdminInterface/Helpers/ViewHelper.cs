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
				return size + " Á";
			if (size < 1048576)
				return (size / 1024f).ToString("#.##") + " ÊÁ";
			if (size < 1073741824)
				return (size / 1048576f).ToString("#.##") + " ÌÁ";

			return (size / 1073741824f).ToString("#.##") + " ÃÁ";
		}

		public static string GetRowStyle(int rowIndex)
		{
			return rowIndex % 2 == 0 ? "EvenRow" : "OddRow";
		}

		public static string FormatMessage(string message)
		{
			return message.Replace(Environment.NewLine, "<br>");
		}

		public static string InWords(float sum)
		{
			return TextUtil.FirstUpper(TextUtil.NumToString(sum));
		}

		public static string ToHumanReadable(bool value)
		{
			if (value)
				return "Äà";

			return "Íåò";
		}

		public static string GetDirection(string sortBy, string direction, string property)
		{
			if (sortBy == property && direction == "ascending")
				return "descending";
			return "ascending";
		}

		public static string SortArrow(string sortBy, string direction, string property)
		{
			if (sortBy != property)
				return null;
			if (String.IsNullOrEmpty(direction))
				direction = "ascending";
			direction = direction.ToLower();
			var url = "../Images/arrow-down-blue-reversed.gif";
			if (direction == "descending")
				url = "../Images/arrow-down-blue.gif";
			return String.Format("<img src=\"{0}\" style=\"border: none;\" />", url);
		}
	}
}
