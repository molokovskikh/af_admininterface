using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	public class PaymentFixture : WatinFixture
	{
		public PaymentFixture()
		{
			UseTestScope = true;
		}

		[Test]
		public void Update_payment()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Billing/Edit.rails?BillingCode=", client.Id))
			{
				var payment = browser.TextField("");
				Assert.That(payment.Text, Is.EqualTo(800));
				payment.TypeText(900.ToString());
			}

			var accounting = client.Users[0].Accounting;
			accounting.Refresh();
			Assert.That(accounting.Payment, Is.EqualTo(900));
		}

		[Test]
		public void View_payments()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�������")).Click();
			}
		}
	}
}