using AdminInterface.Models.Suppliers;
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
	}
}