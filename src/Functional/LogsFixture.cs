using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Helpers;
using WatiN.Core;
using AdminInterface.Models;
using Castle.ActiveRecord;

namespace Functional
{
	public class LogsFixture : WatinFixture
	{
		private void Set_calendar_dates(IE browser)
		{
			var calendarFrom = browser.Div("beginDateCalendarHolder");
			var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
			headerRow.TableCells[1].MouseDown();
			headerRow.TableCells[1].MouseUp();
			headerRow.TableCells[1].MouseDown();
			headerRow.TableCells[1].MouseUp(); //Выбрали 2 месяца назад			
		}

		[Test]
		public void Check_address_links_when_user_orders_history_show()
		{
			using (new SessionScope())
			{
				var sql = @"select max(ClientCode) from orders.ordershead join future.clients on clients.Id = ordershead.ClientCode";
				var clientId = String.Empty;
				ArHelper.WithSession(session => clientId = session.CreateSQLQuery(sql).UniqueResult().ToString());
				var client = Client.Find(Convert.ToUInt32(clientId));
				var user = client.Users.First();
				using (var browser = Open("users/{0}/edit", user.Login))
				{
					browser.Link(Find.ByText("История заказов")).Click();
					using (
						var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format(@"История заказов пользователя {0}", user.Login))))
					{
						Set_calendar_dates(openedWindow);
						openedWindow.Button(Find.ByValue("Показать")).Click();
						Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
						Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
						var addressLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[4].Links;
						Assert.That(addressLinks.Length, Is.EqualTo(1));
						var text = addressLinks[0].Text;
						addressLinks[0].Click();
						Assert.That(openedWindow.Text, Text.Contains(String.Format("Адрес доставки {0}", text)));
					}
				}
			}
		}

		[Test]
		public void Check_users_links_when_client_orders_history_show()
		{
			var sql = @"select max(ClientCode) from orders.ordershead join future.clients on clients.Id = ordershead.ClientCode";
			var clientId = String.Empty;
			ArHelper.WithSession(session => clientId = session.CreateSQLQuery(sql).UniqueResult().ToString());
			var client = Client.Find(Convert.ToUInt32(clientId));
			using (var browser = Open("client/{0}", clientId))
			{
				browser.Link(Find.ByText("История заказов")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format("История заказов клиента {0}", client.Name))))
				{
					Set_calendar_dates(openedWindow);
					openedWindow.Button(Find.ByValue("Показать")).Click();
					Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
					Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
					var userLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[5].Links;
					Assert.That(userLinks.Length, Is.EqualTo(1));
					var text = userLinks[0].Text;
					userLinks[0].Click();
					if (String.IsNullOrEmpty(text))
						Assert.That(openedWindow.Text, Text.Contains(String.Format("Пользователь {0}", text)));
					else
						Assert.That(openedWindow.TextField(Find.ByName("user.Name")).Text, Is.EqualTo(text));
				}				
			}			
		}
	}
}
