using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Event;
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

		public AuditableProperty(PropertyInfo property, string name, object newValue, object oldValue)
		{
			Property = property;
			Name = name;

			if (String.IsNullOrEmpty(name))
			{
				Name = BindingHelper.GetDescription(property);
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
		}

		private string AsString(PropertyInfo property, object value)
		{
			if (property.PropertyType.IsEnum)
			{
				return BindingHelper.GetDescription(value);
			}
			if (property.PropertyType == typeof(bool))
			{
				return (bool)value ? "���" : "����";
			}

			return value.ToString();
		}

		public override string ToString()
		{
			return String.Format("$$$�������� '{0}' ���� '{1}' ����� '{2}'", Name, OldValue, NewValue);
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

		private List<AuditableProperty> GetDirtyAuditableProperties(IEntityPersister persister, object[] state, object[] oldState, object entity, ISessionImplementor session)
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