using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Helpers;
using WatiN.Core;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using AdminInterface.Models.Logs;
using Document=AdminInterface.Models.Document;
using System.Threading;

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
				using (var browser = Open("users/{0}/edit", user.Id))
				{
					browser.Link(Find.ByText("История заказов")).Click();
					using (
						var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format(@"История заказов пользователя {0}", user.Login))))
					{
						Set_calendar_dates(openedWindow);
						openedWindow.Button(Find.ByValue("Показать")).Click();
						Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
						Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
						var addressLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[4].Links;
						Assert.That(addressLinks.Count, Is.EqualTo(1));
						var text = addressLinks[0].Text;
						addressLinks[0].Click();
						Assert.That(openedWindow.Text, Is.StringContaining(String.Format("Адрес доставки {0}", text)));
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
			using (var browser = Open(client))
			{
				browser.Link(Find.ByText("История заказов")).Click();
				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("История заказов клиента {0}", client.Name))))
				{
					Set_calendar_dates(openedWindow);
					openedWindow.Button(Find.ByValue("Показать")).Click();
					Assert.IsTrue(openedWindow.TableBody(Find.ById("SearchResults")).Exists);
					Assert.That(openedWindow.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
					var userLinks = openedWindow.TableBody(Find.ById("SearchResults")).TableRows[0].TableCells[5].Links;
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

		private void Create_loaded_document_logs(out Client client, out Supplier supplier, out DocumentReceiveLog documentLogEntity,
			out Document document, out UpdateLogEntity updateLogEntity)
		{
			using (var scope = new TransactionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				supplier = DataMother.CreateTestSupplier();
				documentLogEntity = DataMother.CreateTestDocumentLog(supplier, client);
				document = DataMother.CreateTestDocument(supplier, client, documentLogEntity);
				updateLogEntity = DataMother.CreateTestUpdateLogEntity(client);

				//documentLogEntity.SendUpdateId = updateLogEntity.Id;
				documentLogEntity.SendUpdateLogEntity = updateLogEntity;
				documentLogEntity.SaveAndFlush();
				scope.VoteCommit();
			}			
		}

		private void Create_loaded_document_logs_unparsed_document(out Client client, out Supplier supplier,
			out DocumentReceiveLog documentLogEntity, out UpdateLogEntity updateLogEntity)
		{
			using (var scope = new TransactionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				supplier = DataMother.CreateTestSupplier();
				documentLogEntity = DataMother.CreateTestDocumentLog(supplier, client);
				updateLogEntity = DataMother.CreateTestUpdateLogEntity(client);

				//documentLogEntity.SendUpdateId = updateLogEntity.Id;
				documentLogEntity.SendUpdateLogEntity = updateLogEntity;
				documentLogEntity.SaveAndFlush();
				scope.VoteCommit();
			}
		}

		[Test]
		public void View_loaded_documents_details_from_client_update_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			using (var mainWindow = Open("Client/{0}", client.Id))
			{
				mainWindow.Link(Find.ByText("История обновлений")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика обновлений"))))
				{
					Thread.Sleep(2000);
					Assert.IsTrue(browser.Link(Find.ByText("Загрузка документов на сервер")).Exists);
					browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
					Thread.Sleep(2000);

					Assert.That(browser.Text, Is.StringContaining("Дата загрузки"));
					Assert.That(browser.Text, Is.StringContaining("Тип документа"));
					Assert.That(browser.Text, Is.StringContaining("Дата разбора"));
					Assert.That(browser.Text, Is.StringContaining("Имя файла"));
					Assert.That(browser.Text, Is.StringContaining("Статус"));
					Assert.That(browser.Text, Is.StringContaining("Разобран"));
					Assert.That(browser.Text, Is.StringContaining(supplier.Name));

					browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
					Check_document_view(browser, document);
				}
			}
		}

		[Test]
		public void View_loaded_documents_details_from_user_update_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			using (var mainWindow = Open("Users/{0}/edit", client.Users[0].Id))
			{
				mainWindow.Link(Find.ByText("История обновлений")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика обновлений"))))
				{
					Thread.Sleep(2000);
					browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
					
					Thread.Sleep(2000);
					Assert.That(browser.Text, Is.StringContaining("Дата загрузки"));
					Assert.That(browser.Text, Is.StringContaining("Тип документа"));
					Assert.That(browser.Text, Is.StringContaining("Дата разбора"));
					Assert.That(browser.Text, Is.StringContaining("Имя файла"));
					Assert.That(browser.Text, Is.StringContaining("Статус"));
					Assert.That(browser.Text, Is.StringContaining("Разобран"));
					Assert.That(browser.Text, Is.StringContaining(supplier.Name));

					browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
					Check_document_view(browser, document);
				}
			}
		}

		[Test]
		public void View_loaded_documents_details_from_client_document_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			using (var mainWindow = Open("Client/{0}", client.Id))
			{
				mainWindow.Link(Find.ByText(@"История документов")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format(@"Статистика получения\отправки документов клиента " + client.Name))))
				{
					Thread.Sleep(2000);
					Assert.That(browser.Text, Is.StringContaining(supplier.Name));
					browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
					Thread.Sleep(1000);
					Assert.That(browser.Text, Is.StringContaining("Код товара"));
					Assert.That(browser.Text, Is.StringContaining("Наименование"));
					Assert.That(browser.Text, Is.StringContaining("Производитель"));
					Assert.That(browser.Text, Is.StringContaining("Страна"));
					Assert.That(browser.Text, Is.StringContaining("Количество"));
					Assert.That(browser.Text, Is.StringContaining("Срок годности"));

					Check_document_view(browser, document);
				}
			}
		}

		[Test]
		public void View_loaded_documents_details_from_user_document_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			using (var mainWindow = Open("Users/{0}/edit", client.Users[0].Id))
			{
				mainWindow.Link(Find.ByText(@"История документов")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format(@"Статистика получения\отправки документов пользователя " + client.Users[0].Login))))
				{
					Thread.Sleep(2000);
					Assert.That(browser.Text, Is.StringContaining(supplier.Name));
					browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
					Thread.Sleep(1000);
					Assert.That(browser.Text, Is.StringContaining("Код товара"));
					Assert.That(browser.Text, Is.StringContaining("Наименование"));
					Assert.That(browser.Text, Is.StringContaining("Производитель"));
					Assert.That(browser.Text, Is.StringContaining("Страна"));
					Assert.That(browser.Text, Is.StringContaining("Количество"));
					Assert.That(browser.Text, Is.StringContaining("Срок годности"));

					Check_document_view(browser, document);
				}
			}
		}

		[Test]
		public void View_loaded_documents_details_unparsed_document_documents()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs_unparsed_document(out client, out supplier, out documentLogEntity, out updateEntity);
			using (var mainWindow = Open("Users/{0}/edit", client.Users[0].Id))
			{
				mainWindow.Link(Find.ByText(@"История документов")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format(@"Статистика получения\отправки документов пользователя " + client.Users[0].Login))))
				{
					Thread.Sleep(2000);
					Assert.That(browser.Text, Is.StringContaining(supplier.Name));
					Assert.That(browser.Text, Is.StringContaining(documentLogEntity.FileName));
					Assert.IsFalse(browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Exists);
				}
			}
		}

		[Test]
		public void View_loaded_documents_details_unparsed_document_updates()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs_unparsed_document(out client, out supplier, out documentLogEntity, out updateEntity);
			using (var mainWindow = Open("Users/{0}/edit", client.Users[0].Id))
			{
				mainWindow.Link(Find.ByText(@"История обновлений")).Click();
				using (var browser = IE.AttachTo<IE>(Find.ByTitle(@"Статистика обновлений")))
				{
					Thread.Sleep(2000);
					Assert.IsTrue(browser.Link(Find.ByText("Загрузка документов на сервер")).Exists);
					browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
					Thread.Sleep(2000);
					Assert.That(browser.Text, Is.StringContaining("Не разобран"));
				}
			}
		}

		private void Check_document_view(IE browser, Document document)
		{
			Thread.Sleep(2000);
			Assert.That(browser.Text, Is.StringContaining(document.ProviderDocumentId));
			Assert.That(browser.Text, Is.StringContaining(document.Lines[0].Producer));
			Assert.That(browser.Text, Is.StringContaining(document.Lines[0].Country));
			Assert.That(browser.Text, Is.StringContaining(ViewHelper.CostFormat(document.Lines[0].ProducerCost, 2)));
			Assert.That(browser.Text, Is.StringContaining(ViewHelper.CostFormat(document.Lines[0].Nds, 2)));
			Assert.That(browser.Text, Is.StringContaining(document.Lines[0].Certificates));
		}
	}
}
