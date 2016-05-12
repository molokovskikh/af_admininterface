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

			Open(supplier.Payer);
			AssertText("Плательщик");
			browser.CheckBox(Find.ByName("status")).Click();
			browser.TextField(Find.ByName("AddComment")).AppendText("TestComment");
			ConfirmDialog();

			Wait(() => {
				session.Refresh(supplier);
				return supplier.Disabled;
			}, String.Format("Нe удалось дождаться отключения поставщика {0}", browser.Url));
			Assert.That(supplier.Disabled, Is.True, browser.Url);
		}
	}
}