using System;
using AdminInterface.Model;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
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
			var documentLog = new DocumentLogEntity
			{
				DocumentType = DocumentType.Waybill,
				FileName = "test.txt",
				LogTime = DateTime.Now,
				ForClient = Client.Find(2575u),
				FromSupplier = Client.Find(5u),
			};
			documentLog.Save();

			using (var browser = new IE(BuildTestUrl("Client/2575")))
			{
				browser.Link(Find.ByText("Статистика документов")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(@"Статистика получения\отправки документов клиента ТестерК")))
				{
					Assert.That(openedWindow.Text, Text.Contains(documentLog.Id.ToString()));
				}
			}
		}
	}
}