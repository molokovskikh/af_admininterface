using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;
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
			payer.Recipient = Recipient.FindFirst();
			payer.Save();
			browser = Open(string.Format("Billing/Edit?BillingCode={0}#tab-mail", payer.Id));
		}

		[Test]
		public void Set_null_recipient()
		{
			var selectList = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
			var items = selectList.Options;
			Console.WriteLine(items[0].Value);
			selectList.SelectByValue(items[0].Value);
			browser.TableCell("savePayer").Buttons.First().Click();
			scope.Flush();
			payer.Refresh();
			Assert.IsNull(payer.Recipient);
		}

		[Test]
		public void Check_comment_with_disable_client()
		{
			var client =  DataMother.CreateClientAndUsers();
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
	}
}
