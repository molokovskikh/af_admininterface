using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class EditFixture : WatinFixture2
	{
		private Payer _payer;
		private Client _client;

		[SetUp]
		public void SetUp()
		{
			_client = DataMother.CreateClientAndUsers();
			_payer = _client.Payers[0];
			_payer.Recipient = session.Query<Recipient>().First();
			var report = new Report { Payer = _payer };
			_payer.Reports.Add(report);
			session.Save(_client);
			session.Save(_payer);
			session.Save(report);
			session.Save(new ReportAccount(report));
			Flush();
		}

		[Test]
		public void Set_null_recipient()
		{
			browser = Open(string.Format("Billing/Edit?BillingCode={0}#tab-mail", _payer.Id));
			var selectList = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			var items = selectList.Options;
			selectList.SelectByValue(items[0].Value);
			browser.TableCell("savePayer").Buttons.First().Click();
			Flush();
			_payer.Refresh();
			Assert.IsNull(_payer.Recipient);
		}

		[Test]
		public void Check_comment_with_disable_client()
		{
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", _payer.Id));
			var checkBox = browser.Css("#clients input[name=\"status\"]");
			checkBox.Checked = false;
			browser.TextField(Find.ByName("AddComment")).AppendText("TestComment");
			var buttons = browser.Buttons.Where(b => !string.IsNullOrEmpty(b.ClassName) && b.ClassName.Contains("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).ToList();
			buttons.First(b => b.InnerHtml.Contains("Продолжить")).Click();
			Thread.Sleep(500);
			browser.Refresh();
			AssertText("TestComment");
		}

		[Test]
		public void Check_free_accounting()
		{
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", _payer.Id));
			Css("input[name=accounted]").Checked = true;
			Css("input[name=free]").Checked = true;
			Css("input[name=FreePeriodEnd]").AppendText(DateTime.Now.AddMonths(1).ToShortDateString());
			Css("input[name=AddComment]").AppendText("Check_free_accounting");
			var buttons = browser.Buttons.Where(b => !string.IsNullOrEmpty(b.ClassName) && b.ClassName.Contains("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).ToList();
			buttons.First(b => b.InnerHtml.Contains("Продолжить")).Click();
			var accounted = Css("input[name=accounted]");
			Assert.IsFalse(accounted.Checked);
			Assert.IsFalse(accounted.Enabled);
			Assert.That(browser.Text, !Is.StringContaining("Это поле необходимо заполнить."));
			AssertText(DateTime.Now.AddMonths(1).ToShortDateString());
			browser.Refresh();
			AssertText("Check_free_accounting");
			AssertText(DateTime.Now.AddMonths(1).ToShortDateString());
		}

		[Test]
		public void Check_report_status_test()
		{
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", _payer.Id));
			Css("#reports input[name=status]").Checked = true;
			Css("#reports input[name=status]").Checked = false;
			Css("input[name=AddComment]").AppendText("Check_report_status_test");
			var buttons = browser.Buttons.Where(b => !string.IsNullOrEmpty(b.ClassName) && b.ClassName.Contains("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).ToList();
			buttons.First(b => b.InnerHtml.Contains("Продолжить")).Click();
			browser.Refresh();
			AssertText("Check_report_status_test");
		}

		[Test]
		public void Check_audit_record_messages_for_client()
		{
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", _payer.Id));

			var record = new AuditRecord("test_message_for_client", _client);
			session.Save(record);

			Flush();

			Assert.IsFalse(browser.CheckBox("filter_Types").Checked);
			browser.CheckBox("filter_Types").Checked = true;

			AssertText("test_message_for_client");
		}
	}
}