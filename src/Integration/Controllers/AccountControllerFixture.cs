using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using IgorO.ExposedObjectProject;
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
			scope.Flush();
			payer = user.Payer;
			controller = new AccountsController();
			PrepareController(controller, "Account", "SetUserStatus");
		}

		[Test]
		public void Disable_supplier_user()
		{
			Assert.That(user.Enabled, Is.True);
			controller.SetUserStatus(user.Id, false);
			scope.Flush();
			ActiveRecordMediator.Refresh(user);
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

			controller.Update(account.Id, true, null, true, 500, null);
			scope.Flush();

			account.Refresh();
			Assert.That(account.Payment, Is.EqualTo(500));
			Assert.That(account.BeAccounted, Is.True);
		}

		[Test]
		public void Return_updated_addresses()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			var address = client.Addresses[0];
			user.AvaliableAddresses.Add(address);

			var userAccount = user.Accounting;
			userAccount.IsFree = true;
			var addressAccount = address.Accounting;
			addressAccount.IsFree = true;

			client.Save();

			//анонимные объекты internal для того что бы получить доступ к полям использую exposed object
			var result = ExposedObject.From(controller.Update(userAccount.Id, null, false, null, null, null));

			addressAccount.Refresh();
			Assert.That(addressAccount.IsFree, Is.False);
			Assert.That(result.message, Is.EqualTo(String.Format("Следующие адреса доставки стали платными: {0}", address.Value)));
			Assert.That(result.accounts.Length, Is.EqualTo(1));
			var resultAccount = ExposedObject.From(result.accounts[0]);
			Assert.That(resultAccount.id, Is.EqualTo(addressAccount.Id));
			Assert.That(resultAccount.free, Is.EqualTo(addressAccount.IsFree));
		}
	}
}