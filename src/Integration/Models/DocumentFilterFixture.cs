using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Logs;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class DocumentFilterFixture : Test.Support.IntegrationFixture
	{
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
			for (int i = 0; i < 1000; i++)
			{
				var client = DataMother.CreateTestClientWithAddress();
				client.Save();
				Save(DataMother.CreateTestDocumentLog(supplier, client));
			}
			Console.WriteLine(supplier.Id);
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
	}
}