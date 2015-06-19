using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Selenium;

namespace Functional.ForTesting
{
	public class AdmSeleniumFixture : SeleniumFixture
	{
		public DataMother DataMother;

		[SetUp]
		public void AdmSeleniumSetup()
		{
			DataMother = new DataMother(session);
		}
	}
}