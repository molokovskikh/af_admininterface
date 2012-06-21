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

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddress();
			supplier = DataMother.CreateSupplier();

			client.Save();
			supplier.Save();
		}

		[Test]
		public void View_documents()
		{
			var document = new DocumentReceiveLog(supplier) {
				FileName = "test.txt",
				ForClient = client,
				Address = client.Addresses.First(),
			};

			document.Save();
			Flush();

			Open(client);
			Click("История документов");
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История документов")))
			{
				Assert.That(openedWindow.Text, Is.StringContaining(document.Id.ToString()));
				Assert.That(openedWindow.Text, Is.StringContaining("тестовый адрес"));
			}
		}

		[Test]
		public void View_documents_with_client_column()
		{
			User testUser = new User(client);
			testUser.Setup();
			
			client.Users.Add(
				testUser
			);
			
			client.Save();
			Flush();

			var document = new DocumentReceiveLog(supplier) {
				FileName = "test.txt",
				ForClient = client,
				Address = client.Addresses.First(),
			};

			document.Save();
			Flush();

			var sendLog = new DocumentSendLog() {
				Committed = true,
				ForUser = client.Users.First(),
				Received = document
			};
			sendLog.Save();
			Flush();

			Open(client);
			Click("История документов");

			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История документов"))) {
				//Смотрим, есть ли надпись "Клиенту"
				Assert.That(openedWindow.ContainsText("Клиенту"));
				//Смотрим, есть ли ссылка на поставщика
				Assert.That(openedWindow.Links.Select(l => l.Text == supplier.Name && l.Text.Contains("Suppliers//" + supplier.Id.ToString())).Count(), Is.GreaterThan(0));
				//Смотрим, есть ли ссылка на клиента
				Assert.That(openedWindow.Links.Select(l => l.Text == client.Name && l.Text.Contains("Clients//" + client.Id.ToString())).Count(), Is.GreaterThan(0));
				//Смотрим, есть ли ссылка на пользователя, получившего документ
				Assert.That(openedWindow.Links.Select(l => l.Text == testUser.Name && l.Text.Contains("Users//" + testUser.Id.ToString())).Count(), Is.GreaterThan(0));
			}
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