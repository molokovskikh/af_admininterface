using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using NHibernate.Event;

namespace AdminInterface.Models.Audit
{
	public interface IMultiAuditable
	{
		IEnumerable<IAuditRecord> GetAuditRecords();
	}

	[EventListener]
	public class AuditListener : BaseAuditListener
	{
		protected override void Log(PostUpdateEvent @event, string message, bool isHtml)
		{
			var auditable = @event.Entity as IAuditable;
			var multiAuditable = @event.Entity as IMultiAuditable;
			if (multiAuditable != null)
				LogMultiAuditable(@event, multiAuditable, message, isHtml);
			else if (auditable != null)
				base.Log(@event, message, isHtml);
			else
				@event.Session.Save(new AuditRecord(message, @event.Entity) {
					IsHtml = isHtml,
					MessageType = LogMessageType.System
				});
		}

		private void LogMultiAuditable(PostUpdateEvent @event, IMultiAuditable auditable, string message, bool isHtml)
		{
			var session = @event.Session;
			var records = LoadData(session, () => auditable.GetAuditRecords().ToArray());
			if (records == null)
				return;

			foreach (var record in records) {
				record.IsHtml = isHtml;
				record.Message = message;
				@event.Session.Save(record);
			}
		}

		protected override AuditableProperty GetPropertyForNotify(PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region")) {
				return new MaskedAuditableProperty(property, name, newState, oldState);
			}
			return new HtmlAuditableProperty(property, name, newState, oldState);
		}

		protected override AuditableProperty GetAuditableProperty(PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region")) {
				return new MaskedAuditableProperty(property, name, newState, oldState);
			}
			return base.GetAuditableProperty(property, name, newState, oldState, entity);
		}
	}
}