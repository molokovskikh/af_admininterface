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
			var property = GetProperty(expression);
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
		private static EntityEntry GetEntry<TActiveRecord>(this ISession s, TActiveRecord activeRecord)
		{
			var context = s.GetSessionImplementation().PersistenceContext;

			EntityEntry entry = null;
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

			if (entry == null)
			{
				throw new TransientObjectException(
					"object references an unsaved transient instance - save the transient instance before flushing: "
					+ activeRecord.GetType().FullName);
			}

			return entry;
		}

		public static TResult OldValue<TActiveRecord, TResult>(this TActiveRecord activeRecord, Expression<Func<TActiveRecord, TResult>> expression)
		{
			var property = GetProperty(expression);
			return ArHelper.WithSession(s => {
				var entry = s.GetEntry(activeRecord);
				var index = entry.Persister.PropertyNames.IndexOf(n => n.Equals(property, StringComparison.OrdinalIgnoreCase));
				return (TResult)entry.LoadedState[index];
			});
		}

		private static string GetProperty<T, T1>(Expression<Func<T, T1>> expression)
		{
			return ((MemberExpression)expression.Body).Member.Name;
		}
	}
}