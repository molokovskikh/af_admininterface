using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class SendPaymentNotificationFixture : IntegrationFixture
	{
		[Test]
		public void Send_notification_for_debtors()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			payer.Balance = -1000;
			payer.SendPaymentNotification = true;
			payer.Save();
			scope.Flush();
			new SendPaymentNotification().Process();
			var message = UserMessage.Find(client.Users.First().Id);
			Assert.That(message.Message, Is.StringContaining("обслуживание будет приостановлено"));
		}
	}
}