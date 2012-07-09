using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Listeners;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
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
				@event.Session.Save(new ClientInfoLogEntity(message, @event.Entity) {
					IsHtml = isHtml,
					MessageType = LogMessageType.System
				});
		}

		private void LogMultiAuditable(PostUpdateEvent @event, IMultiAuditable auditable, string message, bool isHtml)
		{
			var session = @event.Session;
			var records = PreventFlush(session, () => auditable.GetAuditRecords().ToArray());
			if (records == null)
				return;

			foreach (var record in records) {
				record.IsHtml = isHtml;
				record.Message = message;
				@event.Session.Save(record);
			}
		}

		public static T PreventFlush<T>(IEventSource session, Func<T> func)
		{
			var oldFlushing = session.PersistenceContext.Flushing;
			try
			{
				session.PersistenceContext.Flushing = true;
				return func();
			}
			finally
			{
				session.PersistenceContext.Flushing = oldFlushing;
			}
		}

		protected override AuditableProperty GetAuditableProperty(PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region")) {
				var auditableProperty = new MaskedAuditableProperty(property, name, newState, oldState);
				if (auditableProperty.Added.Length > 0) {
					var user = entity as User;
					if (user != null && property.Name.Contains("WorkRegionMask")) {
						auditableProperty.DoActionPostSend = session => 
							new SetForceReplication(session).ForUser(user.Id);
					}
					var client = entity as Client;
					if (client != null && property.Name.Contains("MaskRegion")) {
						auditableProperty.DoActionPostSend = session => 
							new SetForceReplication(session).ForClient(client.Id);
					}
					var supplier = entity as Supplier;
					if (supplier != null) {
						auditableProperty.DoActionPostSend = session => 
							new SetForceReplication(session).ForSupplier(supplier.Id);
					}
				}
				return auditableProperty;
			}
			if (entity is Payer && property.Name == "Comment")
				return new DiffAuditableProperty(property, name, newState, oldState);
			return base.GetAuditableProperty(property, name, newState, oldState, entity);
		}
	}
}