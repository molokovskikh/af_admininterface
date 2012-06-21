using System;
using System.Linq;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Criterion;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;

namespace Functional.Drugstore
{
	public class OrderFixture : WatinFixture2
	{
		Client client;
		User user;
		ClientOrder order;

		[SetUp]
		public void Setup()
		{
			var supplier = DataMother.CreateSupplier();
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users[0];
			user.AvaliableAddresses.Add(client.Addresses[0]);

			order = new ClientOrder(user, supplier.Prices[0]);

			
			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			var line = new OrderLine(order, product, 100, 1);

			Save(supplier, order, product, line);
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
		public void View_order_details()
		{
			Open("Monitoring/Orders");
			AssertText("Очередь заказов к отправке");
			Click(order.Id.ToString());
			browser.WaitUntilContainsText("Тестовое наименование", 2);
			AssertText("Тестовое наименование");
		}

		[Test]
		public void Check_address_links_when_user_orders_history_show()
		{
			Open(user);
			browser.Link(Find.ByText("История заказов")).Click();

			OpenedWindow(@"История заказов");
			SetCalendarDates(browser);
			browser.Button(Find.ByValue("Показать")).Click();
			Assert.IsTrue(browser.TableBody(Find.ById("SearchResults")).Exists);
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			var addressLinks = browser.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[5].Links;
			Assert.That(addressLinks.Count, Is.EqualTo(1));
			var text = addressLinks[0].Text;
			addressLinks[0].Click();
			Assert.That(browser.Text, Is.StringContaining(String.Format("Адрес доставки {0}", text)));
		}

		[Test]
		public void Check_users_links_when_client_orders_history_show()
		{
			Open(client);
			browser.Link(Find.ByText("История заказов")).Click();

			OpenedWindow(@"История заказов");
			SetCalendarDates(browser);
			browser.Button(Find.ByValue("Показать")).Click();
			Assert.IsTrue(browser.TableBody(Find.ById("SearchResults")).Exists);
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			var userLinks = browser.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[6].Links;
			Assert.That(userLinks.Count, Is.EqualTo(1));
			var text = userLinks[0].Text;
			userLinks[0].Click();
			if (String.IsNullOrEmpty(text))
				Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", text)));
			else
				Assert.That(browser.TextField(Find.ByName("user.Name")).Text, Is.EqualTo(text));
		}

		[Test]
		public void Check_Region_Column()
		{
			Open(client);
			browser.Link(Find.ByText("История заказов")).Click();
			OpenedWindow(@"История заказов");
			SetCalendarDates(browser);
			browser.Button(Find.ByValue("Показать")).Click();
			Assert.IsTrue(browser.TableBody(Find.ById("SearchResults")).Exists);
			//Смотрим, соотв. ли регион тестового клиента колонке в 1 строке таблицы - там должно быть Воронеж
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[7].Text, Is.EqualTo(client.HomeRegion.Name));
		}
	}
}