using System;
using NUnit.Framework;

namespace Integration.ForTesting
{
	public class ControllerFixture : Common.Web.Ui.Test.Controllers.ControllerFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitializeMailer();
		}
	}
}