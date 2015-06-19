using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration.Queries
{
	[TestFixture]
	public class ParsedWaybillsFilterFixture : AdmIntegrationFixture
	{
		private Supplier supplier;
		[SetUp]
		public void SetUp()
		{
			supplier = DataMother.CreateSupplier();
			Save(supplier);
		}
		[Test]
		public void FindWaybillsTest()
		{
			var client = DataMother.CreateTestClientWithAddress();
			var documentLog = DataMother.CreateTestDocumentLog(supplier, client);
			var document = DataMother.CreateTestDocument(supplier, client, documentLog);
			document.WriteTime = DateTime.Now.AddDays(-1);
			document.Lines[0].SerialNumber = "123";
			Save(document);
			Flush();
			var filter = new ParsedWaybillsFilter { Session = session, Period = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now) };
			var documentsInfo = filter.Find();
			var testDocument = documentsInfo.FirstOrDefault(d => ((ParsedWaybillsItem)d).SupplierCode == supplier.Id);
			Assert.That(testDocument, Is.Not.Null);
			Assert.That(((ParsedWaybillsItem)testDocument).SupplierCode, Is.EqualTo(supplier.Id));
			Assert.That(((ParsedWaybillsItem)testDocument).SerialNumber, Is.EqualTo("*"));
		}

		[Test(Description = "Проверяет корректную работу фильтра по клиенту")]
		public void FindWaybillsWithClientTest()
		{
			// Создаем двух клиентов и документы для них
			var client = DataMother.CreateTestClientWithAddress();
			var documentLog = DataMother.CreateTestDocumentLog(supplier, client);
			var document = DataMother.CreateTestDocument(supplier, client, documentLog);
			document.WriteTime = DateTime.Now.AddDays(-1);
			document.Lines[0].SerialNumber = "123";
			var client1 = DataMother.CreateTestClientWithAddress();
			var documentLog1 = DataMother.CreateTestDocumentLog(supplier, client1);
			var document1 = DataMother.CreateTestDocument(supplier, client1, documentLog1);
			document1.WriteTime = DateTime.Now.AddDays(-1);
			document1.Lines[0].Product = "123";
			Save(document);
			Save(document1);
			Flush();
			// Ищем накладные только для первого клиента
			var filter = new ParsedWaybillsFilter { Session = session,
				Period = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now),
				ClientId = client.Id,
				ClientName = client.Name
			};
			var documentsInfo = filter.Find();
			var testDocument = documentsInfo.FirstOrDefault(d => ((ParsedWaybillsItem)d).SupplierCode == supplier.Id);
			Assert.That(testDocument, Is.Not.Null);
			Assert.That(((ParsedWaybillsItem)testDocument).SupplierCode, Is.EqualTo(supplier.Id));
			Assert.That(((ParsedWaybillsItem)testDocument).SerialNumber, Is.EqualTo("*"));
			Assert.That(((ParsedWaybillsItem)testDocument).Product, Is.Null);
		}
	}
}
