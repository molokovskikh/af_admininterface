using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;

namespace AdminInterface.Models.Audit
{
	public class MaskedAuditableProperty : AuditableProperty
	{
		public ulong[] Added { get; set; }

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

			Added = Complement(current, old).ToArray();
			var removed = Complement(old, current).ToArray();

			Message = String.Format("$$$Изменено '{0}'", Name);

			if (removed.Length > 0)
				Message += " Удалено " + ToString(removed);

			if (Added.Length > 0)
				Message += " Добавлено " + ToString(Added);
		}

		public static string ToString<T>(IEnumerable<T> items)
		{
			return items
				.Select(i => Region.TryFind(i))
				.Where(r => r != null)
				.Implode(r => "'" + r.Name + "'");
		}

		public static IEnumerable<T> Complement<T>(IEnumerable<T> first, IEnumerable<T> second)
		{
			foreach (var item in first)
			{
				if (second.All(i => !Equals(i, item)))
					yield return item;
			}
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