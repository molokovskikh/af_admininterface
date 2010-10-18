using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using WatiNCssSelectorExtensions;

namespace Functional.Billing
{
	[TestFixture]
	public class ReportFixture : WatinFixture
	{
		Client client;
		IE browser;
		Report report;

		public ReportFixture()
		{
			UseTestScope = true;
		}

		[SetUp]
		public void SetUp()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			report = new Report {
				Allow = true,
				Comment = "עוסעמגûי מעקוע",
				Payer = client.Payer,
			};
			report.Save();
			client.Payer.Reports = new List<Report>();
			client.Payer.Reports.Add(report);

			browser = Open("Billing/Edit.rails?ClientCode={0}", client.Id);
		}

		[TearDown]
		public void TearDown()
		{
			browser.Dispose();
		}

		[Test]
		public void Short_report_for_payer()
		{
			Assert.That(browser.Text, Is.StringContaining("־עקועû"));
			Assert.That(browser.Text, Is.StringContaining("עוסעמגûי מעקוע"));
		}

		[Test]
		public void Disable_report()
		{
			var element = (CheckBox)ElementFor(report, r => r.Allow);
			element.Click();
			browser.Eval(String.Format("$('input[name=allow]').change()"));

			report.Refresh();
			Assert.That(report.Allow, Is.False);
		}

		private Element ElementFor<T>(T item, Func<Report, object> property)
		{
			var id = item.GetType().GetProperty("Id").GetValue(item, null);
			var idElement = browser.CssSelect(String.Format("input[type=hidden][name=id][value='{0}']", id));
			var propertyName = "allow";
			var row = (TableRow)idElement.Parents().OfType<TableRow>().First();
			return row.CheckBox(Find.ByName(propertyName));
		}
	}
}