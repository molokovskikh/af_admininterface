using System;
using System.Text;

namespace AdminInterface.Helpers
{
	public class ViewHelper
	{
		public static string ConvertToUserFriendlySize(ulong size)
		{
			if (size == 0)
				return "-";
			if (size < 1024)
				return size + " �";
			if (size < 1048576)
				return (size / 1024f).ToString("#.##") + " ��";
			if (size < 1073741824)
				return (size / 1048576f).ToString("#.##") + " ��";

			return (size / 1073741824f).ToString("#.##") + " ��";
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
				return "��";

			return "���";
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
			var url = "${siteroot}/Images/arrow-down-blue-reversed.gif";
			if (direction == "descending")
				url = "${siteroot}/Images/arrow-down-blue.gif";
			return String.Format("<img src=\"{0}\" style=\"border: none;\" />", url);
		}

		/// <summary>
		/// �������� � Url �������� ���������� �� �����, ���� ��������� �� �������, �� ���������
		/// </summary>
		/// <param name="newParams"> ��������� � �������� ������ ���� ������ ���1, ��������1, ���2, ��������2</param>
		public static string GetUrlWithReplacedParams(string url, params string[] newParams)
		{
			url = url.TrimEnd('?');
			string[] urlParts = url.Split('?');

			var newUrl = new StringBuilder(urlParts[0]);
			newUrl.Append('?');
			if (urlParts.Length > 1)
			{
				string[] oldParams = urlParts[1].Split('&');
				for (int i = 0; i < oldParams.Length; i++)
				{
					string newParam = ProcessOneParam(oldParams[i], newParams);
					if (!String.IsNullOrEmpty(newParam))
					{
						newUrl.Append(newParam);
						newUrl.Append('&');
					}
				}
			}
			if (newUrl[newUrl.Length - 1] == '&')
				newUrl.Remove(newUrl.Length - 1, 1);

			for (int i = 0; (i + 1) < newParams.Length; i += 2)
				if (!url.Contains('?' + newParams[i] + '=') &&
				  !url.Contains('&' + newParams[i] + '='))
				{
					if (newUrl[newUrl.Length - 1] != '?')
						newUrl.Append('&');
					newUrl.Append(newParams[i]);
					newUrl.Append('=');
					newUrl.Append(newParams[i + 1]);
				}

			return newUrl.ToString();
		}

		private static string ProcessOneParam(string param, string[] newParams)
		{
			string[] oldParams = param.Split('=');

			for (int i = 0; (i + 1) < newParams.Length; i += 2)
				if (oldParams[0] == newParams[i])
					if (!String.IsNullOrEmpty(newParams[i + 1]))
						return oldParams[0] + "=" + newParams[i + 1];
					else
						return String.Empty;

			return param;
		}
	}
}
