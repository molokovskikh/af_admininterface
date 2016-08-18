using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Selenium;

namespace Functional.ForTesting
{
	public class AdmSeleniumFixture : SeleniumFixture
	{
		public DataMother DataMother;
		public string DataRoot;

		[SetUp]
		public void AdmSeleniumSetup()
		{
			DataRoot = "../../../AdminInterface/Data/";
			DataMother = new DataMother(session);
		}
	}
}