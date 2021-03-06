﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using NHibernate;
using NHibernate.Event;

namespace AdminInterface.Models.Audit
{
	public interface IMultiAuditable
	{
		IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties = null);
	}

	[EventListener]
	public class AuditListener : BaseAuditListener
	{
		protected override void Log(PostUpdateEvent @event, IEnumerable<AuditableProperty> properties, bool isHtml)
		{
			var auditable = @event.Entity as IAuditable;
			var multiAuditable = @event.Entity as IMultiAuditable;
			if (multiAuditable != null)
				LogMultiAuditable(@event, multiAuditable, properties, isHtml);
			else if (auditable != null)
				base.Log(@event, properties, isHtml);
			else
				@event.Session.Save(new AuditRecord(BuildMessage(properties), @event.Entity) {
					IsHtml = isHtml,
					MessageType = LogMessageType.System
				});
		}

		private void LogMultiAuditable(PostUpdateEvent @event, IMultiAuditable auditable, IEnumerable<AuditableProperty> properties, bool isHtml)
		{
			var session = @event.Session;
			var records = LoadData(session, () => auditable.GetAuditRecords(properties).ToArray());
			if (records == null)
				return;

			foreach (var record in records) {
				record.IsHtml = isHtml;
				record.Message = BuildMessage(properties);
				@event.Session.Save(record);
			}
		}

		protected override AuditableProperty GetPropertyForNotify(ISession session, PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region")) {
				return new MaskedAuditableProperty(session, property, name, newState, oldState);
			}
			return new HtmlAuditableProperty(session, property, name, newState, oldState);
		}

		protected override AuditableProperty GetAuditableProperty(ISession session, PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region")) {
				return new MaskedAuditableProperty(session, property, name, newState, oldState);
			}
			if(property.PropertyType == typeof(bool) && property.Name == "Disabled" && entity.GetType() == typeof(Suppliers.Supplier)) {
				return base.GetAuditableProperty(session, property, name, oldState, newState, entity);
			}
			return base.GetAuditableProperty(session, property, name, newState, oldState, entity);
		}
	}
}