using Castle.ActiveRecord;
using NUnit.Framework;

namespace Integration.ForTesting
{
	[TestFixture]
	public class IntegrationFixture
	{
		private SessionScope scope;

		[SetUp]
		public void Setup()
		{
			scope = new SessionScope(FlushAction.Never);
		}

		[TearDown]
		public void TearDown()
		{
			if (scope != null)
				scope.Dispose();
		}
	}
}