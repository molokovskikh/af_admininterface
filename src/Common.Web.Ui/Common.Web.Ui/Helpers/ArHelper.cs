using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Castle.ActiveRecord;
using NHibernate;

namespace Common.Web.Ui.Helpers
{
	public class ArHelper
	{
		public static void Evict(ISession session, IEnumerable items)
		{
			foreach (var item in items)
				session.Evict(item);
		}

		public static void WithSession(Action<ISession> sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(ActiveRecordBase));
			try
			{
				sessionDelegate(session);
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		public static IList<T> WithSession<T>(Func<ISession, IList<T>> sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(ActiveRecordBase));
			try
			{
				var result = sessionDelegate(session);
				Evict(session, result);
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		public static T WithSession<T>(Func<ISession, T> sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			//хак что бы обойти ошибку в ActiveRecord
			//в OnSave и OnUpdate нельзя обратиться к текущей сесии тк она считается уже удаленной
			if (HttpContext.Current != null)
			{
				var scope = (SessionScope)HttpContext.Current.Items["SessionScopeWebModule.session"];
				if (scope != null)
				{
					var factory = sessionHolder.GetSessionFactory(typeof(ActiveRecordBase));
					var s = scope.GetSession(factory);
					return sessionDelegate(s);
				}
			}

			var session = sessionHolder.CreateSession(typeof(ActiveRecordBase));
			try
			{
				T result = sessionDelegate(session);
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}
	}
}