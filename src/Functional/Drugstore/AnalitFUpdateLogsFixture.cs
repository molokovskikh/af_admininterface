using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Helpers;
using WatiN.Core;
using Test.Support.Web;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.ForTesting;
using AdminInterface.Models.Logs;
using System.Threading;
using WatiN.Core.Native.InternetExplorer;
using Document = Common.Web.Ui.Models.Document;

namespace Functional.Drugstore
{
	public class AnalitFUpdateLogsFixture : FunctionalFixture
	{
		private Client client;
		private UpdateLogEntity updateLog;
		private User user;

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
			AssertText("История обновлений");
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
			AssertText(user.Login);
		}

		[Test]
		public void Show_update_log()
		{
			updateLog.Log = "Тестовый лог обновления";
			Save(updateLog);
			Refresh();

			Click("Лог");
			browser.WaitUntilContainsText("Тестовый лог обновления", 1);
			AssertText("Тестовый лог обновления");
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
			OpenedWindow(String.Format("История обновлений клиента {0}", client.Name));
			Assert.IsTrue(browser.Link(Find.ByText("Загрузка документов на сервер")).Exists);
			browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
			Thread.Sleep(2000);

			AssertText("Дата загрузки");
			AssertText("Тип документа");
			AssertText("Дата разбора");
			AssertText("Имя файла");
			AssertText("Статус");
			AssertText("Разобран");
			AssertText(supplier.Name);

			browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
			Check_document_view(document);
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
			OpenedWindow(String.Format("История обновлений пользователя {0}", user.Login));
			browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();

			Thread.Sleep(2000);
			AssertText("Дата загрузки");
			AssertText("Тип документа");
			AssertText("Дата разбора");
			AssertText("Имя файла");
			AssertText("Статус");
			AssertText("Разобран");
			AssertText(supplier.Name);

			browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
			Check_document_view(document);
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
			OpenedWindow("История документов");
			AssertText(supplier.Name);
			Css("#ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
			Thread.Sleep(1000);
			AssertText("Код товара");
			AssertText("Наименование");
			AssertText("Производитель");
			AssertText("Страна");
			AssertText("Количество");
			AssertText("Срок годности");

			Check_document_view(document);
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
			OpenedWindow("История документов");
			AssertText(supplier.Name);
			browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
			Thread.Sleep(1000);
			AssertText("Код товара");
			AssertText("Наименование");
			AssertText("Производитель");
			AssertText("Страна");
			AssertText("Количество");
			AssertText("Срок годности");

			Check_document_view(document);
		}

		[Test]
		public void View_loaded_documents_details_unparsed_document_documents()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs_unparsed_document(out client, out supplier, out documentLogEntity, out updateEntity);
			Open("Users/{0}/edit", client.Users[0].Id);
			ClickLink("История документов");
			OpenedWindow("История документов");
			AssertText(supplier.Name);
			AssertText(documentLogEntity.FileName);
			Assert.IsFalse(browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Exists);
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
			OpenedWindow(String.Format("История обновлений пользователя {0}", user.Login));
			Assert.IsTrue(browser.Link(Find.ByText("Загрузка документов на сервер")).Exists);
			browser.Link("ShowUpdateDetailsLink" + updateEntity.Id).Click();
			Thread.Sleep(2000);
			AssertText("Не разобран");
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
			var tbl = Css(".DataTable");
			Click("Лог");
			browser.WaitUntilContainsText("Тестовый лог Тестовый лог", 1);
			Assert.That(browser.Text, Is.StringContaining("Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог " +
				"Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог Тестовый лог"));
			Assert.Less(logLink.AsHtmlElement.offsetLeft, offset * 1.5);
		}

		[Test]
		public void CertificateDetailsFileTest()
		{
			Client client = null;
			Supplier supplier = null;
			DocumentReceiveLog documentLogEntity = null;
			Document document = null;
			UpdateLogEntity updateEntity = null;

			Create_loaded_document_logs(out client, out supplier, out documentLogEntity, out document, out updateEntity);
			var sert = new CertificateRequestLog {
				Line = document.Lines[0],
				Update = updateEntity
			};
			Save(sert);
			Flush();
			Open("Main/Stat");
			Css("#StatisticsTD a").Click();
			AssertText("Статистика по сертификатам");
			Thread.Sleep(2000);
			AssertText(supplier.Name);
			browser.Link("ShowDocumentDetailsLink" + documentLogEntity.Id).Click();
			Thread.Sleep(1000);
			AssertText("Код товара");
			AssertText("Наименование");
			AssertText("Производитель");
			AssertText("Страна");
			AssertText("Количество");
			AssertText("Срок годности");

			Thread.Sleep(2000);
			AssertText(document.Lines[0].Producer);
			AssertText(document.Lines[0].Country);
		}

		private void Check_document_view(Document document)
		{
			Thread.Sleep(2000);
			AssertText(document.ProviderDocumentId);
			AssertText(document.Lines[0].Producer);
			AssertText(document.Lines[0].Country);
			AssertText(ViewHelper.CostFormat(document.Lines[0].ProducerCost, 2));
			AssertText(ViewHelper.CostFormat(document.Lines[0].Nds, 2));
			AssertText(document.Lines[0].Certificates);
		}

		private void Create_loaded_document_logs(out Client client, out Supplier supplier, out DocumentReceiveLog documentLogEntity,
			out Document document, out UpdateLogEntity updateLogEntity)
		{
			Create_loaded_document_logs_unparsed_document(out client, out supplier, out documentLogEntity, out updateLogEntity);
			document = DataMother.CreateTestDocument(supplier, client, documentLogEntity);
		}

		private void Create_loaded_document_logs_unparsed_document(out Client client, out Supplier supplier,
			out DocumentReceiveLog documentLogEntity, out UpdateLogEntity updateLogEntity)
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			supplier = DataMother.CreateSupplier();
			Save(supplier);
			documentLogEntity = DataMother.CreateTestDocumentLog(supplier, client);
			updateLogEntity = DataMother.CreateTestUpdateLogEntity(client);

			session.SaveOrUpdate(updateLogEntity);
			documentLogEntity.SendUpdateLogEntity = updateLogEntity;
			Save(documentLogEntity);
		}
	}
}