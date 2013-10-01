using System;
using System.Linq;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Common.Web.Ui.Models.Audit;
using NHibernate;
using NHibernate.Event;

namespace AdminInterface.Models.Listeners
{
	public class ResetReclameDateAttribute : TriggerQueryAttribute
	{
		public ResetReclameDateAttribute()
			: base(typeof(ResetReclameDate))
		{
		}
	}

	public class SetForceReplicationAttribute : TriggerQueryAttribute
	{
		public SetForceReplicationAttribute()
			: base(typeof(SetForceReplication))
		{
		}
	}

	public class TriggerQueryAttribute : Attribute
	{
		public Type QueryType;

		protected TriggerQueryAttribute()
		{
		}

		protected TriggerQueryAttribute(Type type)
		{
			QueryType = type;
		}

		public virtual void Trigger(ISession session, object entity, object oldValue)
		{
			var query = (ITriggerQuery)Activator.CreateInstance(QueryType, entity);
			query.Execute(session);
		}
	}

	public interface ITriggerQuery
	{
		void Execute(ISession session);
	}

	[EventListener]
	public class QueryTriggerListner : AbstractPostUpdateEventListener, IPostUpdateEventListener
	{
		public void OnPostUpdate(PostUpdateEvent @event)
		{
			//Если значение OldState не установлено, то не производим обработку
			if (@event.OldState == null)
				return;

			foreach (var dirty in GetDirty(@event)) {
				var attr = dirty.Item1.GetCustomAttributes(typeof(TriggerQueryAttribute), true)
					.OfType<TriggerQueryAttribute>()
					.FirstOrDefault();
				if (attr == null)
					continue;

				//что бы избежать рекурсивного flush
				BaseAuditListener.LoadData(@event.Session, () => {
					attr.Trigger(@event.Session, @event.Entity, dirty.Item3);
				});
			}
		}
	}
}