using System;
using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Tools;
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
			var supplier = new Supplier {
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
			var client = new Client(payer);
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
			var client = new Client(payer);
			var user = new User(client);
			client.AddUser(user);
			user.Accounting.Accounted();
			payer.PaymentSum = payer.TotalSum;
			Assert.That(payer.PaymentSum, Is.EqualTo(800));
			client.Disabled = true;
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
		}


		[Test, Ignore("Не реализовано #4979, бухгалтерия думает")]
		public void Do_not_notify_user_if_billing_by_quater_but_current_month_payed()
		{
			var payer = new Payer("Тестовый поставщика", "Тестовый поставщика");
			var client = new Client(payer);
			var user = new User(client);
			user.Accounting.Accounted();

			Assert.That(payer.TotalSum, Is.EqualTo(800));
			payer.PayCycle = InvoicePeriod.Quarter;

			SystemTime.Now = () => new DateTime(2011, 1, 1);
			payer.Balance = -2400;
			Assert.That(payer.ShouldNotify(), Is.True);
			payer.Balance += 800;
			Assert.That(payer.ShouldNotify(), Is.False);

			SystemTime.Now = () => new DateTime(2011, 2, 1);
			Assert.That(payer.ShouldNotify(), Is.True);
			payer.Balance += 800;
			Assert.That(payer.ShouldNotify(), Is.False);
			SystemTime.Now = () => new DateTime(2011, 2, 26);
			Assert.That(payer.ShouldNotify(), Is.False);
		}
	}
}
