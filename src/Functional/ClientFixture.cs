using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Functional.ForTesting;
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
            	var user = new User {Client = client, Name = "test user", Enabled = true,};
            	user.Setup(client);
				scope.VoteCommit();
            }
			using (var browser = Open(String.Format("Client/{0}", client.Id)))
			{
				Assert.IsTrue(browser.Link(Find.ByText("Идентификатор")).Exists);
				Assert.IsTrue(browser.Link(Find.ByText("Имя пользователя")).Exists);
				Assert.That(browser.Table(Find.ByName("usersTable")).Exists);
				// Берем 1-ю и 2-ю строки потому что 0 - это заголовок
				var login1 = Convert.ToInt64(browser.Table(Find.ByName("usersTable")).TableRows[1].TableCells[0].Text);
				var login2 = Convert.ToInt64(browser.Table(Find.ByName("usersTable")).TableRows[2].TableCells[0].Text);
				Assert.That(login1, Is.LessThan(login2));				
				browser.Link(Find.ByText("Идентификатор")).Click();
				login1 = Convert.ToInt64(browser.Table(Find.ByName("usersTable")).TableRows[1].TableCells[0].Text);
				login2 = Convert.ToInt64(browser.Table(Find.ByName("usersTable")).TableRows[2].TableCells[0].Text);
				Assert.That(login1, Is.GreaterThan(login2));
				browser.Link(Find.ByText("Имя пользователя")).Click();
				Assert.That(browser.Table(Find.ByName("usersTable")).Exists);
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
					Assert.That(browser.Text, Text.Contains(logEntity.AFTime.ToString()));
				}
			}
		}
	}
}
