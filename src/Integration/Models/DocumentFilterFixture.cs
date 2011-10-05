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
	public class DocumentFilterFixture : IntegrationFixture
	{
		[Test]
		public void Get_document_error_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.Save();
			var document = new DocumentReceiveLog(supplier);
			document.Save();
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
			supplier.Save();
			for (int i = 0; i < 1000; i++)
			{
				var client = DataMother.CreateTestClientWithAddress();
				client.Save();
				DataMother.CreateTestDocumentLog(supplier, client).Save();
			}
			Console.WriteLine(supplier.Id);
		}
	}
}