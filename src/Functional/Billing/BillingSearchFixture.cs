using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	public class BillingSearchFixture : AdmSeleniumFixture
	{
		private Client client;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			session.Save(payer);

			client.AddAddress(new Address { Client = client, Value = "test address for billing", });
			session.Save(client);
			foreach (var address in client.Addresses) {
				address.Enabled = false;
				session.Save(address);
			}
		}

		[Test]
		public void WithoutPayersSearchTest()
		{
			Open("Billing/Search");

			Css("#filter_ClientType").SelectByValue("1");
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
			Css("input[name='filter.SearchBy'][value='3']").Click();
			Css("#filter_SearchText").SendKeys(payer.Id.ToString());
			ClickButton("Найти");

			AssertText(payer.Name);
			ClickLink(payer.Id.ToString());
			AssertText("Плательщик " + payer.Name);
		}
	}
}