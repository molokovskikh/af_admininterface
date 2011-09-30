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
			Close();
		}

		public void Flush()
		{
			scope.Flush();
		}

		public void Save(object entity)
		{
			ActiveRecordMediator.Save(entity);
		}

		public void Delete(object entity)
		{
			ActiveRecordMediator.Delete(entity);
		}

		protected void Reopen()
		{
			Close();
			scope = new SessionScope();
		}

		protected void Close()
		{
			if (scope != null)
			{
				scope.Dispose();
				scope = null;
			}
		}
	}
}