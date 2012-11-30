using System.Collections.Generic;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using Test.Support;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	public class SupplierFixture : IntegrationFixture
	{
		[Test]
		public void Delete_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.Disabled = true;
			Save(supplier);

			Reopen();
			supplier = session.Get<Supplier>(supplier.Id);
			Assert.That(supplier.CanDelete(session), Is.True);
			supplier.Delete(session);

			Reopen();
			Assert.That(session.Get<Supplier>(supplier.Id), Is.Null);
		}

		[Test]
		public void Ignore_user_check_on_supplier_delete()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;
			supplier.Disabled = true;
			Save(supplier);
			Assert.That(supplier.CanDelete(session), Is.True);
			supplier.Delete(session);
			session.Flush();

			Reopen();
			Assert.That(session.Get<Supplier>(supplier.Id), Is.Null);
		}
	}
}