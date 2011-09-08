using System.Web;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;
using NHibernate.Engine;
using NHibernate.Event;

namespace Integration.MonoRailExtentions
{
	public interface IValidatorAccessor
	{
		ValidatorRunner Validator { get; }
	}

	public class MonorailValidatorAccessor : IValidatorAccessor
	{
		public ValidatorRunner Validator
		{
			get
			{
				if (HttpContext.Current == null)
					return null;
				var controller =  HttpContext.Current.Items["currentmrcontroller"] as Controller;
				if (controller == null)
					return null;
				return controller.Validator;
			}
		}
	}

	[EventListener]
	public class ValidEventListner : IPreUpdateEventListener, IPreInsertEventListener
	{
		public static IValidatorAccessor ValidatorAccessor;

		public bool OnPreUpdate(PreUpdateEvent @event)
		{
			//валидатор может запуститься как и на самом объекте так и на его проски
			//здесь же мы получим объект а не прокси, по этому ошибки сохраненные для прокси
			//будут недоступны
			if (!IsReady)
				return false;
			var hasErrors = ValidatorAccessor.Validator.HasErrors(@event.Entity);
			if (!hasErrors)
			{
				var key = new EntityKey(@event.Id, @event.Persister, @event.Session.EntityMode);
				var proxy = @event.Session.PersistenceContext.GetProxy(key);
				if (proxy == null)
					return false;
				hasErrors = ValidatorAccessor.Validator.HasErrors(proxy);
			}
			return hasErrors;
		}

		public bool OnPreInsert(PreInsertEvent @event)
		{
			if (!IsReady)
				return false;
			return ValidatorAccessor.Validator.HasErrors(@event.Entity);
		}

		private static bool IsReady
		{
			get
			{
				if (ValidatorAccessor == null)
					return false;
				var validator = ValidatorAccessor.Validator;
				if (validator == null)
					return false;
				return true;
			}
		}
	}
}
