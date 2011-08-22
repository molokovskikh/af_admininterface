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
			Open("Billing/Search");

			browser.Button(Find.ByValue("�����")).Click();
			Assert.That(browser.Text, Is.StringContaining("����������� �����"));
			Assert.That(browser.Text, Is.StringContaining("���������� �����"));
			Assert.That(browser.Text, Is.StringContaining("����������� �������"));
			Assert.That(browser.Text, Is.StringContaining("���������� �������"));
		}

		[Test]
		public void Payers_should_be_searchable_throw_payer_id()
		{
			Open("/");

			browser.Link(Find.ByText("�������")).Click();
			Assert.That(browser.Text, Is.StringContaining("������ ������������"));
			browser.RadioButton(Find.ById("SearchByBillingId")).Click();
			browser.TextField(Find.ById("SearchText")).TypeText(payer.Id.ToString());
			browser.Button(Find.ByValue("�����")).Click();

			Assert.That(browser.Text, Is.StringContaining(payer.Name));
			browser.Link(Find.ByText(payer.Id.ToString())).Click();
			Assert.That(browser.Text, Is.StringContaining("���������� " + payer.Name));
		}
	}
}