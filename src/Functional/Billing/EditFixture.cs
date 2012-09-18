﻿using System;
using System.Linq;
using AdminInterface.Models.Billing;
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
		private Payer payer;

		[SetUp]
		public void SetUp()
		{
			payer = DataMother.CreatePayer();
			payer.Recipient = session.Query<Recipient>().First();
			session.Save(payer);
			browser = Open(string.Format("Billing/Edit?BillingCode={0}#tab-mail", payer.Id));
		}

		[Test]
		public void Set_null_recipient()
		{
			var selectList = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			var items = selectList.Options;
			selectList.SelectByValue(items[0].Value);
			browser.TableCell("savePayer").Buttons.First().Click();
			Flush();
			payer.Refresh();
			Assert.IsNull(payer.Recipient);
		}

		[Test]
		public void Check_comment_with_disable_client()
		{
			var client = DataMother.CreateClientAndUsers();
			session.Save(client);
			session.Flush();
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", client.Payers[0].Id));
			var checkBox = browser.Css("#clients input[name=\"status\"]");
			checkBox.Checked = false;
			browser.TextField(Find.ByName("AddComment")).AppendText("TestComment");
			browser.Button(Find.ByClass("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).Click();
			browser.Refresh();
			AssertText("TestComment");
		}

		[Test]
		public void Check_free_accounting()
		{
			var client = DataMother.CreateClientAndUsers();
			session.Save(client);
			session.Flush();
			browser = Open(string.Format("Billing/Edit?BillingCode={0}", client.Payers[0].Id));
			Css("input[name=accounted]").Checked = true;
			Css("input[name=free]").Checked = true;
			Css("input[name=FreePeriodEnd]").AppendText(DateTime.Now.AddMonths(1).ToShortDateString());
			browser.Button(Find.ByClass("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only ui-state-hover")).Click();
			var accounted = Css("input[name=accounted]");
			Assert.IsFalse(accounted.Checked);
			Assert.IsFalse(accounted.Enabled);
		}
	}
}