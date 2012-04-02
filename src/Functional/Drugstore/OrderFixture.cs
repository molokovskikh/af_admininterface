using System;
using System.Linq;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Criterion;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Drugstore
{
	public class OrderFixture : WatinFixture2
	{
		Client client;
		User user;

		[SetUp]
		public void Setup()
		{
			var supplier = DataMother.CreateSupplier();
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users[0];
			user.AvaliableAddresses.Add(client.Addresses[0]);
			var order = new ClientOrder(user, supplier.Prices[0]);
			Save(supplier, order);
		}

		[Test]
		public void View_order_queue()
		{
			Open();
			var cell = browser.TableCell(Find.ByClass("statistic-label") && Find.ByText("Очередь:"));
			var linkCell = cell.ContainingTableRow.TableCells[cell.Index + 1];
			linkCell.Links[0].Click();
			OpenedWindow("Очередь заказов к отправке");
			AssertText("Очередь заказов к отправке");
		}

		[Test]
		public void Check_address_links_when_user_orders_history_show()
		{
			Open(user);
			browser.Link(Find.ByText("История заказов")).Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(@"История заказов")))
			{
				SetCalendarDates(openedWindow);
				openedWindow.Button(Find.ByValue("Показать")).Click();
				Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
				Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				var addressLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[5].Links;
				Assert.That(addressLinks.Count, Is.EqualTo(1));
				var text = addressLinks[0].Text;
				addressLinks[0].Click();
				Assert.That(openedWindow.Text, Is.StringContaining(String.Format("Адрес доставки {0}", text)));
			}
		}

		[Test]
		public void Check_users_links_when_client_orders_history_show()
		{
			Open(client);
			browser.Link(Find.ByText("История заказов")).Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(@"История заказов")))
			{
				SetCalendarDates(openedWindow);
				openedWindow.Button(Find.ByValue("Показать")).Click();
				Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
				Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				var userLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[6].Links;
				Assert.That(userLinks.Count, Is.EqualTo(1));
				var text = userLinks[0].Text;
				userLinks[0].Click();
				if (String.IsNullOrEmpty(text))
					Assert.That(openedWindow.Text, Is.StringContaining(String.Format("Пользователь {0}", text)));
				else
					Assert.That(openedWindow.TextField(Find.ByName("user.Name")).Text, Is.EqualTo(text));
			}
		}
	}
}