using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class AccountFixture
	{
		[Test]
		public void Do_not_include_account_with_zero_payment_into_invoice()
		{
			var account = new ReportAccount(new Report {
				Allow = true
			});
			Assert.That(account.ShouldPay(), Is.False);
		}
	}
}