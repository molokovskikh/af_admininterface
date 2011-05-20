using System.Linq;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class ScopeFixture
	{
		[Test]
		public void Broken_scopes()
		{
			using (new SessionScope(FlushAction.Never))
			{
				DataMother.CreateSupplier().Save();
				using (new SessionScope())
				{
					Supplier.Find(12915u);
					using (new SessionScope())
					{
						Supplier.Find(12915u);
					}
				}
			}

			using (new SessionScope(FlushAction.Never))
			{
				DataMother.CreateSupplier().Save();
			}
		}
	}
}