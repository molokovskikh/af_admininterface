using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class EditFixture : FunctionalFixture
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
		}

		[Test]
		public void Set_null_recipient()
		{
			Open($"Billing/Edit?BillingCode={_payer.Id}#tab-mail");
			var selectList = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			var items = selectList.Options;
			selectList.SelectByValue(items[0].Value);
			browser.TableCell("savePayer").Buttons.First().Click();
			Flush();
			session.Refresh(_payer);
			Assert.IsNull(_payer.Recipient);
		}

		[Test]
		public void Check_comment_with_disable_client()
		{
			Open($"Billing/Edit?BillingCode={_payer.Id}");
			Css("#clients input[name=\"status\"]").Checked = false;
			Css("input[name=AddComment]").AppendText("TestComment");
			ConfirmDialog();

			Wait(() => session.Query<PayerAuditRecord>()
				.Any(r => r.Comment == "TestComment" && r.Payer == _payer),
				$"не дождался сохранения сообщения {_payer.Id}");
			Refresh();
			AssertText("TestComment");
		}

		[Test]
		public void Check_free_accounting()
		{
			var user = _payer.Users[0];
			Open($"Billing/Edit?BillingCode={_payer.Id}");
			Css($"#UserRow{user.Id} input[name=accounted]").Checked = true;
			Css($"#UserRow{user.Id} input[name=free]").Checked = true;
			Css("input[name=FreePeriodEnd]").AppendText(DateTime.Now.AddMonths(1).ToShortDateString());
			Css("input[name=AddComment]").AppendText("Check_free_accounting");

			ConfirmDialog();
			Wait(() => {
				var account = user.Accounting;
				session.Refresh(account);
				return account.IsFree;
			}, "Не удалось дождаться обновления учетной информации");

			var accounted = Css($"#UserRow{user.Id} input[name=accounted]");
			Assert.IsFalse(accounted.Checked);
			Assert.IsFalse(accounted.Enabled);
			Assert.That(browser.Text, !Is.StringContaining("Это поле необходимо заполнить."));
			WaitForText(DateTime.Now.AddMonths(1).ToShortDateString());
			Refresh();
			WaitForText("Check_free_accounting");
			AssertText(DateTime.Now.AddMonths(1).ToShortDateString());
		}

		[Test]
		public void Check_report_status_test()
		{
			Open($"Billing/Edit?BillingCode={_payer.Id}");
			Css("#reports input[name=status]").Checked = true;
			Css("#reports input[name=status]").Checked = false;
			Css("input[name=AddComment]").AppendText("Check_report_status_test");
			ConfirmDialog();

			Wait(() => session.Query<PayerAuditRecord>()
				.Any(r => r.Comment == "Check_report_status_test" && r.Payer == _payer),
				$"не дождался сохранения сообщения {_payer.Id}");
			Refresh();
			AssertText("Check_report_status_test");
		}

		[Test]
		public void Check_audit_record_messages_for_client()
		{
			Open($"Billing/Edit?BillingCode={_payer.Id}");

			var record = new AuditRecord("test_message_for_client", _client);
			session.Save(record);

			Flush();

			Assert.IsFalse(browser.CheckBox("filter_Types").Checked);
			browser.CheckBox("filter_Types").Checked = true;
			WaitAjax();

			AssertText("test_message_for_client");
		}
	}
}