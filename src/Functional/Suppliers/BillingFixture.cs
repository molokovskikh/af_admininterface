using System;
using System.Threading;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;

namespace Functional.Suppliers
{
	public class BillingFixture : FunctionalFixture
	{
		[Test]
		public void Disable_supplier()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;
			Save(supplier);
			Flush();

			Open(supplier.Payer);
			AssertText("Плательщик");
			browser.CheckBox(Find.ByName("status")).Click();
			browser.TextField(Find.ByName("AddComment")).AppendText("TestComment");
			ConfirmDialog();
			Thread.Sleep(500);

			session.Refresh(supplier);
			Assert.That(supplier.Disabled, Is.True);
		}
	}
}