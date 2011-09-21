using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class AccountControllerFixture : ControllerFixture
	{
		private AccountsController controller;
		private User user;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			user = DataMother.CreateSupplierUser();
			payer = user.Payer;
			controller = new AccountsController();
			PrepareController(controller, "Account", "SetUserStatus");
		}

		[Test]
		public void Disable_supplier_user()
		{
			Assert.That(user.Enabled, Is.True);
			controller.SetUserStatus(user.Id, false);
			user.Refresh();
			Assert.That(user.Enabled, Is.False);
		}

		[Test]
		public void Update_report_account()
		{
			var report = new Report {
				Allow = true,
				Comment = "тестовый отчет",
				Payer = payer,
			};
			var account = new ReportAccount(report);
			account.Save();
			scope.Flush();

			controller.Update(account.Id, true, null, true, 500);
			scope.Flush();

			account.Refresh();
			Assert.That(account.Payment, Is.EqualTo(500));
			Assert.That(account.BeAccounted, Is.True);
		}
	}
}