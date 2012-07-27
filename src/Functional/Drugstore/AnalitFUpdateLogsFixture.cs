using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Helpers;
using WatiN.Core; using Test.Support.Web;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using AdminInterface.Models.Logs;
using System.Threading;
using WatiN.Core.Native.InternetExplorer;
using Document = Common.Web.Ui.Models.Document;

namespace Functional.Drugstore
{
	public class AnalitFUpdateLogsFixture : WatinFixture2
	{
		Client client;
		UpdateLogEntity updateLog;
		User user;

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
			Save(updateLog);

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
			Save(updateLog);
			Open("Logs/UpdateLog?regionMask={0}&updateType={1}", 18446744073709551615ul, UpdateType.AccessError);
			Assert.That(browser.Text, Is.StringContaining(user.Login));
		}

		[Test]
		public void Show_update_log()
		{
			updateLog.Log = "Тестовый лог обновления";
			Save(updateLog);
			Refresh();

			Click("Лог");
			browser.WaitUntilContainsText("Тестовый лог обновления", 1);
			Assert.That(browser.Text, Is.StringContaining("Тестовый лог обновления"));
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
			using (var scope = new TransactionScope()) {
				client = DataMother.CreateTestClientWithAddressAndUser();
				supplier = DataMother.CreateSupplier();
				Save(supplier);
				documentLogEntity = DataMother.CreateTestDocumentLog(supplier, client);
				updateLogEntity = DataMother.CreateTestUpdateLogEntity(client);

				ActiveRecordMediator.Save(updateLogEntity);
				documentLogEntity.SendUpdateLogEntity = updateLogEntity;
				Save(documentLogEntity);
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

		[Test]
		public void Show_log_with_long_row()
		{
			updateLog.Log = "Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог";
			Save(updateLog);
			Refresh();
			var logLink = browser.Link(Find.ByText("Лог")).NativeElement as IEElement;
			int offset = logLink.AsHtmlElement.offsetLeft;
			var tbl = browser.Css(".DataTable");
			Click("Лог");
			browser.WaitUntilContainsText("Тестовый лог Тестовый лог", 1);
			Assert.That(browser.Text, Is.StringContaining("Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог"));
			Assert.Less(logLink.AsHtmlElement.offsetLeft, offset*1.5);
		}
	}
}
