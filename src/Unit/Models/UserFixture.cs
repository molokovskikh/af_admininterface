using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
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
			var supplier = new Supplier(Data.DefaultRegion, new Payer("Тестовый плательщик"));
			var user = new User(supplier);
			Assert.That(user.WorkRegionMask, Is.EqualTo(ulong.MaxValue));
		}

		[Test]
		public void If_address_registred_with_free_user_it_should_be_free()
		{
			var client = new Client(new Payer(""), Data.DefaultRegion);
			var user = new User(client);
			client.AddUser(user);
			var address = new Address(client);
			client.AddAddress(address);

			user.Accounting.IsFree = true;
			user.Accounting.FreePeriodEnd = DateTime.Today.AddDays(10);

			user.RegistredWith(address);

			Assert.That(address.Accounting.IsFree, Is.True);
			Assert.That(address.Accounting.FreePeriodEnd, Is.EqualTo(DateTime.Today.AddDays(10)));
		}

		[Test]
		public void IsPermissionAssignedTest()
		{
			var client = new Client(new Payer(""), Data.DefaultRegion);
			var user = new User(client);
			client.AddUser(user);

			var permission = new UserPermission { Shortcut = "AF" };
			Assert.That(user.IsPermissionAssigned(permission), Is.False);
			user.AssignedPermissions.Add(new UserPermission { Shortcut = "AF" });
			Assert.That(user.IsPermissionAssigned(permission));
		}
	}
}