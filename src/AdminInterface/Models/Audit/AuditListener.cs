using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Event;

namespace AdminInterface.Models.Audit
{
	[EventListener]
	public class AuditListener : BaseAuditListener
	{
		protected override void Log(PostUpdateEvent @event, string message, bool isHtml)
		{
			var auditable = @event.Entity as IAuditable;
			if (auditable != null)
				base.Log(@event, message, isHtml);
			else
				@event.Session.Save(new ClientInfoLogEntity(message, @event.Entity) {
					IsHtml = isHtml
				});
		}

		protected override AuditableProperty GetAuditableProperty(PropertyInfo property, string name, object newState, object oldState, object entity)
		{
			if (property.PropertyType == typeof(ulong) && property.Name.Contains("Region"))
			{
				return new MaskedAuditableProperty(property, name, newState, oldState);
			}
			if (entity is Payer && property.Name == "Comment")
			{
				return new DiffAuditableProperty(property, name, newState, oldState);
			}
			return base.GetAuditableProperty(property, name, newState, oldState, entity);
		}
	}
}