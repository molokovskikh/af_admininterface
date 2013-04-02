using System;
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
	public class UpdateCollectionListner : BaseUpdateCollectionListner
	{
		public override void DoRecord(IEventSource session, object owner, string message)
		{
			BaseAuditListener.LoadData(session, () =>
				session.Save(new AuditRecord(message, owner) {
					MessageType = LogMessageType.System,
					IsHtml = true
				}));
		}
	}
}