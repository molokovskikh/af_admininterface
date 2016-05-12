using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using NUnit.Framework;

namespace Functional.Billing
{
	[TestFixture]
	public class BalanceOperationFixture : FunctionalFixture
	{
		private Payer payer;
		private Client client;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();
			payer.Name += payer.Id;
			session.Update(payer);

			Open(payer);
			browser.WaitUntilContainsText("Плательщик", 2);
			AssertText("Плательщик");
		}

		[Test]
		public void Edit_balance_operation()
		{
			var operation = new BalanceOperation(payer) {
				Sum = 500,
				Description = "Тест"
			};
			session.Save(operation);
			FlushAndCommit();

			Click($"Платежи/Счета {DateTime.Today.Year}");
			var selector = $"#balance-summary-{DateTime.Today.Year}-tab";
			WaitForCss(selector);
			Click(Css(selector), "Редактировать");

			AssertText("Возврат");
			Css("#operation_Sum").TypeText("1000");
			Click("Сохранить");
			AssertText("Сохранено");

			session.Refresh(operation);
			Assert.That(operation.Sum, Is.EqualTo(1000));
		}
	}
}