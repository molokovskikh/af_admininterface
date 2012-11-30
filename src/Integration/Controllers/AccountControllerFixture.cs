using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using ExposedObject;
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
			Flush();
			payer = user.Payer;
			controller = new AccountsController();
			Prepare(controller);
		}

		[Test]
		public void Disable_supplier_user()
		{
			Assert.That(user.Enabled, Is.True);
			controller.SetUserStatus(user.Id, false, null);
			Flush();
			session.Refresh(user);
			Assert.That(user.Enabled, Is.False);
		}

		[Test]
		public void Update_report_account()
		{
			var report = new Report {
				Allow = true,
				Comment = "тестовый отчет",
				Payer = payer
			};
			var account = new ReportAccount(report);
			session.Save(account);
			Flush();

			controller.Update(account.Id, true, null, true, 500, null, null);
			Flush();

			session.Refresh(account);
			Assert.That(account.Payment, Is.EqualTo(500));
			Assert.That(account.BeAccounted, Is.True);
		}

		[Test]
		public void Get_Ready_For_Accounting_if_disabled()
		{
			var account = new UserAccount(user) { ReadyForAccounting = true, BeAccounted = false };
			session.Save(account);
			Flush();

			var acoounts = Account.GetReadyForAccounting(new Pager { PageSize = 1000 }, session).Select(a => a.ObjectId).ToList();
			Assert.IsTrue(acoounts.Contains(account.ObjectId));
			controller.Update(account.Id, false, null, false, 500, null, null);
			Flush();

			acoounts = Account.GetReadyForAccounting(new Pager { PageSize = 1000 }, session).Select(a => a.ObjectId).ToList();
			Assert.IsFalse(acoounts.Contains(account.ObjectId));
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

			session.Save(client);

			//анонимные объекты internal для того что бы получить доступ к полям использую exposed object
			var result = Exposed.From(controller.Update(userAccount.Id, null, false, null, null, null, null));

			session.Refresh(addressAccount);
			Assert.That(addressAccount.IsFree, Is.False);
			Assert.That(result.message, Is.EqualTo(String.Format("Следующие адреса доставки стали платными: {0}", address.Value)));
			Assert.That(result.accounts.Length, Is.EqualTo(1));
			var resultAccount = Exposed.From(result.accounts[0]);
			Assert.That(resultAccount.id, Is.EqualTo(addressAccount.Id));
			Assert.That(resultAccount.free, Is.EqualTo(addressAccount.IsFree));
		}
	}
}