using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class UserFixture
	{
		//после регистрации пользователя для поставщика он должен получить максимальную маску регионов что бы 
		//иметь доступ в интерфейс поставщика
		[Test]
		public void Set_user_supplier_mask()
		{
			var supplier = new Supplier();
			var user = new User(supplier);
			Assert.That(user.WorkRegionMask, Is.EqualTo(ulong.MaxValue));
		}
	}
}