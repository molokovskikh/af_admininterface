using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Persister.Entity;

namespace AdminInterface.Models.Listeners
{
	[EventListener]
	public class UpdateReplicationInfoListener : AbstractPostUpdateEventListener, IPostUpdateEventListener
	{
		public void OnPostUpdate(PostUpdateEvent @event)
		{
			var dirty = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);
			var user = @event.Entity as IUser;
			var price = @event.Entity as IPrice;
			var settings = @event.Entity as IDrugstoreSettings;
			if (user != null) {
				if (PropertyDirty(@event.Persister, dirty, new string[]{"InheritPricesFrom"}))
					@event.Session.CreateSQLQuery(@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where UserId = :userId")
						.SetParameter("userId", user.Id)
						.ExecuteUpdate();
			}
			else if (price != null && price.Supplier != null) {
				if (PropertyDirty(@event.Persister,  dirty, new string[]{"AgencyEnabled", "Enabled"}))
					@event.Session.CreateSQLQuery(@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where FirmCode = :supplierId")
						.SetParameter("supplierId", price.Supplier.Id)
						.ExecuteUpdate();
			}
			else if (settings != null) {
				if (PropertyDirty(@event.Persister,  dirty, new string[]{"BuyingMatrixPrice", "BuyingMatrixType", "WarningOnBuyingMatrix"}))
					@event.Session.CreateSQLQuery(@"
update Usersettings.AnalitFReplicationInfo r
join Customers.Users u on u.Id = r.UserId
set ForceReplication = 1
where u.ClientId = :ClientId")
						.SetParameter("ClientId", settings.Id)
						.ExecuteUpdate();
			}
		}
	}
}