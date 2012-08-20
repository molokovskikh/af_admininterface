using System.Collections.Generic;
using System.Text;
using DiffMatchPatch;

namespace AdminInterface.Helpers
{
	public class DiffHelper
	{
		public string ToHtml(IEnumerable<Diff> diffs)
		{
			var html = new StringBuilder();
			foreach (var aDiff in diffs) {
				var text = aDiff.text.Replace("&", "&amp;").Replace("<", "&lt;")
					.Replace(">", "&gt;").Replace("\n", "&para;<br>");
				switch (aDiff.operation) {
					case Operation.INSERT:
						html.Append("<ins style=\"background:#e6ffe6;\">").Append(text)
							.Append("</ins>");
						break;
					case Operation.DELETE:
						html.Append("<del style=\"background:#ffe6e6;\">").Append(text)
							.Append("</del>");
						break;
					case Operation.EQUAL:
						html.Append("<span>").Append(text).Append("</span>");
						break;
				}
			}
			return html.ToString();
		}
	}
}