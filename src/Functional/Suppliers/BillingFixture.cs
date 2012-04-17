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
			supplier.Save();
			scope.Flush();

			Open(supplier.Payer);
			Assert.That(browser.Text, Is.StringContaining("Плательщик"));
			browser.CheckBox(Find.ByName("status")).Click();
			Thread.Sleep(500);

			ActiveRecordMediator<Supplier>.Refresh(supplier);
			Assert.That(supplier.Disabled, Is.True);
		}
	}
}