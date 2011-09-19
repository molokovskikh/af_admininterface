using System.Data;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using NHibernate;

namespace AdminInterface.NHibernateExtentions
{
	public class ConnectionScope : AbstractScope
	{
		private IDbConnection connection;

		public ConnectionScope(IDbConnection connection)
			: this(connection, FlushAction.Auto)
		{}

		public ConnectionScope(IDbConnection connection, FlushAction flushAction)
			: base(flushAction, SessionScopeType.Custom)
		{
			this.connection = connection;
		}

		public override ISession OpenSession(ISessionFactory sessionFactory, IInterceptor interceptor)
		{
			return sessionFactory.OpenSession(connection, interceptor);
		}

		public override void FailSession(ISession session)
		{
			session.Clear();
		}
	}
}