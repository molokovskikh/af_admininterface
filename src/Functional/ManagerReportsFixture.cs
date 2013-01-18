using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.Suppliers;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class ManagerReportsFixture : WatinFixture2
	{
		[Test]
		public void BaseShowTest()
		{
			Open();
			Click("Отчеты менеджеров");
			AssertText("Отчеты для менеджеров");
			Click("Зарегистрированные пользователи и адреса");
			AssertText("Зарегистрированные пользователи и адреса в регионе");
			Click("Показать");
			AssertText("Зарегистрированные пользователи и адреса в регионе");
			browser.SelectList(Find.ByName("filter.FinderType")).SelectByValue(((int)RegistrationFinderType.Addresses).ToString());
			Click("Показать");
			AssertText("Зарегистрированные пользователи и адреса в регионе");
			Open();
			Click("Отчеты менеджеров");
			AssertText("Отчеты для менеджеров");
			Click("Клиенты и адреса, по которым не принимаются накладные");
			AssertText("Клиенты и адреса в регионе, по которым не принимаются накладные");
		}

		[Test]
		public void Switch_off_show_test()
		{
			Open();
			Click("Отчеты менеджеров");
			Click("Список отключенных клиентов");
			AssertText("Список отключенных клиентов");
			Click("Показать");
			AssertText("Список отключенных клиентов");
		}

		[Test]
		public void Analysis_of_work_drugstores()
		{
			Open("ManagerReports");
			Click("Сравнительный анализ работы аптек");
			AssertText("Сравнительный анализ работы аптек");
		}

		[Test]
		public void NoParsedWaybills()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var client = DataMother.CreateClientAndUsers();
			Save(client);
			var documentLog = new AdminInterface.Models.Logs.DocumentReceiveLog(supplier);
			documentLog.ForClient = client;
			documentLog.LogTime = DateTime.Now;
			Save(documentLog);

			Open("ManagerReports");
			Click("Неразобранные накладные");
			AssertText("Неразобранные накладные");
			AssertText("Регион:");
			Assert.That(browser.Text, Is.Not.Contains("Только неразобранные накладные"));
			Click("Показать");
			AssertText("Номер документа");
			Assert.That(browser.Text, Is.Not.Contains("Дата документа"));
			Assert.That(browser.Text, Is.Not.Contains("Дата отправки"));
			AssertText(client.Name);
			AssertText(supplier.Name);
		}

		[Test]
		public void NoParsedWaybillsReport()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var client = DataMother.CreateClientAndUsers();
			Save(client);
			var documentLog = new AdminInterface.Models.Logs.DocumentReceiveLog(supplier);
			documentLog.ForClient = client;
			documentLog.LogTime = DateTime.Now;
			Save(documentLog);

			Open("ManagerReports");
			Click("Отчет о состоянии неформализованных накладных по поставщикам");
			AssertText("Отчет о состоянии неформализованных накладных по поставщикам");
			AssertText("Кол-во нераспознанных накладных");
			AssertText(supplier.Id.ToString());
			AssertText(supplier.Name);
		}

		[Test]
		public void ParsedWaybillsReportTest()
		{
			Open("ManagerReports");
			Click("Отчет о состоянии формализованных накладных по поставщикам");
			AssertText("Отчет о состоянии формализованных накладных");
		}

		[Test]
		public void ClientAddressMonitorTest()
		{
			Open("ManagerReports");
			Click("Клиенты и адреса, по которым не принимаются накладные");
			AssertText("Клиенты и адреса в регионе, по которым не принимаются накладные");
		}

		[Test]
		public void ClientConditionMonitoring()
		{
			Open("ManagerReports");
			Click("Мониторинг выставления условий клиенту");
			AssertText("Мониторинг выставления условий клиенту");
			Click("Показать");
			AssertText("Мониторинг выставления условий клиенту");
		}

		[Test]
		public void AnalisOfWorkTest()
		{
			var client = session.Query<Client>().First();
			var user = client.Users.First();
			var address = client.Addresses.First();
			user.AvaliableAddresses.Add(address);
			session.Save(user);
			Open("ManagerReports");
			Click("Сравнительный анализ работы аптек");
			Click("Показать");
			Click("Код");
			browser.Link(client.Id.ToString()).Click();
			AssertText(string.Format("Клиент: {0}", client.Name));
			AssertText(user.Id.ToString());
			AssertText(address.Name);
		}

		[Test]
		public void UserAndAddresseTest()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			Open("ManagerReports");
			Click("Зарегистрированные пользователи и адреса");
			browser.Link(client.Id.ToString()).Click();
			AssertText(string.Format("Клиент: {0}", client.Name));
			foreach (var user in client.Users) {
				AssertText(string.Format("{0} - ({1})", user.Name, user.Id));
			}
		}

		[Test]
		public void FormPositionFixture()
		{
			var supplier = TestSupplier.Create();
			var price = supplier.Prices[0];
			var itemFormat = price.Costs[0].PriceItem.Format;
			itemFormat.PriceFormat = PriceFormatType.NativeDbf;
			itemFormat.FCode = "F1";
			session.Save(itemFormat);
			session.Flush();
			session.Save(price);
			Open("ManagerReports");
			Click("Отчет о состоянии формализуемых полей в прайс-листах поставщиков");
			AssertText("Отчет о состоянии формализуемых полей в прайс-листах поставщиков");
			AssertText("Выгрузить в Excel");
			Click("Показать");
			AssertText(price.Id.ToString());
		}
	}
}