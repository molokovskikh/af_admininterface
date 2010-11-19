using System;
using System.Linq.Expressions;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Engine;

namespace AdminInterface.NHibernateExtentions
{
	public static class StateExtensions
	{
		public static bool IsChanged<TActiveRecord, TResult>(this TActiveRecord activeRecord, Expression<Func<TActiveRecord, TResult>> expression)
		{
			var property = GetProperty(expression);
			return ArHelper.WithSession(s => {
				var entry = GetEntry(s, activeRecord);
				var index = entry.Persister.PropertyNames.IndexOf(n => n.Equals(property, StringComparison.OrdinalIgnoreCase));
				var currentState = entry.Persister.GetPropertyValue(activeRecord, index, s.ActiveEntityMode);
				return !Equals(currentState, entry.LoadedState[index]);
			});
		}

		//activeRecord может быть как proxy так и уже загруженным объектом
		//по этому нужно получать EntityEntry по id объекта
		private static EntityEntry GetEntry<TActiveRecord>(ISession s, TActiveRecord activeRecord)
		{
			var name = s.GetEntityName(activeRecord);
			var persister = s.GetSessionImplementation().GetEntityPersister(name, activeRecord);
			var entityMode = s.ActiveEntityMode;
			var entityKey = new EntityKey(persister.GetIdentifier(activeRecord, entityMode), persister, entityMode);
			var context = s.GetSessionImplementation().PersistenceContext;
			var entity = context.GetEntity(entityKey);
			return context.GetEntry(entity);
		}

		public static TResult OldValue<TActiveRecord, TResult>(this TActiveRecord activeRecord, Expression<Func<TActiveRecord, TResult>> expression)
		{
			var property = GetProperty(expression);
			return ArHelper.WithSession(s => {
				var entry = GetEntry(s, activeRecord);
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