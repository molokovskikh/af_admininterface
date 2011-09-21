using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class ReportFixture : WatinFixture2
	{
		Client client;
		Report report;
		Payer payer;
		ReportAccount account;

		[SetUp]
		public void SetUp()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			account = DataMother.Report(payer);
			report = account.Report;
			account.Save();

			payer.Reports = new List<Report>();
			payer.Reports.Add(report);

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));
		}

		[Test]
		public void Short_report_for_payer()
		{
			Assert.That(browser.Text, Is.StringContaining("Отчеты"));
			Assert.That(browser.Text, Is.StringContaining("тестовый отчет"));
		}

		[Test]
		public void Disable_report()
		{
			var element = (CheckBox)ElementFor(account, r => r.Status);
			element.Click();

			report.Refresh();
			Assert.That(report.Allow, Is.False);
		}

		[Test]
		public void Show_unaccounted_report()
		{
			Open("/Accounts/Index");
			AssertText("тестовый отчет");
		}

		[Test]
		public void Show_accounted_report()
		{
			account.Accounted();
			account.Save();

			Open("/Accounts/Index");
			Click("История поставленных на учет");
			AssertText("тестовый отчет");
		}

		[Test]
		public void Edit_report_account_description()
		{
			Click("#reports", "Редактировать");
			AssertText("Отчет, тестовый отчет");
			Assert.That(Css("[name='account.Description']").Value, Is.EqualTo("Статистический отчет по фармрынку за {0}"));
			Css("[name='account.Description']").TypeText("Стат. отчет");
			Click("Сохранить");
			AssertText("Сохранено");

			account.Refresh();
			Assert.That(account.Description, Is.EqualTo("Стат. отчет"));
		}

		private Element ElementFor<T>(T item, Func<T, object> property)
		{
			var id = item.GetType().GetProperty("Id").GetValue(item, null);
			var idElement = (Element)browser.Css(String.Format("input[type=hidden][name=id][value='{0}']", id));
			var propertyName = "status";
			var row = (TableRow)idElement.Parents().OfType<TableRow>().First();
			return row.CheckBox(Find.ByName(propertyName));
		}
	}
}
