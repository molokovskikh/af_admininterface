using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class AccountingFixture : WatinFixture
	{
		public void BaseSearchBy(IE browser, string radioButtonId, string searchText)
		{
			browser.TextField(Find.ByName("SearchBy.BeginDate")).TypeText("01.01.2009");
			browser.RadioButton(radioButtonId).Checked = true;
			browser.TextField(Find.ById("SearchText")).TypeText(searchText);
			browser.Button(Find.ByValue("Найти")).Click();

			Assert.That(browser.Text, Is.Not.Contains("За указанный период ничего не найдено"));
			Assert.That(browser.Table(Find.ById("MainTable")).TableRows.Count(), Is.GreaterThan(1));
		}

		[Test]
		public void SearchInHistoryByUser()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByUser", "Аптека");
			}
		}

		[Test]
		public void SearchInHistoryByAddress()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByAddress", "офис");
			}
		}

		[Test]
		public void SearchInHistoryByClient()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByClient", "аптека");
			}
		}

		[Test]
		public void SearchInHistoryByPayer()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "SearchByPayer", "офис");
			}
		}

		[Test]
		public void SearchInHistoryByAuto()
		{
			using (var browser = Open("Billing/Accounting?tab=AccountingHistory"))
			{
				BaseSearchBy(browser, "Autosearch", "офис");
			}
		}
	}
}
