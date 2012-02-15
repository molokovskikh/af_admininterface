using System.Collections.Generic;
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

		public void Save(IEnumerable<object> entities)
		{
			foreach(var entity in entities)
				Save(entity);
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
			if (_session != null)
			{
				var holder = ActiveRecordMediator.GetSessionFactoryHolder();
				holder.ReleaseSession(session);
				_session = null;
			}
			if (scope != null)
			{
				scope.Dispose();
				scope = null;
			}
		}
	}
}