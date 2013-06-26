using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

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
			foreach (var address in client.Addresses) {
				address.Enabled = false;
				address.UpdateAndFlush();
			}
		}

		[Test]
		public void WithoutPayersSearchTest()
		{
			Open("Billing/Search");

			browser.SelectList(Find.ById("filter_ClientType")).SelectByValue("1");
			AssertText("Игнорировать плательщиков, содержащих Поставщиков");
		}

		[Test]
		public void Check_columns_by_billing_search()
		{
			Open("Billing/Search");

			ClickButton("Найти");
			AssertText("Отключенных копий");
			AssertText("Работающих копий");
			AssertText("Отключенных адресов");
			AssertText("Работающих адресов");
		}

		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			Open();

			ClickLink("Биллинг");
			AssertText("Фильтр плательщиков");
			var button = (RadioButton)Css("input[name='filter.SearchBy'][value='3']");
			button.Click();
			browser.TextField(Find.ById("filter_SearchText")).TypeText(payer.Id.ToString());
			ClickButton("Найти");

			AssertText(payer.Name);
			ClickLink(payer.Id.ToString());
			AssertText("Плательщик " + payer.Name);
		}
	}
}