using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Audit
{
	public class MaskedAuditableProperty : AuditableProperty, INotificationAware
	{
		public string NotifyMessage { get; set; }

		public MaskedAuditableProperty(PropertyInfo property, string name, object newValue, object oldValue)
			: base(property, name, newValue, oldValue)
		{}

		protected override void Convert(PropertyInfo property, object newValue, object oldValue)
		{
			ulong newRegionValue = 0;
			if (newValue != null)
				newRegionValue = (ulong)newValue;
			ulong oldRegionValue = 0;
			if (oldValue != null)
				oldRegionValue = (ulong)oldValue;

			var current = ToRegionList(newRegionValue);
			var old = ToRegionList(oldRegionValue);

			var added = current.Except(old).ToArray();
			var removed = old.Except(current).ToArray();

			Message = String.Format("$$$Изменено '{0}'", Name);

			if (removed.Length > 0)
				Message += " Удалено " + ToString(removed);

			if (added.Length > 0)
				Message += " Добавлено " + ToString(added);

			var notifyAdded = ToStringForNotify(added);
			var notifyRemoved = ToStringForNotify(removed);
			if (!String.IsNullOrEmpty(notifyAdded) || !String.IsNullOrEmpty(notifyRemoved)) {
				NotifyMessage = String.Format("Изменено '{0}'", Name);

				if (!String.IsNullOrEmpty(notifyAdded))
					NotifyMessage += " Добавлено " + notifyAdded;

				if (!String.IsNullOrEmpty(notifyRemoved))
					NotifyMessage += " Удалено " + notifyRemoved;
			}
		}

		public static string ToString<T>(IEnumerable<T> items)
		{
			return items
				.Select(i => Region.TryFind(i))
				.Where(r => r != null)
				.Implode(r => "'" + r.Name + "'");
		}

		private static string ToStringForNotify<T>(IEnumerable<T> items)
		{
			return items
				.Select(i => Region.TryFind(i))
				.Where(r => r != null)
				.Where(r => !r.DoNotNotify)
				.Implode(r => "'" + r.Name + "'");
		}

		private IEnumerable<ulong> ToRegionList(ulong diff)
		{
			return Enumerable
				.Range(0, 64)
				.Select(i => (ulong)Math.Pow(2, i))
				.Where(i => (diff & i) > 0);
		}
	}
}