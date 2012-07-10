using System;
using AdminInterface.Models.Security;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class PayerSecurityFixture : WatinFixture2
	{
		[Test]
		public void Do_not_edit_payment_without_permission()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var payer = user.Payer;

			var admin = Administrator.GetByName(Environment.UserName);
			admin.RemovePermission();
			Save(admin);

			Open(payer);
			AssertText("Плательщик");
			var input = (TextField)Css(string.Format("#UserRow{0} input[name='payment']", user.Id));
			Assert.That(input.Enabled, Is.False);
		}
	}
}