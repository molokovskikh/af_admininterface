using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;

using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class ClientMessagesFixture : WatinFixture
	{
		[Test]
		public void User_can_send_message_to_client()
		{
			ForTest.InitialzeAR();

			List<ClientMessage> messages;

			using (new SessionScope())
			{
				messages = Client.Find(2575u).Users.Select(u => ClientMessage.Find(u.Id)).ToList();
				foreach (var message in messages)
				{
					message.Message = "1";
					message.ShowMessageCount = 0;
					message.Update();
				}
			}

			using (var browser = new IE(BuildTestUrl("Billing/edit?clientCode=2575")))
			{
				browser.TextField(Find.ByName("NewClientMessage.Message")).TypeText("Тестовое сообщение");
				browser.Button(Find.ByValue("Отправить сообщение")).Click();
				Assert.That(browser.Text, Text.Contains("Сообщение сохранено"));
			}

			foreach (var message in messages)
			{
				message.Refresh();
				Assert.That(message.Message, Is.EqualTo("Тестовое сообщение"));
				Assert.That(message.ShowMessageCount, Is.EqualTo(1));	
			}
		}

		[Test]
		public void Cancel_message_for_client()
		{
			ForTest.InitialzeAR();

			List<ClientMessage> messages;

			using (new SessionScope())
			{
				messages = Client.Find(2575u).Users.Select(u => ClientMessage.Find(u.Id)).ToList();
				foreach (var message in messages)
				{
					message.Message = "тестовое сообщение";
					message.ShowMessageCount = 1;
					message.Update();
				}
			}

			using (var browser = new IE(BuildTestUrl("Billing/edit?clientCode=2575")))
			{
				Assert.That(browser.Text, Text.Contains("Остались не показанные сообщения"));
				browser.Link(Find.ByText("Просмотреть сообщение")).Click();
				Thread.Sleep(400);
				Assert.That(browser.Text, Text.Contains("тестовое сообщение"));
				Assert.That(browser.Text, Text.Contains("Осталось показать 1 раз"));
				browser.Button(Find.ByValue("Отменить показ сообщения")).Click();
				Thread.Sleep(400);
				Assert.That(browser.Text, Text.Contains("Сообщение удалено"));

				browser.Refresh();
				Assert.That(browser.Text, Text.DoesNotContain("Остались не показанные сообщения"));
			}


			foreach (var message in messages)
			{
				message.Refresh();
				Assert.That(message.Message, Is.EqualTo("тестовое сообщение"));
				Assert.That(message.ShowMessageCount, Is.EqualTo(0));
			}
		}
	}
}
