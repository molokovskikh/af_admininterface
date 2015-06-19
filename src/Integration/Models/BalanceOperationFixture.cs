using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class BalanceOperationFixture : AdmIntegrationFixture
	{
		[Test]
		public void Update_payer_balance()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var operation = new BalanceOperation(payer);
			operation.Type = OperationType.DebtRelief;
			operation.Description = "Возврат средств";
			operation.Sum = 1000;
			session.Save(operation);

			Flush();
			Reopen();

			payer = session.Load<Payer>(payer.Id);
			Assert.That(payer.Balance, Is.EqualTo(1000));
		}
	}
}