using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Event;

namespace AdminInterface.Models
{
	[EventListener]
	public class AuditListner : BaseAuditListner
	{
		protected override void Log(PostUpdateEvent @event, string message)
		{
			var auditable = @event.Entity as IAuditable;
			if (auditable != null)
			{
				var record = auditable.GetAuditRecord();
				record.Message = message.Remove(0, 3);
				@event.Session.Save(record);
			}
			else
				@event.Session.Save(new ClientInfoLogEntity(message, @event.Entity));
		}

		protected override AuditableProperty GetAuditableProperty(PropertyInfo property, string name, object newState, object oldState)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region"))
			{
				return new MaskedAuditableProperty(property, name, newState, oldState);
			}
			return base.GetAuditableProperty(property, name, newState, oldState);
		}
	}

	public class MaskedAuditableProperty : AuditableProperty
	{
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

			var added = Complement(current, old).ToArray();
			var removed = Complement(old, current).ToArray();

			Message = String.Format("$$$Изменено '{0}'", Name);

			if (removed.Length > 0)
				Message += " Удалено " + ToString(removed);

			if (added.Length > 0)
				Message += " Добавлено " + ToString(added);
		}

		public string ToString(IEnumerable<ulong> items)
		{
			return items
				.Select(i => "'" + Region.Find(i).Name + "'")
				.Implode();
		}

		public IEnumerable<T> Complement<T>(IEnumerable<T> first, IEnumerable<T> second)
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