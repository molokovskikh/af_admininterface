using System;
using System.Linq;
using System.Reflection;
using System.Web;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using DiffMatchPatch;
using NHibernate;

namespace AdminInterface.Models.Audit
{
	public class DiffAuditableProperty : AuditableProperty
	{
		public DiffAuditableProperty(ISession session, PropertyInfo property, string name, object newValue, object oldValue)
			: base(session, property, name, newValue, oldValue)
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

			Message = $"$$$Изменено '{Name}'<br><div>{String.Join("", asHtml)}</div>";
		}

		public string ToHtml(Diff diff)
		{
			var text = ViewHelper.FormatMessage(diff.text);
			if (diff.operation == Operation.INSERT)
				return $"<ins style=\"background:#e6ffe6;\">{text}</ins>";
			else if (diff.operation == Operation.DELETE)
				return $"<del style=\"background:#ffe6e6;\">{text}</del>";
			return $"<span>{text}</span>";
		}
	}
}