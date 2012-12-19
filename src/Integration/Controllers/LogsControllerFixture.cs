using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using PriceType = AdminInterface.Models.Suppliers.PriceType;

namespace Integration.Controllers
{
	[TestFixture]
	public class LogsControllerFixture : ControllerFixture
	{
		private LogsController _controller;

		[SetUp]
		public void SetUp()
		{
			_controller = new LogsController();
			Prepare(_controller);
		}

		[Test]
		public void ConvertDocumentLineTest()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var address = client.Addresses[0];
			var supplier = DataMother.CreateSupplier();

			session.Save(client);
			session.Save(supplier);

			var document = new DocumentReceiveLog(supplier) {
				FileName = "test.txt",
				ForClient = client,
				Address = address,
			};

			var catalogName = new TestCatalogName { Name = "testName" };
			var catalogForm = new TestCatalogForm { Form = "testForm" };
			session.Save(catalogForm);
			session.Save(catalogName);

			session.Save(document);
			var documentLog = new FullDocument(document);
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

			var assortPrice = supplier.AddPrice("Ассортиментный", PriceType.Assortment);
			client.Settings.AssortimentPrice = assortPrice;
			session.Save(client.Settings);
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
			_controller.Converted(line.Id, client.Id);

			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("0000 Тестовый синоним"));
			Assert.That(_controller.PropertyBag["convertedLine"], Is.StringContaining("1111 Тестовый синоним"));
		}
	}
}
