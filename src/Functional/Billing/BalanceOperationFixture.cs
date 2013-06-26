using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core.UtilityClasses;
using WatiN.CssSelectorExtensions;

namespace Functional.Billing
{
	[TestFixture]
	public class BalanceOperationFixture : WatinFixture2
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
			session.Flush();

			Click(String.Format("Платежи/Счета {0}", DateTime.Today.Year));
			var selector = String.Format("#balance-summary-{0}-tab", DateTime.Today.Year);
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