using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NHibernate;
using NUnit.Framework;

namespace Integration.Models
{
	public class AccountingFixture : AdmIntegrationFixture
	{
		private Client client;
		private Account userAccount;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			userAccount = client.Users[0].Accounting;
		}

		[Test]
		public void Find_ready_for_accounting()
		{
			session.CreateSQLQuery(@"
update billing.Accounts
set ReadyForAccounting = 0,
BeAccounted = 0;")
				.ExecuteUpdate();

			var accountings = Account.GetReadyForAccounting(new Pager(), session);
			Assert.That(accountings.Count(), Is.EqualTo(0));
			client.Users[0].Accounting.ReadyForAccounting = true;
			session.SaveOrUpdate(client);
			Flush();

			accountings = Account.GetReadyForAccounting(new Pager(), session);
			Assert.That(accountings.Count(), Is.EqualTo(1), accountings.Implode(a => a.Name));
		}

		[Test]
		public void Find_accounting_by_user()
		{
			userAccount.Accounted();
			session.SaveOrUpdate(client);
			Flush();

			var accounts = new AccountFilter { SearchBy = AccountingSearchBy.ByUser, SearchText = client.Users[0].Id.ToString() }
				.Find(session, new Pager());
			Assert.That(accounts.Count, Is.EqualTo(1));
			Assert.That(accounts.Single().Id, Is.EqualTo(client.Users[0].Accounting.Id));
		}

		[Test]
		public void Find_after_free_period_end()
		{
			userAccount.ReadyForAccounting = true;
			userAccount.FreePeriodEnd = DateTime.Today.AddDays(20);
			userAccount.IsFree = true;

			session.SaveOrUpdate(client);
			Flush();

			Assert.That(Ready(), Is.Not.Contains(userAccount.Id));

			userAccount.FreePeriodEnd = DateTime.Today.AddDays(-1);
			session.Save(userAccount);
			Flush();

			Assert.That(Ready().Any(id => id == userAccount.Id), Is.True, "не нашли аккаунт {0}", userAccount.Id);
		}

		private uint[] Ready()
		{
			var pager = new Pager();
			pager.PageSize = 1000;
			var items = Account.GetReadyForAccounting(pager, session);
			var ready = items.Select(i => i.Id).ToArray();
			return ready;
		}
	}
}