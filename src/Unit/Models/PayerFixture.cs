using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class PayerFixture
	{
		[Test]
		public void Payer_get_total_sum_for_supplier()
		{
			var payer = new Payer("Тестовый поставщика", "Тестовый поставщика");
			var supplier = new Supplier(Data.DefaultRegion) {
				Payer = payer
			};
			var user = new User(payer, supplier);
			user.Login = new Random().Next().ToString();
			user.Accounting.Accounted();
			payer.Users.Add(user);
			Assert.That(payer.TotalSum, Is.EqualTo(800));
		}

		[Test]
		public void Update_payer_payment_sum_on_user_disabled()
		{
			var payer = new Payer("Тестовый плательщик");
			var client = new Client(payer, new Region {
				UserPayment = 800,
				AddressPayment = 200
			});
			var user = new User(client);
			client.AddUser(user);
			user.Accounting.Accounted();
			payer.PaymentSum = payer.TotalSum;
			Assert.That(payer.PaymentSum, Is.EqualTo(800));
			user.Enabled = false;
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
		}

		[Test]
		public void Update_payer_payment_sum_on_client_disabled()
		{
			var payer = new Payer("Тестовый плательщик");
			var client = new Client(payer, Data.DefaultRegion);
			var user = new User(client);
			client.AddUser(user);
			user.Accounting.Accounted();
			payer.PaymentSum = payer.TotalSum;
			Assert.That(payer.PaymentSum, Is.EqualTo(800));
			client.Disabled = true;
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
		}
	}
}
