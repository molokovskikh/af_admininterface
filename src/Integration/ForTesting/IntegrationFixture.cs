using Castle.ActiveRecord;
using NHibernate;
using NUnit.Framework;

namespace Integration.ForTesting
{
	[TestFixture]
	public class IntegrationFixture
	{
		private ISession _session;

		protected ISessionScope scope;

		protected ISession session
		{
			get
			{
				if (_session == null)
				{
					var holder = ActiveRecordMediator.GetSessionFactoryHolder();
					_session = holder.CreateSession(typeof(ActiveRecordBase));
				}
				return _session;
			}
		}

		[SetUp]
		public void Setup()
		{
			scope = new TransactionlessSession();
		}

		[TearDown]
		public void TearDown()
		{
			if (_session != null)
			{
				var holder = ActiveRecordMediator.GetSessionFactoryHolder();
				holder.ReleaseSession(session);
			}
			if (scope != null)
				scope.Dispose();
		}
	}
}