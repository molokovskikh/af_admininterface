using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class EditFixture : WatinFixture2
	{
		[Test]
		public void Set_null_recipient()
		{
			var payer = DataMother.CreatePayer();
			payer.Recipient = Recipient.FindFirst();
			payer.Save();
			using (var browser = Open(string.Format("Billing/Edit?BillingCode={0}#tab-mail", payer.Id))) {
				browser.ShowWindow(NativeMethods.WindowShowStyle.ShowNormal);
				var selectList = browser.SelectList(Find.ByName("Instance.Recipient.Id"));
				var items = selectList.Options;
				Console.WriteLine(items[0].Value);
				selectList.SelectByValue(items[0].Value);
				browser.TableCell("savePayer").Buttons.First().Click();
			}
			scope.Flush();
			payer.Refresh();
			Assert.IsNull(payer.Recipient);
		}
	}
}
