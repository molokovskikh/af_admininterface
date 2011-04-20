using System;
using System.Collections.Generic;
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
			var legalEntity = new LegalEntity();
			var payer = new Payer {
				Name = "test",
				ContactGroupOwner = new ContactGroupOwner(),
				JuridicalOrganizations = new List<LegalEntity> {
					legalEntity
				}
			};
			legalEntity.Payer = payer;
			var supplier = new Supplier {
				Payer = payer
			};
			var user = new User(payer, supplier);
			user.Login = new Random().Next().ToString();
			user.Accounting.Accounted();
			payer.Users.Add(user);
			Assert.That(payer.TotalSum, Is.EqualTo(800));
		}
	}
}