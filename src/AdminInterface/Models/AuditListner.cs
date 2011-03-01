using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Persister.Entity;

namespace AdminInterface.Models
{
	public class Auditable : Attribute
	{
		public Auditable()
		{}

		public Auditable(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
	}

	public class AuditableProperty
	{
		public PropertyInfo Property { get; set; }
		public string Name { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
		public string Message { get ;set; }

		public AuditableProperty(PropertyInfo property, string name, object newValue, object oldValue)
		{
			Property = property;
			Name = name;

			if (String.IsNullOrEmpty(name))
			{
				Name = BindingHelper.GetDescription(property);
			}

			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region"))
			{
				SpecialCase((ulong)newValue, (ulong)oldValue);
				return;
			}

			if (oldValue == null)
			{
				OldValue = "";
			}
			else
			{
				OldValue = AsString(property, oldValue);
			}

			if (newValue == null)
			{
				NewValue = "";
			}
			else
			{
				NewValue = AsString(property, newValue);
			}
			Message = String.Format("$$$Изменено '{0}' было '{1}' стало '{2}'", Name, OldValue, NewValue);
		}

		private void SpecialCase(ulong newValue, ulong oldValue)
		{
			var current = ToRegionList(newValue);
			var old = ToRegionList(oldValue);

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

		private string AsString(PropertyInfo property, object value)
		{
			if (property.PropertyType.IsEnum)
			{
				return BindingHelper.GetDescription(value);
			}
			if (property.PropertyType == typeof(bool))
			{
				return (bool)value ? "вкл" : "выкл";
			}
			if (!NHibernateUtil.IsInitialized(value))
			{
				return "";
/*
				var asString = value.ToString();
				new DefaultEvictEventListener().OnEvict(new EvictEvent(value, session));
				return asString;
*/
			}

			return value.ToString();
		}

		public override string ToString()
		{
			return Message;
		}
	}

	[EventListener]
	public class AuditListner : IPostUpdateEventListener
	{
		public void OnPostUpdate(PostUpdateEvent @event)
		{
			var type = @event.Persister.GetMappedClass(EntityMode.Poco);
			if (@event.OldState == null)
				return;

			if (!IsAuditable(type))
				return;

			var properties = GetDirtyAuditableProperties(@event.Persister, @event.State, @event.OldState, @event.Entity, @event.Session);
			if (properties.Count == 0)
				return;

			var message = BuildMessage(properties);
			@event.Session.Save(new ClientInfoLogEntity(message, @event.Entity));
		}

		private string BuildMessage(List<AuditableProperty> properties)
		{
			return properties
				.Select(k => k.ToString())
				.Implode("\r\n");
		}

		private List<AuditableProperty> GetDirtyAuditableProperties(IEntityPersister persister, object[] state, object[] oldState, object entity, IEventSource session)
		{
			var dirty = persister.FindDirty(state, oldState, entity, session);
			var result = new List<AuditableProperty>();
			foreach (var index in dirty)
			{
				var name = persister.ClassMetadata.PropertyNames[index];
				var property = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
				if (property == null)
					continue;

				var attributes = property.GetCustomAttributes(typeof(Auditable), false);
				if (attributes.Length == 0)
					continue;
				result.Add(new AuditableProperty(
					property,
					((Auditable)attributes[0]).Name,
					state[index],
					oldState[index]
				));
			}
			return result;
		}

		private bool IsAuditable(Type type)
		{
			return typeof(Client) == type
				|| typeof(Address) == type
				|| typeof(User) == type
				|| typeof(DrugstoreSettings) == type;
		}
	}
}