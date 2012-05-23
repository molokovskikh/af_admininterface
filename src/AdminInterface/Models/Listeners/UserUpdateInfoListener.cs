using Castle.ActiveRecord;
using NHibernate.Event;

namespace AdminInterface.Models.Listeners
{
	[EventListener]
	public class UserUpdateInfoListener : AbstractPostUpdateEventListener, IPostUpdateEventListener
	{
		public void OnPostUpdate(PostUpdateEvent @event)
		{
			var settings = @event.Entity as IDrugstoreSettings;
			if (settings != null) {
				var dirty = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);
				if (PropertyDirty(@event.Persister,  dirty, new string[]{"ShowAdvertising"}))
					@event.Session.CreateSQLQuery(@"
update Usersettings.UserUpdateInfo uui
join Customers.Users u on u.Id = uui.UserId
set Reclamedate = null
where u.ClientId = :ClientId")
						.SetParameter("ClientId", settings.Id)
						.ExecuteUpdate();
			}
		}
	}
}