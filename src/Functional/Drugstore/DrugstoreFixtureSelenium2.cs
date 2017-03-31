using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;
using Document = Common.Web.Ui.Models.Document;
using DocumentType = AdminInterface.Models.Logs.DocumentType;

namespace Functional.Drugstore
{
	[TestFixture]
	public class DrugstoreFixtureSelenium2 : AdmSeleniumFixture
	{
		private Client testClient;
		private Supplier testSupplier;

		protected void CloseAllTabsButLast()
		{
			var allTabsToClose = GlobalDriver.WindowHandles.ToList();
			var lastName = allTabsToClose[allTabsToClose.Count - 1];
			if (allTabsToClose.Count > 1)
				for (int i = 0; i < allTabsToClose.Count; i++) {
					if (lastName != allTabsToClose[i]) {
						GlobalDriver.SwitchTo().Window(allTabsToClose[i]);
						GlobalDriver.Close();
					}
				}
			GlobalDriver.SwitchTo().Window(lastName);
		}

		[SetUp]
		public void Setup()
		{
			testClient = DataMother.CreateTestClientWithAddressAndUser();

			var user = testClient.Users.First();
			user.AvaliableAddresses.Add(testClient.Addresses.First());
			session.Save(user);
			testSupplier = DataMother.CreateSupplier();
			session.Save(testSupplier);
			var order = new ClientOrder(testClient.Users.First(), testSupplier.Prices[0]);
			session.Save(order);

			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			var line = new OrderLine(order, product, 100, 1);
			session.Save(product);
			session.Save(line);

			session.Save(order);
			testSupplier = DataMother.CreateSupplier();
			var documentLogEntity = DataMother.CreateTestDocumentLog(testSupplier, testClient);
			var updateLogEntity = DataMother.CreateTestUpdateLogEntity(testClient);
			documentLogEntity.DocumentType = DocumentType.Waybill;
			session.SaveOrUpdate(updateLogEntity);
			documentLogEntity.SendUpdateLogEntity = updateLogEntity;
			session.Save(documentLogEntity);

			var document = DataMother.CreateTestDocument(testSupplier, testClient, documentLogEntity);
			session.Save(document);
			session.Flush();

			Open(testClient);
			AssertText("Клиент");
		}

		[Test]
		public void Try_to_view_WaybillStatistics()
		{
			Click("Статистика накладных");
			CloseAllTabsButLast();
			WaitForCss(".calendar");
			AssertText("Статистика накладных");
			AssertText(testClient.Name);
			AssertText(testSupplier.Name);
		}
	}
}