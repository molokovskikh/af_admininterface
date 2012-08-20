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

		protected TriggerQueryAttribute(Type type)
		{
			QueryType = type;
		}
	}

	public interface IAppQuery
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

			var queries = GetDirty(@event)
				.Select(t => t.Item1.GetCustomAttributes(typeof(TriggerQueryAttribute), true)
					.OfType<TriggerQueryAttribute>()
					.FirstOrDefault())
				.Where(a => a != null);

			//что бы избежать рекурсивного flush
			foreach (var queryAttribute in queries) {
				BaseAuditListener.LoadData(@event.Session, () => {
					var query = (IAppQuery)Activator.CreateInstance(queryAttribute.QueryType, @event.Entity);
					query.Execute(@event.Session);
				});
			}
		}
	}
}