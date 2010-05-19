using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class DocumentLogFixture : WatinFixture
	{
		[Test]
		public void View_documents()
		{
			var client = CreateClientWithDeliveryAddress();
			var documentLog = new DocumentReceiveLog {
				DocumentType = DocumentType.Waybill,
				FileName = "test.txt",
				LogTime = DateTime.Now,
				ForClient = client,
				Address = client.Addresses.First(),
				FromSupplier = Supplier.Find(5u),
			};
			documentLog.Save();

			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("История документов")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format(@"Статистика получения\отправки документов клиента {0}", client.Name))))
				{
					Assert.That(openedWindow.Text, Is.StringContaining(documentLog.Id.ToString()));
					Assert.That(openedWindow.Text, Is.StringContaining("тестовый адрес доставки"));
				}
			}
		}

		public Client CreateClientWithDeliveryAddress()
		{
			var client = DataMother.CreateTestClientWithUser();
			var address = new Address {
				Client = client,
				Value = "тестовый адрес доставки",
			};
			address.Save();
			client.Addresses = new List<Address> {address};
			return client;
		}
	}
}