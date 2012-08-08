using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core; using Test.Support.Web;

namespace Functional.Billing
{
	public class BillingSearchFixture : WatinFixture2
	{
		private Client client;
		private Payer payer;

		[SetUp]
		public new void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			payer.UpdateAndFlush();

			client.AddAddress(new Address { Client = client, Value = "test address for billing", });
			session.SaveOrUpdate(client);
			foreach (var address in client.Addresses)
			{
				address.Enabled = false;
				address.UpdateAndFlush();
			}
		}

		[Test]
		public void Check_columns_by_billing_search()
		{
			Open("Billing/Search");

			browser.Button(Find.ByValue("Найти")).Click();
			Assert.That(browser.Text, Is.StringContaining("Отключенных копий"));
			Assert.That(browser.Text, Is.StringContaining("Работающих копий"));
			Assert.That(browser.Text, Is.StringContaining("Отключенных адресов"));
			Assert.That(browser.Text, Is.StringContaining("Работающих адресов"));
		}

		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			Open();

			browser.Link(Find.ByText("Биллинг")).Click();
			Assert.That(browser.Text, Is.StringContaining("Фильтр плательщиков"));
			var button = (RadioButton)Css("input[name='filter.SearchBy'][value='3']");
			button.Click();
			browser.TextField(Find.ById("filter_SearchText")).TypeText(payer.Id.ToString());
			browser.Button(Find.ByValue("Найти")).Click();

			Assert.That(browser.Text, Is.StringContaining(payer.Name));
			browser.Link(Find.ByText(payer.Id.ToString())).Click();
			Assert.That(browser.Text, Is.StringContaining("Плательщик " + payer.Name));
		}
	}
}
