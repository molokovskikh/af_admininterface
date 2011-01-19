using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using AdminInterface.Models.Logs;

namespace Functional
{
	public class ClientFixture : WatinFixture
	{
		[Test]
		public void Try_sort_users()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var user = new User(client) {Name = "test user", Enabled = true,};
				user.Setup(client);
				scope.VoteCommit();
			}
			using (var browser = Open(String.Format("Client/{0}", client.Id)))
			{
				Assert.That(browser.Text, Is.StringContaining("клиент"));
				Assert.IsTrue(browser.Link(Find.ByText("Код пользователя")).Exists);
				Assert.IsTrue(browser.Link(Find.ByText("Имя пользователя")).Exists);
				Assert.That(browser.Table(Find.ByName("users")).Exists);
				// Берем 1-ю и 2-ю строки потому что 0 - это заголовок
				var login1 = Convert.ToInt64(browser.Table(Find.ByName("users")).TableRows[1].TableCells[0].Text);
				var login2 = Convert.ToInt64(browser.Table(Find.ByName("users")).TableRows[2].TableCells[0].Text);
				Assert.That(login1, Is.LessThan(login2));				
				browser.Link(Find.ByText("Код пользователя")).Click();
				login1 = Convert.ToInt64(browser.Table(Find.ByName("users")).TableRows[1].TableCells[0].Text);
				login2 = Convert.ToInt64(browser.Table(Find.ByName("users")).TableRows[2].TableCells[0].Text);
				Assert.That(login1, Is.GreaterThan(login2));
				browser.Link(Find.ByText("Имя пользователя")).Click();
				Assert.That(browser.Table(Find.ByName("users")).Exists);
			}
		}

		[Test]
		public void Check_last_service_usage()
		{
			using (new SessionScope())
			{
				var logEntity = AuthorizationLogEntity.Queryable.OrderByDescending(item => item.Id).First(entity => entity.AFTime != null);
				var user = User.Find(logEntity.Id);
				using (var browser = Open(String.Format("Client/{0}", user.Client.Id)))
				{
					Assert.That(browser.Text, Is.StringContaining(logEntity.AFTime.ToString()));
				}
			}
		}

		[Test]
		public void View_client_message_from_user()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var scope = new TransactionScope())
			{				
				var user = new User(client) {Name = "User2",};
				user.Setup(client);
				user.SaveAndFlush();
				client.Users.Add(user);
				client.UpdateAndFlush();
				scope.VoteCommit();
				client = Client.Find(client.Id);
			}
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.TextField(Find.ByName("message")).TypeText("This message for client");
				browser.Button(Find.ByValue("Принять")).Click();
				
				browser.Link(Find.ByText(client.Users[0].Login)).Click();
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
				browser.TextField(Find.ByName("message")).TypeText("This message for user1");
				browser.Button(Find.ByValue("Принять")).Click();

				browser.GoTo(BuildTestUrl(String.Format("users/{0}/edit", client.Users[1].Id)));
				browser.Refresh();
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
				Assert.That(browser.Text, Text.DoesNotContain("This message for user1"));
				browser.TextField(Find.ByName("message")).TypeText("This message for user2");
				browser.Button(Find.ByValue("Принять")).Click();

				browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
				browser.Refresh();
				Assert.That(browser.Text, Is.StringContaining("This message for user1"));
				Assert.That(browser.Text, Is.StringContaining("This message for user2"));
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
			}
		}

		[Test]
		public void Sort_messages()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.TextField(Find.ByName("message")).TypeText("This message for client");
				browser.Button(Find.ByValue("Принять")).Click();
				browser.Link(Find.ByText(client.Users[0].Login)).Click();
				browser.TextField(Find.ByName("message")).TypeText("This message for user1");
				browser.Button(Find.ByValue("Принять")).Click();
				browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
				browser.Refresh();

				Assert.IsTrue(browser.Link(Find.ByText("Дата")).Exists);
				Assert.IsTrue(browser.Link(Find.ByText("Оператор")).Exists);
				Assert.IsTrue(browser.Link(Find.ByText("Пользователь")).Exists);

				browser.Link(Find.ByText("Дата")).Click();
				browser.Link(Find.ByText("Дата")).Click();
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
				Assert.IsTrue(browser.Table("ClientMessagesTable").Exists);

				browser.Link(Find.ByText("Оператор")).Click();
				browser.Link(Find.ByText("Оператор")).Click();
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
				Assert.IsTrue(browser.Table("ClientMessagesTable").Exists);

				browser.Link(Find.ByText("Пользователь")).Click();
				browser.Link(Find.ByText("Пользователь")).Click();
				Assert.That(browser.Text, Is.StringContaining("This message for client"));
				Assert.IsTrue(browser.Table("ClientMessagesTable").Exists);
			}
		}

		[Test]
		public void Client_page_must_contains_client_id()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				Assert.That(browser.Text, Is.StringContaining(String.Format("Клиент {0}, Код {1}", client.Name, client.Id)));
			}
		}
	}
}
