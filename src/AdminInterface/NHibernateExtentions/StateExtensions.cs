using System;
using System.Linq.Expressions;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Proxy;

namespace AdminInterface.NHibernateExtentions
{
	public static class StateExtensions
	{
		public static bool IsChanged<TActiveRecord, TResult>(this TActiveRecord activeRecord, Expression<Func<TActiveRecord, TResult>> expression)
		{
			var property = expression.GetProperty();
			return ArHelper.WithSession(s => {
				var entry = s.GetEntry(activeRecord);
				//если объект только создан и не был еще сохранен
				if (entry == null)
					return true;
				var index = entry.Persister.PropertyNames.IndexOf(n => n.Equals(property, StringComparison.OrdinalIgnoreCase));
				var currentState = entry.Persister.GetPropertyValue(activeRecord, index, s.ActiveEntityMode);
				if (entry.LoadedState != null)
					return !Equals(currentState, entry.LoadedState[index]);
				else
					return true;
			});
		}

		//activeRecord может быть как proxy так и уже загруженным объектом
		//по этому нужно получать EntityEntry по id объекта
		private static EntityEntry GetEntry(this ISession s, object activeRecord)
		{
			var context = s.GetSessionImplementation().PersistenceContext;

			EntityEntry entry;
			var proxy = activeRecord as INHibernateProxy;
			if (proxy != null)
			{
				if (!context.ContainsProxy(proxy))
					throw new TransientObjectException("proxy was not associated with the session");

				var li = proxy.HibernateLazyInitializer;
				entry = context.GetEntry(li.GetImplementation());
			}
			else
			{
				entry = context.GetEntry(activeRecord);
			}

			return entry;
		}

		public static TResult OldValue<TActiveRecord, TResult>(this TActiveRecord activeRecord, Expression<Func<TActiveRecord, TResult>> expression)
		{
			var property = expression.GetProperty();
			return ArHelper.WithSession(s => {
				var entry = s.GetEntry(activeRecord);
				if (entry == null)
					throw new Exception(String.Format(
						"Не могу получить значение свойства загруженного из базы т.к. сесия в которой был загружен объект {0} уже закрыта",
						activeRecord));

				var index = entry.Persister.PropertyNames.IndexOf(n => n.Equals(property, StringComparison.OrdinalIgnoreCase));
				if (entry.LoadedState == null)
					return default(TResult);
				return (TResult)entry.LoadedState[index];
			});
		}
	}
}