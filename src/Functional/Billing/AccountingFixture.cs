using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	public class AccountingFixture : WatinFixture2
	{
		private Client client;

		public void BaseSearchBy(IE browser, string radioButtonId, string searchText)
		{
			browser.TextField(Find.ByName("SearchBy.BeginDate")).TypeText("01.01.2009");
			browser.RadioButton(radioButtonId).Checked = true;
			browser.TextField(Find.ById("SearchText")).TypeText(searchText);
			browser.Button(Find.ByValue("Найти")).Click();

			Assert.That(browser.Text, Is.Not.Contains("За указанный период ничего не найдено"));
			Assert.That(browser.Table(Find.ById("MainTable")).TableRows.Count(), Is.GreaterThan(1));
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByUser()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByUser", "Аптека");
			}
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByAddress()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByAddress", "офис");
			}
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByClient()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByClient", "аптека");
			}
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByPayer()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByPayer", "офис");
			}
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByAuto()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "Autosearch", "офис");
			}
		}

		[Test,
			NUnit.Framework.Description("1 пользователь, 2 адреса. 1-й адрес не должне быть в списке неучтенных, 2-й должен быть"),
			Ignore("Временно до починки")]
		public void Check_address_for_accounting()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var user = new User(client) {Name = "test user",};
				user.Setup();
				var address = new Address {Value = "address",};
				client.AddAddress(address);
				address.Save();
				client = Client.Find(client.Id);

				client.Users[0].Enabled = true;
				client.Users[0].Save();

				client.Addresses[0].Enabled = true;
				client.Addresses[0].Accounting.BeAccounted = false;
				client.Addresses[0].Value = String.Format("Test address for accounting [{0}]", client.Addresses[0].Id);
				client.Addresses[0].Save();

				client.Addresses[1].Enabled = true;
				client.Addresses[1].Accounting.BeAccounted = true;
				client.Addresses[1].Value = String.Format("Test address for accounting [{0}]", client.Addresses[1].Id);
				client.Addresses[1].Save();
				foreach (var addr in client.Addresses)
				{
					addr.AvaliableForUsers = new List<User> { client.Users[0] };
					addr.Save();
				}
				scope.VoteCommit();
			}
			using (var browser = Open("Billing/Accounting"))
			{
				Assert.That(browser.Text, Is.Not.StringContaining(client.Addresses[0].Value));
				Assert.That(browser.Text, Is.StringContaining(client.Addresses[1].Value));
			}
		}


		[Test, Ignore("Временно до починки")]
		public void Check_user_for_accounting()
		{
			client.Users[0].Name = String.Format("Test username for Accounting [{0}]", client.Users[0].Id);
			client.Users[0].UpdateAndFlush();

			using (var browser = Open("Billing/Accounting"))
			{
				Assert.That(browser.Text, Is.StringContaining("Учет адресов и пользователей"));
				Assert.That(browser.Text, Is.StringContaining(client.Users[0].Name));
			}
		}

		[Test]
		public void Show_accounting_history_for_supplier_user()
		{
			var user = DataMother.CreateSupplierUser();
			user.Name = user.Login;
			user.Accounting.Accounted();
			Open("Accounts/Index?tab=AccountingHistory");
			browser.WaitUntilContainsText("Поиск", 2);
			Assert.That(browser.Text, Is.StringContaining("Поиск"));
			Assert.That(browser.Text, Is.StringContaining(user.Login));
		}
	}
}
