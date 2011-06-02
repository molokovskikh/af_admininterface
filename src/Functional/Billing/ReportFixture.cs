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
			var payer = client.Payers.First();
			report = new Report {
				Allow = true,
				Comment = "עוסעמגûי מעקוע",
				Payer = payer,
			};
			report.Save();
			
			payer.Reports = new List<Report>();
			payer.Reports.Add(report);

			browser = Open(payer);
			Assert.That(browser.Text, Is.StringContaining("ֿכאעוכüשטך"));
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
			var idElement = (Element)browser.Css(String.Format("input[type=hidden][name=id][value='{0}']", id));
			var propertyName = "allow";
			var row = (TableRow)idElement.Parents().OfType<TableRow>().First();
			return row.CheckBox(Find.ByName(propertyName));
		}
	}
}