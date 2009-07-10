using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class BillingFixture : WatinFixture
	{
		[Test]
		public void User_can_send_message_to_client()
		{
			ForTest.InitialzeAR();

			var message = ClientMessage.Find(2575u);
			message.Message = "1";
			message.ShowMessageCount = 0;
			message.UpdateAndFlush();

			using (var browser = new IE(BuildTestUrl("Billing/edit?clientCode=2575")))
			{
				browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText("Тестовое сообщение");
				browser.Button(Find.ByValue("Отправить сообщение")).Click();
				Assert.That(browser.Text, Text.Contains("Сообщение сохранено"));
			}

			message.Refresh();
			Assert.That(message.Message, Is.EqualTo("Тестовое сообщение"));
			Assert.That(message.ShowMessageCount, Is.EqualTo(1));
		}
	}
}
