﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	public class AccountingFixture : FunctionalFixture
	{
		private Client client;

		public void BaseSearchBy(string radioButtonId, string searchText)
		{
			browser.TextField(Find.ByName("SearchBy.BeginDate")).TypeText("01.01.2009");
			browser.RadioButton(radioButtonId).Checked = true;
			browser.TextField(Find.ById("SearchText")).TypeText(searchText);
			ClickButton("Найти");

			AssertNoText("За указанный период ничего не найдено");
			Assert.That(browser.Table(Find.ById("MainTable")).TableRows.Count(), Is.GreaterThan(1));
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByUser()
		{
			Open("Billing/Accounting?tab=AccountingHistory");
			BaseSearchBy("SearchByUser", "Аптека");
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByAddress()
		{
			Open("Billing/Accounting?tab=AccountingHistory");
			BaseSearchBy("SearchByAddress", "офис");
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByClient()
		{
			Open("Billing/Accounting?tab=AccountingHistory");
			BaseSearchBy("SearchByClient", "аптека");
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByPayer()
		{
			Open("Billing/Accounting?tab=AccountingHistory");
			BaseSearchBy("SearchByPayer", "офис");
		}

		[Test, Ignore("Временно до починки")]
		public void SearchInHistoryByAuto()
		{
			Open("Billing/Accounting?tab=AccountingHistory");
			BaseSearchBy("Autosearch", "офис");
		}

		[Test,
		 NUnit.Framework.Description("1 пользователь, 2 адреса. 1-й адрес не должне быть в списке неучтенных, 2-й должен быть"),
		 Ignore("Временно до починки")]
		public void Check_address_for_accounting()
		{
			client.AddUser(session, "test user");

			var address = new Address { Value = "address", };
			client.AddAddress(address);
			session.Save(address);
			client = session.Load<Client>(client.Id);

			client.Users[0].Enabled = true;
			session.SaveOrUpdate(client.Users[0]);

			client.Addresses[0].Enabled = true;
			client.Addresses[0].Accounting.BeAccounted = false;
			client.Addresses[0].Value = String.Format("Test address for accounting [{0}]", client.Addresses[0].Id);
			session.Save(client.Addresses[0]);

			client.Addresses[1].Enabled = true;
			client.Addresses[1].Accounting.BeAccounted = true;
			client.Addresses[1].Value = String.Format("Test address for accounting [{0}]", client.Addresses[1].Id);
			session.Save(client.Addresses[1]);
			foreach (var addr in client.Addresses) {
				addr.AvaliableForUsers = new List<User> { client.Users[0] };
				session.Save(addr);
			}
			Open("Billing/Accounting");
			Assert.That(browser.Text, Is.Not.StringContaining(client.Addresses[0].Value));
			AssertText(client.Addresses[1].Value);
		}


		[Test, Ignore("Временно до починки")]
		public void Check_user_for_accounting()
		{
			client.Users[0].Name = String.Format("Test username for Accounting [{0}]", client.Users[0].Id);
			session.SaveOrUpdate(client.Users[0]);

			Open("Billing/Accounting");
			AssertText("Учет адресов и пользователей");
			AssertText(client.Users[0].Name);
		}

		[Test]
		public void Show_accounting_history_for_supplier_user()
		{
			var user = DataMother.CreateSupplierUser();
			user.Accounting.Accounted();
			Open("Accounts/Index?tab=AccountingHistory");
			browser.WaitUntilContainsText("Поиск", 2);
			AssertText("Поиск");
			AssertText(user.Login);
		}
	}
}