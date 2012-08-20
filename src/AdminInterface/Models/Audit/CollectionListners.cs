﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using NHibernate;
using NHibernate.Event;

namespace AdminInterface.Models.Audit
{
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
					AuditListener.LoadData(@event.Session, () => @event.Session.Save(new AuditRecord(message, ((dynamic)@event.AffectedOwnerOrNull).Client) {
						MessageType = LogMessageType.System,
						IsHtml = true
					}));
			}
		}
	}

	[EventListener]
	public class UpdateCollectionListner : IPostCollectionRemoveEventListener
	{
		public void OnPostRemoveCollection(PostCollectionRemoveEvent @event)
		{
			var item = @event.AffectedOwnerOrNull;
			if (item != null) {
				var itemStringTypes = @event.Collection.Role.Split(new[] { '.' });
				var propertyName = itemStringTypes.Last();
				var auditables = item.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public).GetCustomAttributes(typeof(Auditable), true);
				if (auditables.Length > 0) {
					IEnumerable<object> oldList = new List<object>();
					IEnumerable<object> newList = new List<object>();
					if (@event.Collection.StoredSnapshot != null)
						oldList = ((IList<object>)@event.Collection.StoredSnapshot).ToList();
					if (NHibernateUtil.IsInitialized(item)) {
						var persistentBag = item.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(item, null);
						if (NHibernateUtil.IsInitialized(persistentBag))
							newList = (IEnumerable<object>)persistentBag;
					}

					var name = ((Auditable)auditables[0]).Name;
					var message = String.Format("$$$Изменено '{0}'", name);

					BuildMessage(@event, message, newList, oldList);
				}
			}
		}

		public void BuildMessage(AbstractCollectionEvent @event, string message, IEnumerable<object> newList, IEnumerable<object> oldList)
		{
			var added = newList.Except(oldList).ToArray();
			var removed = oldList.Except(newList).ToArray();

			if (removed.Length > 0)
				message += "</br> <b> Удалено </b>" + GetListString(removed);

			if (added.Length > 0)
				message += "</br> <b> Добавлено </b>" + GetListString(added);

			if (removed.Length > 0 || added.Length > 0)
				AuditListener.LoadData(@event.Session, () =>
					@event.Session.Save(new AuditRecord(message, @event.AffectedOwnerOrNull) {
						MessageType = LogMessageType.System,
						IsHtml = true
					}));
		}

		public static string GetListString(IEnumerable<object> item)
		{
			return item.Implode(a => string.Format("</br> {0}", ((dynamic)a).Name));
		}
	}
}