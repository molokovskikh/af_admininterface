using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
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
	public class RemoveCollectionListner : IPostCollectionUpdateEventListener
	{
		public void OnPostUpdateCollection(PostCollectionUpdateEvent @event)
		{
			var item = @event.AffectedOwnerOrNull;
			if (item != null) {
				var message = string.Empty;
				var needSave = false;
				if (item is User && @event.Collection.Role.Contains("AvaliableAddresses")) {
					var oldList = ((IList<object>)@event.Collection.StoredSnapshot).Cast<Address>().ToList();
					message = string.Format("$$$У пользовалеля {0} - ({1}) отключены все адреса доставки: {2}",
						((User)item).Id,
						((User)item).Name,
						UpdateCollectionListner.GetListString(oldList));
					needSave = true;
				}
				if (item is Address && @event.Collection.Role.Contains("AvaliableForUsers")) {
					var oldList = ((IList<object>)@event.Collection.StoredSnapshot).Cast<User>().ToList();
					message = string.Format("$$$Адрес {0} - ({1}) отключен у всех пользователей: {2}",
						((Address)item).Id,
						((Address)item).Name,
						UpdateCollectionListner.GetListString(oldList));
					needSave = true;
				}
				if (needSave)
					AuditListener.PreventFlush(@event.Session, () => @event.Session.Save(new ClientInfoLogEntity(message, ((dynamic)@event.AffectedOwnerOrNull).Client) {
						MessageType = LogMessageType.System,
						IsHtml = true
					}));
			}
		}
	}

	[EventListener]
	public class UpdateCollectionListner :IPostCollectionRemoveEventListener
	{
		public void OnPostRemoveCollection(PostCollectionRemoveEvent @event)
		{
			var item = @event.AffectedOwnerOrNull;
			if (item != null) {
				var itemStringTypes = @event.Collection.Role.Split(new []{'.'});
				var propertyName = itemStringTypes.Last();
				var typeName = Path.GetFileNameWithoutExtension(@event.Collection.Role);
				var assembly = Assembly.GetAssembly(typeof(UpdateCollectionListner));
				var entity = Activator.CreateInstance(Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name), typeName).Unwrap();
				var auditables = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public).GetCustomAttributes(typeof(Auditable), true);
				if (auditables.Length > 0) {
					IList<object> oldList = new List<object>();
					IEnumerable<object> newList = new List<object>();
					if (@event.Collection.StoredSnapshot != null)
						oldList = ((IList<object>)@event.Collection.StoredSnapshot).ToList();
					if (NHibernateUtil.IsInitialized(entity))
						newList = (IEnumerable<object>)item.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(item, null);
					var message = string.Format("$$$Изменен {2} {0} - ({1})", ((dynamic)item).Id, ((dynamic)item).Name, ((dynamic)auditables[0]).Name);
					BuildMessage(@event, message, newList, oldList);
				}
			}
		}

		public void BuildMessage(AbstractCollectionEvent @event, string _message, IEnumerable<object> newList, IEnumerable<object> oldList)
		{
			var added = MaskedAuditableProperty.Complement(newList, oldList).ToArray();
			var removed = MaskedAuditableProperty.Complement(oldList, newList).ToArray();

			if (removed.Length > 0)
				_message += "</br> <b> Удалено </b>" + GetListString(removed);

			if (added.Length > 0)
				_message += "</br> <b> Добавлено </b>" + GetListString(added);
			if (((dynamic)@event.AffectedOwnerOrNull).Client != null && (removed.Length > 0 || added.Length > 0))
				AuditListener.PreventFlush(@event.Session, () => 
					@event.Session.Save(new ClientInfoLogEntity(_message, ((dynamic)@event.AffectedOwnerOrNull).Client) {
					MessageType = LogMessageType.System,
					IsHtml = true
				}));
		}

		public static string GetListString(IEnumerable<object> addresses)
		{
			return addresses.Implode(a => string.Format("</br> {0} - ({1})", ((dynamic)a).Id, ((dynamic)a).Name));
		}
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
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region"))
				return new MaskedAuditableProperty(property, name, newState, oldState);
			if (entity is Payer && property.Name == "Comment")
				return new DiffAuditableProperty(property, name, newState, oldState);
			return base.GetAuditableProperty(property, name, newState, oldState, entity);
		}
	}
}