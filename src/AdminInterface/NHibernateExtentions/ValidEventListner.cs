using System.Web;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;
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
			if (ValidatorAccessor == null)
				return false;
			return ValidatorAccessor.Validator.HasErrors(@event.Entity);
		}

		public bool OnPreInsert(PreInsertEvent @event)
		{
			if (ValidatorAccessor == null)
				return false;
			return ValidatorAccessor.Validator.HasErrors(@event.Entity);
		}
	}
}