using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional.Drugstore
{
	[TestFixture]
	public class DocumentLogFixture : WatinFixture2
	{
		private Client client;
		private Supplier supplier;
		private Address address;
		private User user;
		private DocumentReceiveLog document;
		private DocumentSendLog sendLog;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users[0];
			address = client.Addresses[0];
			supplier = DataMother.CreateSupplier();

			session.SaveOrUpdate(client);
			Save(supplier);

			document = new DocumentReceiveLog(supplier) {
				FileName = "test.txt",
				ForClient = client,
				Address = address,
			};

			sendLog = new DocumentSendLog(user, document);

			Save(document, sendLog);
		}

		[Test]
		public void View_documents()
		{
			Open(client);
			Click("История документов");
			OpenedWindow("История документов");
			AssertText(document.Id.ToString());
			AssertText("Пользователь получивший документ");
			AssertText("тестовый адрес");
		}

		[Test]
		public void Resend_document()
		{
			Open("Logs/Documents?filter.Client.Id={0}", client.Id);
			browser.CheckBox(Find.ByName("ids[0]")).Click();
			Click("Повторить");
			AssertText("Документы будут отправлены повторно");

			session.Refresh(sendLog);
			Assert.That(sendLog.Committed, Is.False);
		}

		[Test]
		public void ShowDocumentForMultiUserClient()
		{
			var newUser = client.AddUser("Новый тестовый пользователь");
			Save(client);

			var newSendLog = new DocumentSendLog(newUser, document);
			Save(newSendLog);
			Reopen();
			Open("Logs/Documents?filter.Client.Id={0}", client.Id);
			AssertText("Статистика");
			Assert.That(browser.Text, Is.Not.StringContaining("Пользователь получивший документ"));
			Click("...");
			browser.WaitUntilContainsText("Пользователь получивший документ", 1);
			AssertText("Пользователь получивший документ");
			AssertText(user.Login);
		}

		[Test]
		public void Producer_in_document_detalization_for_position()
		{
			var catalogName = new TestCatalogName { Name = "testCatName" };
			var catalogForm = new TestCatalogForm { Form = "testForm" };
			Save(catalogName, catalogForm);

			document.FileName = "SpecialFileNameForThisReport";
			session.Save(document);
			var documentLog = new FullDocument(document);
			var catalog = new Catalog { Name = "testCatalog", NameId = catalogName.Id, FormId = catalogForm.Id };
			var product = new Product(catalog);
			var producer = new Producer { Name = "testProducer" };
			Save(catalog, producer, product);
			var line = new DocumentLine {
				CatalogProducer = producer,
				CatalogProduct = product,
				Product = "123",
				Document = documentLog
			};
			documentLog.Lines = new List<DocumentLine>();
			documentLog.Lines.Add(line);
			session.Save(documentLog);

			Open("Logs/Documents?filter.Client.Id={0}", client.Id);

			Click("SpecialFileNameForThisReport");
			Click("123");
			browser.WaitUntilContainsText("Сопоставлен с", 1);
			AssertText("Сопоставлен с \"testCatName testForm\"");
			AssertText("По производителю с \"testProducer\"");
		}

		[Test]
		public void View_documents_with_client_column()
		{
			Open(client);
			Click("История документов");

			OpenedWindow("История документов");
			//Смотрим, есть ли надпись "Клиенту"
			AssertText("Клиенту");
			//Смотрим, есть ли ссылка на поставщика
			Assert.That(browser.Links.Select(l => l.Text == supplier.Name && l.Text.Contains("Suppliers//" + supplier.Id.ToString())).Count(), Is.GreaterThan(0));
			//Смотрим, есть ли ссылка на клиента
			Assert.That(browser.Links.Select(l => l.Text == client.Name && l.Text.Contains("Clients//" + client.Id.ToString())).Count(), Is.GreaterThan(0));
			//Смотрим, есть ли ссылка на пользователя, получившего документ
			Assert.That(browser.Links.Select(l => l.Text == user.Name && l.Text.Contains("Users//" + user.Id.ToString())).Count(), Is.GreaterThan(0));
		}

		[Test]
		public void View_document_certificates()
		{
			var log = DataMother.CreateTestDocumentLog(supplier, client);
			var document = DataMother.CreateTestDocument(supplier, client, log);
			var line = document.Lines[0];
			line.CatalogProduct = DataMother.Product();
			line.Product = line.CatalogProduct.Catalog.Name;
			line.Certificate = DataMother.Certificate(line.CatalogProduct.Catalog);
			session.Save(line);

			var dir = "../../../AdminInterface/Data/Certificates/";
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var filename = line.Certificate.Files[0].Filename;
			File.WriteAllText(Path.Combine(dir, filename), "");

			Open("Logs/Documents?filter.Client.Id={0}", client.Id);
			AssertText("Тестовый поставщик");
			Click("TestFile.txt");
			browser.WaitUntilContainsText("Страна", 1);
			AssertText("Страна");
			Click(line.Product);
			browser.WaitUntilContainsText(filename, 1);
			AssertText(filename);
		}
	}
}