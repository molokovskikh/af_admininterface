using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Security;
using Common.Tools;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class PayerSecurityFixture : WatinFixture2
	{
		private List<Administrator> administrators;
		private Permission permission;

		[SetUp]
		public void Setup()
		{
			administrators = session.Query<Administrator>()
				.Where(a => a.UserName == Environment.UserName)
				.ToList();
			permission = session.Query<Permission>()
				.First(p => p.Type == PermissionType.ChangePayment);
			administrators.ForEach(a => a.AllowedPermissions.Remove(permission));
		}

		[TearDown]
		public void TearDown()
		{
			administrators.Each(a => a.AllowedPermissions.Add(permission));
		}

		[Test]
		public void Do_not_edit_payment_without_permission()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var payer = user.Payer;
			session.Flush();

			Open(payer);
			AssertText("Плательщик");
			var input = (TextField)Css(string.Format("#UserRow{0} input[name='payment']", user.Id));
			Assert.That(input.Enabled, Is.False);
		}
	}
}