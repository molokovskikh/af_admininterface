using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	public class AccountingFixture : IntegrationFixture
	{
		[Test]
		public void Find_ready_for_accounting()
		{
			ArHelper.WithSession(s => s.CreateSQLQuery(@"
update billing.accounting
set ReadyForAcounting = 0,
BeAccounted = 0;
").ExecuteUpdate());
			var client = DataMother.CreateTestClientWithAddressAndUser();

			var accountings = Accounting.GetReadyForAccounting(new Pager());
			Assert.That(accountings.Count(), Is.EqualTo(0));
			client.Users[0].Accounting.ReadyForAcounting = true;
			client.SaveAndFlush();

			accountings = Accounting.GetReadyForAccounting(new Pager());
			Assert.That(accountings.Count(), Is.EqualTo(1), accountings.Implode(a => a.Name));
		}

		[Test]
		public void Find_accounting_by_user()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].Accounting.Accounted();
			client.SaveAndFlush();

			var accounts = new AccountFilter {SearchBy = AccountingSearchBy.ByUser, SearchText = client.Users[0].Id.ToString()}.Find(new Pager());
			Assert.That(accounts.Count, Is.EqualTo(1));
			Assert.That(accounts.Single().Id, Is.EqualTo(client.Users[0].Accounting.Id));
		}
	}
}