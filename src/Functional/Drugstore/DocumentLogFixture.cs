using System;
using System.IO;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;
using WatiN.Core; using Test.Support.Web;

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

			client.Save();
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
			AssertText("тестовый адрес");
		}

		[Test]
		public void Resend_document()
		{
			Console.WriteLine(client.Id);
			Open("Logs/Documents?filter.Client.Id={0}", client.Id);
			Click("Повторить");
			AssertText("Документ будет отправлен повторно");

			session.Refresh(sendLog);
			Assert.That(sendLog.Committed, Is.False);
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