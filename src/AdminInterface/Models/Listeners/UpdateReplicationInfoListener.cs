using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Persister.Entity;

namespace AdminInterface.Models.Listeners
{
	public class SetForceReplication
	{
		protected ISession Session { get; set; }

		public SetForceReplication(ISession session)
		{
			Session = session;
		}

		public void ForUser(uint id)
		{
			Session.CreateSQLQuery(
@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where UserId = :userId")
			.SetParameter("userId", id)
			.ExecuteUpdate();
		}

		public void ForClient(uint id)
		{
			Session.CreateSQLQuery(@"
update Usersettings.AnalitFReplicationInfo r
join Customers.Users u on u.Id = r.UserId
set ForceReplication = 1
where u.ClientId = :ClientId")
			.SetParameter("ClientId", id)
			.ExecuteUpdate();
		}

		public void ForSupplier(uint id)
		{
			Session.CreateSQLQuery(
@"update Usersettings.AnalitfReplicationInfo set ForceReplication = 1 where FirmCode = :supplierId")
			.SetParameter("supplierId",id)
			.ExecuteUpdate();
		}
	}

	[EventListener]
	public class UpdateReplicationInfoListener : AbstractPostUpdateEventListener, IPostUpdateEventListener
	{
		public void OnPostUpdate(PostUpdateEvent @event)
		{
			//Если значение OldState не установлено, то не производим обработку
			if (@event.OldState == null)
				return;

			var dirty = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);
			var user = @event.Entity as IUser;
			var price = @event.Entity as IPrice;
			var settings = @event.Entity as IDrugstoreSettings;
			if (user != null) {
				if (PropertyDirty(@event.Persister, dirty, new string[]{"InheritPricesFrom"}))
					new SetForceReplication(@event.Session).ForUser(user.Id);
			}
			else if (price != null && price.Supplier != null) {
				if (PropertyDirty(@event.Persister,  dirty, new string[]{"AgencyEnabled", "Enabled"}))
					new SetForceReplication(@event.Session).ForSupplier(price.Supplier.Id);
			}
			else if (settings != null) {
				if (PropertyDirty(@event.Persister,  dirty, new string[]{"BuyingMatrixPrice", "BuyingMatrixType", "WarningOnBuyingMatrix"}))
					new SetForceReplication(@event.Session).ForClient(settings.Id);
			}
		}
	}
}