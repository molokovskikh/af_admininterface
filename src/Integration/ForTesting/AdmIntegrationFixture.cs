using Common.Tools;
using NUnit.Framework;

namespace Integration.ForTesting
{
	[TestFixture]
	public class AdmIntegrationFixture : Test.Support.IntegrationFixture
	{
		public DataMother DataMother;

		[SetUp]
		public void AdmIntegrationSetup()
		{
			DataMother = new DataMother(session);
		}

		[TearDown]
		public void AdmIntegrationTearDown()
		{
			SystemTime.Reset();
		}
	}
}