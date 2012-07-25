using System;
using System.Threading;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;

namespace Functional.Suppliers
{
	public class BillingFixture : WatinFixture2
	{
		[Test]
		public void Disable_supplier()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;
			Save(supplier);
			scope.Flush();

			Open(supplier.Payer);
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));
			browser.CheckBox(Find.ByName("status")).Click();
			browser.TextField(Find.ByName("AddComment")).AppendText("TestComment");
			browser.Button(Find.ByClass("ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only")).Click();
			Thread.Sleep(500);

			ActiveRecordMediator<Supplier>.Refresh(supplier);
			Assert.That(supplier.Disabled, Is.True);
		}
	}
}