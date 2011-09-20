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
		ReportAccounting account;

		[SetUp]
		public void SetUp()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			report = new Report {
				Allow = true,
				Comment = "�������� �����",
				Payer = payer,
			};
			account = new ReportAccounting(report);
			account.Save();
			
			payer.Reports = new List<Report>();
			payer.Reports.Add(report);

			Open(payer);
			Assert.That(browser.Text, Is.StringContaining("����������"));
		}

		[Test]
		public void Short_report_for_payer()
		{
			Assert.That(browser.Text, Is.StringContaining("������"));
			Assert.That(browser.Text, Is.StringContaining("�������� �����"));
		}

		[Test]
		public void Disable_report()
		{
			var element = (CheckBox)ElementFor(account, r => r.Status);
			element.Click();
			browser.Eval(String.Format("$('input[name=allow]').change()"));

			report.Refresh();
			Assert.That(report.Allow, Is.False);
		}

		[Test]
		public void Show_unaccounted_report()
		{
			Open("/Accounts/Index");
			AssertText("�������� �����");
		}

		[Test]
		public void Show_accounted_report()
		{
			account.Accounted();
			account.Save();

			Open("/Accounts/Index");
			Click("������� ������������ �� ����");
			AssertText("�������� �����");
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