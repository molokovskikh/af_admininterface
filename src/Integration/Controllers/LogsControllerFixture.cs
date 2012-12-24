﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
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

		[Test]
		public void ConvertDocumentLineTest()
		{
			var catalogName = new TestCatalogName { Name = "testName" };
			var catalogForm = new TestCatalogForm { Form = "testForm" };
			session.Save(catalogForm);
			session.Save(catalogName);

			var documentLog = new FullDocument(_document);
			var catalog = new Catalog { Name = "testCatalog", NameId = catalogName.Id, FormId = catalogForm.Id };
			var product = new Product(catalog);
			var producer = new Producer { Name = "testProducer" };
			session.Save(catalog);
			session.Save(product);
			session.Save(producer);
			var line = new DocumentLine {
				CatalogProducer = producer,
				CatalogProduct = product,
				Product = "123",
				Document = documentLog
			};
			documentLog.Lines = new List<DocumentLine>();
			documentLog.Lines.Add(line);
			session.Save(documentLog);

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
				CodeFirmCr = (int)producer.Id,
				Price = assortPrice,
				ProducerSynonym = prSynonym,
				ProductSynonym = synonym,
				ProductId = (int)product.Id,
				Quantity = "1"
			};

			session.Save(core);
			session.Flush();
			_controller.Converted(line.Id, _client.Id);

			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("0000 Тестовый синоним"));
			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("1111 Тестовый синоним"));
		}

		[Test]
		public void ResendIfMultiUserTest()
		{
			var sendLog = new DocumentSendLog(_user, _document) { Committed = true };
			session.Save(sendLog);
			var newUser = _client.AddUser("Новый тестовый пользователь");
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
	}
}