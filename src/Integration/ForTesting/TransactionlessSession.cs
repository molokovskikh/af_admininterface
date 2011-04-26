using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using NHibernate;

namespace Integration.ForTesting
{
	public class TransactionlessSession : AbstractScope
	{
		public TransactionlessSession()
			: base(FlushAction.Auto, SessionScopeType.Simple)
		{}

		public override void FailSession(ISession session)
		{}

		public override ISession GetSession(object key)
		{
			return key2Session[key];
		}

		public override ISession OpenSession(ISessionFactory sessionFactory, IInterceptor interceptor)
		{
			var session = sessionFactory.OpenSession(interceptor);
			return session;
		}
	}
}