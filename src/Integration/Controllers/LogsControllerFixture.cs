using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using DocumentType = AdminInterface.Models.Logs.DocumentType;
using PriceType = AdminInterface.Models.Suppliers.PriceType;

namespace Integration.Controllers
{
	[TestFixture]
	public class LogsControllerFixture : ControllerFixture
	{
		private LogsController _controller;
		private Client _client;
		private Supplier _supplier;
		private Address _address;
		private User _user;
		private DocumentReceiveLog _document;

		[SetUp]
		public void SetUp()
		{
			_client = DataMother.CreateTestClientWithAddressAndUser();
			_user = _client.Users[0];
			_address = _client.Addresses[0];
			_supplier = DataMother.CreateSupplier();

			session.Save(_client);
			session.Save(_supplier);

			_document = new DocumentReceiveLog(_supplier) {
				FileName = "test.txt",
				ForClient = _client,
				Address = _address,
			};
			session.Save(_document);


			referer = "http://ya.ru";
			_controller = new LogsController();
			Prepare(_controller);
		}

		private Product _product;
		private Producer _producer;
		private DocumentLine _line;

		private void PrepareTestDocumentLog()
		{
			var catalogName = new TestCatalogName { Name = "testName" };
			var catalogForm = new TestCatalogForm { Form = "testForm" };
			session.Save(catalogForm);
			session.Save(catalogName);

			var documentLog = new FullDocument(_document);
			var catalog = new Catalog { Name = "testCatalog", NameId = catalogName.Id, FormId = catalogForm.Id };
			_product = new Product(catalog);
			_producer = new Producer { Name = "testProducer" };
			session.Save(catalog);
			session.Save(_product);
			session.Save(_producer);
			_line = new DocumentLine {
				CatalogProducer = _producer,
				CatalogProduct = _product,
				Product = "123",
				Document = documentLog
			};
			documentLog.Lines = new List<DocumentLine>();
			documentLog.Lines.Add(_line);
			session.Save(documentLog);
			session.Flush();
		}

		[Test]
		public void ConvertDocumentWithoutAssortmentTest()
		{
			PrepareTestDocumentLog();
			_controller.Converted(_line.Id, _client.Id);

			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("testCatalog"));
			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("testProducer"));
			Assert.That(_controller.PropertyBag["notFindAssortment"], Is.StringContaining("* не указан ассортиментный прайс-лист для конвертации"));
		}

		[Test]
		public void ConvertDocumentLineTest()
		{
			PrepareTestDocumentLog();
			var assortPrice = _supplier.AddPrice("Ассортиментный", PriceType.Assortment);
			_client.Settings.AssortimentPrice = assortPrice;
			session.Save(_client.Settings);
			var synonym = new ProductSynonym("Тестовый синоним");
			session.Save(synonym);
			var prSynonym = new ProducerSynonym("Тестовый синоним");
			session.Save(prSynonym);
			var core = new Core {
				Code = "0000",
				CodeCr = "1111",
				CodeFirmCr = (int)_producer.Id,
				Price = assortPrice,
				ProducerSynonym = prSynonym,
				ProductSynonym = synonym,
				ProductId = (int)_product.Id,
				Quantity = "1"
			};

			session.Save(core);
			session.Flush();
			_controller.Converted(_line.Id, _client.Id);

			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("0000 Тестовый синоним"));
			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("1111 Тестовый синоним"));
		}

		[Test]
		public void ResendIfMultiUserTest()
		{
			var sendLog = new DocumentSendLog(_user, _document) { Committed = true };
			session.Save(sendLog);
			var newUser = _client.AddUser(session, "Новый тестовый пользователь");
			session.Save(_client);

			var newSendLog = new DocumentSendLog(newUser, _document) { Committed = true };
			session.Save(newSendLog);
			session.Save(_document);

			_controller.Resend(new uint[] { _document.Id }, true);
			session.Flush();

			var sendLogs = session.Query<DocumentSendLog>().Where(l => l.Received.Id == _document.Id && l.Committed);
			Assert.That(sendLogs.Any(l => l.Committed), Is.False);
		}

		[Test]
		public void ResendTest()
		{
			var sendLog = new DocumentSendLog(_user, _document) { Committed = true };
			session.Save(sendLog);

			_controller.Resend(new uint[] { sendLog.Id }, false);
			session.Flush();

			var savedLog = session.Load<DocumentSendLog>(sendLog.Id);
			Assert.That(savedLog.Committed, Is.False);
		}

		[Test]
		public void filterDocument()
		{
			var catalogName = new TestCatalogName { Name = "testName" };
			var catalogForm = new TestCatalogForm { Form = "testForm" };
			session.Save(catalogForm);
			session.Save(catalogName);
			var catalog = new Catalog { Name = "testCatalog", NameId = catalogName.Id, FormId = catalogForm.Id };
			_product = new Product(catalog);
			_producer = new Producer { Name = "testProducer" };
			session.Save(catalog);
			session.Save(_product);
			session.Save(_producer);
			session.Flush();

			_document = new DocumentReceiveLog(_supplier)
			{
				FileName = "test.sst",
				ForClient = _client,
				Address = _address,
				DocumentType = DocumentType.Reject,
				Reject = new RejectHeader(),
			};
			session.Save(_document);

			var reject = new RejectHeader()
			{
				Address = _address,
				Lines = new List<RejectLine>(),
				Log = _document,
				Parser = "TestParser",
				Supplier = _supplier,
				WriteTime = SystemTime.Now()
			};
			reject.Lines.Add(new RejectLine() {
				Code = "testCode",
				CodeCr = "testCodeCr",
				Cost = 777,
				Header = reject,
				OrderId = 333,
				Ordered = 111,
				Producer = _producer.Name,
				ProducerEntity = _producer,
				Product = _product.Catalog.Name,
				ProductEntity = _product,
				Rejected = 10
			});
			session.Save(reject);
			_document.Reject = reject;
			session.Save(_document);

			var sendLog = new DocumentSendLog(_user, _document) { Committed = true };
			session.Save(sendLog);
			session.Flush();

			_controller.Documents(new DocumentFilter() {Supplier = _supplier });
			var items = _controller.PropertyBag["logEntities"] as IList<DocumentLog>;
			Assert.AreEqual(1, items.Count(s=>s.DocumentType == DocumentType.Waybill) );
			Assert.AreEqual(1, items.Count(s => s.DocumentType == DocumentType.Reject));

			var rejectFromDocument = items.First(s => s.DocumentType == DocumentType.Reject);
			Assert.AreEqual("test.sst", rejectFromDocument.FileName);
			Assert.AreEqual(_address.Name, rejectFromDocument.Address);
			Assert.AreEqual("TestParser", rejectFromDocument.Parser);
			Assert.AreEqual(_client.Id.ToString(), rejectFromDocument.ClientId);
			Assert.AreEqual(_supplier.Id.ToString(), rejectFromDocument.SupplierId);
		}
	}
}
