using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class BalanceOperationFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Update_payer_balance()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var operation = new BalanceOperation(payer);
			operation.Type = OperationType.DebtRelief;
			operation.Description = "Возврат средств";
			operation.Sum = 1000;
			operation.Save();

			Flush();
			Reopen();

			payer = Payer.Find(payer.Id);
			Assert.That(payer.Balance, Is.EqualTo(1000));
		}
	}
}