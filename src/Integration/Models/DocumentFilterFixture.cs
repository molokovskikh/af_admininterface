using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class DocumentFilterFixture : Test.Support.IntegrationFixture
	{
		private Supplier _supplier;
		private DocumentReceiveLog _documentLog;
		private Client _client;
		[SetUp]
		public void SetUp()
		{
			// Создаем поставщика, клиента, лог документа
			_supplier = DataMother.CreateSupplier();
			Save(_supplier);
			_client = DataMother.CreateClientAndUsers();
			Save(_client);
			_documentLog = new DocumentReceiveLog(_supplier);
			_documentLog.ForClient = _client;
			Save(_documentLog);
		}

		[Test]
		public void Get_document_error_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var document = new DocumentReceiveLog(supplier);
			Save(document);
			Flush();

			var filter = new DocumentFilter();
			filter.Supplier = supplier;
			var documents = filter.Find();
			Assert.That(documents.Count, Is.GreaterThan(0));
			Assert.That(documents.Any(d => d.Id == document.Id), Is.True,
				"должен быть {0} но есть {1}", document.Id, documents.Implode(d => d.Id));
		}


		[Test, Ignore("Нагрузочный тест")]
		public void Build_data()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			for (int i = 0; i < 1000; i++) {
				var client = DataMother.CreateTestClientWithAddress();
				session.SaveOrUpdate(client);
				Save(DataMother.CreateTestDocumentLog(supplier, client));
			}
		}

		[Test]
		public void FilterGetAllDocumentsIfNoOnlyNoParsedTest()
		{
			// Создаем поставщика
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			// Создаем много документов, чтобы не влезали на одну страницу
			for (int i = 0; i < 33; i++) {
				var documentLog = new DocumentReceiveLog(supplier);
				Save(documentLog);
			}
			// Создаем фильтр и устанавливаем параметр Только неразобранные
			var filter = new DocumentFilter();
			filter.Supplier = supplier;
			filter.OnlyNoParsed = true;

			var documents = filter.Find();
			// должны получить документы в количестве равном одной странице
			Assert.That(documents.Count, Is.EqualTo(filter.PageSize));

			// ищем все документы
			filter.OnlyNoParsed = false;
			documents = filter.Find();
			// должны получить документы в количестве большем одной страницы
			Assert.That(documents.Count, Is.GreaterThan(filter.PageSize));
		}

		[Test(Description = "проверям работу DocumentLog")]
		public void CheckDocumentProcessedSuccessfully()
		{
			//Для документов с DocumentSendLogs.Id > 15374942 все должно работать по новому,

			var log = new DocumentLog();
			log.DeliveredId = 15374943;
			log.FileDelivered = null;
			log.DocumentDelivered = null;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(false));

			log.FileDelivered = false;
			log.DocumentDelivered = null;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(false));

			log.FileDelivered = false;
			log.DocumentDelivered = false;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(false));

			log.FileDelivered = true;
			log.DocumentDelivered = false;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(true));

			log.FileDelivered = false;
			log.DocumentDelivered = true;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(true));


			//для старых документов - они всегда должно помечатся как ProcessedSuccessfully и отдавать RequestTime сразу

			log.DeliveredId = 1;
			log.FileDelivered = null;
			log.DocumentDelivered = null;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(true));

			log.FileDelivered = false;
			log.DocumentDelivered = false;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(true));

			log.FileDelivered = false;
			log.DocumentDelivered = true;
			Assert.That(log.DocumentProcessedSuccessfully(), Is.EqualTo(true));
		}

		[Test]
		public void OnlyNoParsedWithSendLogsTest()
		{
			// создаем фильтр, устанавливаем в качестве параметра созданного поставщика и "искать только неразобранные"
			var filter = new DocumentFilter();
			filter.Supplier = _supplier;
			filter.OnlyNoParsed = true;
			var documents = filter.Find();
			// Добавляем логи отправки
			_documentLog.SendLogs = new List<DocumentSendLog>();
			_documentLog.SendLogs.Add(new DocumentSendLog(_client.Users[0], _documentLog));
			_documentLog.SendLogs.Add(new DocumentSendLog(_client.Users[1], _documentLog));
			Save(_documentLog.SendLogs);
			Save(_documentLog);
			// проверяем, что два сохраненных лога не дают дублирование документа
			var documentsWithSendLogs = filter.Find();
			Assert.That(documents.Count, Is.GreaterThan(0));
			Assert.That(documents.Count, Is.EqualTo(documentsWithSendLogs.Count));
		}

		[Test]
		public void OnlyNoParcedWithDocumentTest()
		{
			// создаем фильтр, устанавливаем в качестве параметра созданного поставщика и "искать только неразобранные"
			var filter = new DocumentFilter();
			filter.Supplier = _supplier;
			filter.OnlyNoParsed = true;
			// создаем документ для лога
			var document = DataMother.CreateTestDocument(_supplier, _client, _documentLog);
			Save(document);
			// не должны выбрать запись лога, так как уже есть документ
			var documents = filter.Find();
			Assert.That(documents.Count, Is.EqualTo(0));
		}

		[Test]
		public void OnlyNoParcedWithFakeTest()
		{
			_documentLog.IsFake = true;
			Save(_documentLog);
			Flush();
			// создаем фильтр, устанавливаем в качестве параметра созданного поставщика и "искать только неразобранные"
			var filter = new DocumentFilter();
			filter.Supplier = _supplier;
			filter.OnlyNoParsed = true;
			// ищем
			var documents = filter.Find();
			// не должны получить сохраненный выше документ из-за установленного IsFake
			Assert.That(documents.Count, Is.EqualTo(0));
		}
	}
}