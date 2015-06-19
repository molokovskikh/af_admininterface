using System;
using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class SendPaymentNotificationFixture : AdmIntegrationFixture
	{
		[Test]
		public void Send_notification_for_debtors()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();
			payer.Balance = -1000;
			payer.SendPaymentNotification = true;
			session.Save(payer);
			Flush();
			SystemTime.Now = () => new DateTime(2011, 7, 27, 1, 2, 1);

			new SendPaymentNotification().Process();
			var message = session.Load<UserMessage>(client.Users.First().Id);
			Assert.That(message.Message, Is.StringContaining("обслуживание будет приостановлено"));
			Assert.That(message.ShowMessageCount, Is.EqualTo(1));
		}
	}
}