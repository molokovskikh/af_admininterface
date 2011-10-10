using System;
using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Drugstore
{
	[TestFixture]
	public class DocumentLogFixture : WatinFixture2
	{
		[Test]
		public void View_documents()
		{
			var client = DataMother.CreateTestClientWithAddress();
			var supplier = DataMother.CreateSupplier();
			var document = new DocumentReceiveLog(supplier) {
				FileName = "test.txt",
				ForClient = client,
				Address = client.Addresses.First(),
			};
			client.Save();
			supplier.Save();
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
	}
}