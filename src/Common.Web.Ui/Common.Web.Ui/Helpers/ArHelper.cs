
using System.Collections;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using NHibernate;

namespace Common.Web.Ui.Helpers
{
	public delegate IList<T> InSessionReturnListDelegate<T>(ISession session);
	public delegate T InSessionReturnInstanceDelegate<T>(ISession session);
	public delegate void InSession(ISession session);

	public class ArHelper
	{
		public static void Evict(ISession session, IEnumerable items)
		{
			foreach (var item in items)
				session.Evict(item);
		}

		public static void WithSession<T>(InSession sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(T));
			try
			{
				sessionDelegate(session);
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		public static IList<T> WithSession<T>(InSessionReturnListDelegate<T> sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(T));
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

		public static T WithSession<T>(InSessionReturnInstanceDelegate<T> sessionDelegate)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(T));
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