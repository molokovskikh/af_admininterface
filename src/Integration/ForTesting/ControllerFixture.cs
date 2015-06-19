using System;
using NUnit.Framework;

namespace Integration.ForTesting
{
	public class ControllerFixture : Common.Web.Ui.Test.Controllers.ControllerFixture
	{
		public DataMother DataMother;

		[SetUp]
		public void ControllerSetup()
		{
			DataMother = new DataMother(session);
			ForTest.InitializeMailer();
		}
	}
}