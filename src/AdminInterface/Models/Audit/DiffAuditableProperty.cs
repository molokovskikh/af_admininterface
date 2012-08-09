using System;
using System.Linq;
using System.Reflection;
using System.Web;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using DiffMatchPatch;

namespace AdminInterface.Models.Audit
{
	public class DiffAuditableProperty : AuditableProperty
	{
		public DiffAuditableProperty(PropertyInfo property, string name, object newValue, object oldValue)
			: base(property, name, newValue, oldValue)
		{
			IsHtml = true;
		}

		protected override void Convert(PropertyInfo property, object newValue, object oldValue)
		{
			if (oldValue == null) {
				OldValue = "";
			}
			else {
				OldValue = AsString(property, oldValue);
			}

			if (newValue == null) {
				NewValue = "";
			}
			else {
				NewValue = AsString(property, newValue);
			}

			var diff = new diff_match_patch();
			var diffs = diff.diff_main(OldValue, NewValue);
			var asHtml = diffs.Select(ToHtml).ToArray();

			Message = String.Format("$$$Изменено '{0}'<br><div>{1}</div>", Name, String.Join("", asHtml));
		}

		public string ToHtml(Diff diff)
		{
			var text = ViewHelper.FormatMessage(diff.text);
			if (diff.operation == Operation.INSERT)
				return String.Format("<ins style=\"background:#e6ffe6;\">{0}</ins>", text);
			else if (diff.operation == Operation.DELETE)
				return String.Format("<del style=\"background:#ffe6e6;\">{0}</del>", text);
			return String.Format("<span>{0}</span>", text);
		}
	}
}