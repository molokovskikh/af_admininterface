using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Helpers;
using WatiN.Core;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using AdminInterface.Models.Logs;
using System.Threading;
using Document = AdminInterface.Models.Document;

namespace Functional.Drugstore
{
	public class AnalitFUpdateLogsFixture : WatinFixture2
	{
		Client client;
		UpdateLogEntity updateLog;
		User user;

		private void Set_calendar_dates(IE browser)
		{
			var calendarFrom = browser.Div("beginDateCalendarHolder");
			var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
			headerRow.TableCells[1].MouseDown();
			headerRow.TableCells[1].MouseUp();
			headerRow.TableCells[1].MouseDown();
			headerRow.TableCells[1].MouseUp(); //Выбрали 2 месяца назад
		}

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users.First();
			updateLog = new UpdateLogEntity(user) {
				AppVersion = 1000,
				Addition = "Test update",
				Commit = true,
			};
			updateLog.Save();

			Open("Logs/UpdateLog?userId={0}", user.Id);
			Assert.That(browser.Text, Is.StringContaining("История обновлений"));
		}

		[Test]
		public void Show_update_reject_for_supplier_user()
		{
			var user = DataMother.CreateSupplierUser();
			updateLog = new UpdateLogEntity(user) {
				UpdateType = UpdateType.AccessError,
			};
			updateLog.Save();
			Open("Logs/UpdateLog?regionMask={0}&updateType={1}", 18446744073709551615ul, UpdateType.AccessError);
			Assert.That(browser.Text, Is.StringContaining(user.Login));
		}

		[Test]
		public void Show_update_log()
		{
			updateLog.Log = "Тестовый лог обновления";
			updateLog.Save();
			Refresh();

			Click("Лог");
			browser.WaitUntilContainsText("Тестовый лог обновления", 1);
			Assert.That(browser.Text, Is.StringContaining("Тестовый лог обновления"));
		}

		[Test, Ignore("Временно до починки")]
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

		[Test, Ignore("Временно до починки")]
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
			Create_loaded_document_logs_unparsed_document(out client, out supplier, out documentLogEntity, out updateLogEntity);
			using (new TransactionScope()) 
				document = DataMother.CreateTestDocument(supplier, client, documentLogEntity);
		}

		private void Create_loaded_document_logs_unparsed_document(out Client client, out Supplier supplier,
			out DocumentReceiveLog documentLogEntity, out UpdateLogEntity updateLogEntity)
		{
			
			using (var scope = new TransactionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				supplier = DataMother.CreateSupplier();
				Save(supplier);
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

			Open(client);
			Click("История обновлений");
			using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format("История обновлений клиента {0}", client.Name))))
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

		[Test]
		public void View_loaded_documents_details_from_user_update_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			var user = client.Users[0];
			Open(user);
			Click("История обновлений");
			using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format("История обновлений пользователя {0}", user.Login))))
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

		[Test]
		public void View_loaded_documents_details_from_client_document_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			Open(client);
			Click("История документов");
			using (var browser = IE.AttachTo<IE>(Find.ByTitle("История документов")))
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

		[Test]
		public void View_loaded_documents_details_from_user_document_history()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);

			Open(client);
			Click("История документов");
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История документов")))
			{
				Thread.Sleep(2000);
				Assert.That(openedWindow.Text, Is.StringContaining(supplier.Name));
				openedWindow.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
				Thread.Sleep(1000);
				Assert.That(openedWindow.Text, Is.StringContaining("Код товара"));
				Assert.That(openedWindow.Text, Is.StringContaining("Наименование"));
				Assert.That(openedWindow.Text, Is.StringContaining("Производитель"));
				Assert.That(openedWindow.Text, Is.StringContaining("Страна"));
				Assert.That(openedWindow.Text, Is.StringContaining("Количество"));
				Assert.That(openedWindow.Text, Is.StringContaining("Срок годности"));

				Check_document_view(openedWindow, document);
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
				using (var browser = IE.AttachTo<IE>(Find.ByTitle("История документов")))
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
			var user = client.Users[0];
			Open(user);
			Click(@"История обновлений");
			using (var browser = IE.AttachTo<IE>(Find.ByTitle(String.Format("История обновлений пользователя {0}", user.Login))))
			{
				Thread.Sleep(2000);
				Assert.IsTrue(browser.Link(Find.ByText("Загрузка документов на сервер")).Exists);
				browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
				Thread.Sleep(2000);
				Assert.That(browser.Text, Is.StringContaining("Не разобран"));
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
