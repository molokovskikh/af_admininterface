using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class BillingSearchFixture : WatinFixture2
	{
		private Client client;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			payer.UpdateAndFlush();

			client.AddAddress(new Address { Client = client, Value = "test address for billing", });
			client.UpdateAndFlush();
			foreach (var address in client.Addresses)
			{
				address.Enabled = false;
				address.UpdateAndFlush();
			}
		}

		[Test]
		public void Check_columns_by_billing_search()
		{
			using (var browser = Open("Billing/Search.rails"))
			{
				browser.Button(Find.ByValue("Найти")).Click();
				Assert.That(browser.Text, Is.StringContaining("Отключенных копий"));
				Assert.That(browser.Text, Is.StringContaining("Работающих копий"));
				Assert.That(browser.Text, Is.StringContaining("Отключенных адресов"));
				Assert.That(browser.Text, Is.StringContaining("Работающих адресов"));
			}
		}

		[Test]
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

		[Test, NUnit.Framework.Description("1 пользователь, 2 адреса. 1-й адрес не должне быть в списке неучтенных, 2-й должен быть")]
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

		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			using (var scope = new TransactionScope())
			{
				var payer = Payer.Find(921u);
				payer.Name = "Офис123";
				payer.UpdateAndFlush();
				scope.VoteCommit();
			}

			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				Assert.That(browser.Text, Is.StringContaining("Фильтр плательщиков"));
				browser.RadioButton(Find.ById("SearchByBillingId")).Click();
				browser.TextField(Find.ById("SearchText")).TypeText("921");
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.Text, Is.StringContaining("Офис123"));
				browser.Link(Find.ByText("921")).Click();
				Assert.That(browser.Text, Is.StringContaining("Плательщик Офис123"));
			}
		}

		[Test]
		public void View_billing_page_for_client()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Биллинг")).Click();
				browser.RadioButton(Find.ById("SearchByBillingId")).Click();
				browser.TextField(Find.ById("SearchText")).TypeText(payer.Id.ToString());
				browser.Button(Find.ByValue("Найти")).Click();

				Assert.That(browser.Text, Is.StringContaining(payer.Name));
				browser.Link(Find.ByText(payer.Id.ToString())).Click();
				Assert.That(browser.Text, Is.StringContaining("Плательщик " + payer.Name));
			}
		}
	}
}