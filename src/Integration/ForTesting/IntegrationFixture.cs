using Castle.ActiveRecord;
using NUnit.Framework;

namespace Integration.ForTesting
{
	[TestFixture]
	public class IntegrationFixture
	{
		protected ISessionScope scope;

		[SetUp]
		public void Setup()
		{
			scope = new TransactionlessSession();
		}

		[TearDown]
		public void TearDown()
		{
			if (scope != null)
				scope.Dispose();
		}

		protected void Reopen()
		{
			scope.Dispose();
			scope = new SessionScope();
		}

		protected void Close()
		{
			scope.Dispose();
			scope = null;
		}
	}
}