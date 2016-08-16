using System;
using System.Linq;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Functional
{
	[TestFixture]
	public class ManagerReportsFixture : AdmSeleniumFixture
	{
		[Test]
		public void BaseShowTest()
		{
			Open();
			Click("Отчеты менеджеров");
			AssertText("Отчеты для менеджеров");
			Click("зарегистрированные пользователи и адреса");
			AssertText("зарегистрированные пользователи и адреса в регионе");
			Click("Показать");
			AssertText("зарегистрированные пользователи и адреса в регионе");
			Css("[name='filter.FinderType']").SelectByValue(((int)RegistrationFinderType.Addresses).ToString());
			Click("Показать");
			AssertText("зарегистрированные пользователи и адреса в регионе");
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
			session.Save(supplier);
			var client = DataMother.CreateClientAndUsers();
			session.Save(client);
			var documentLog = new DocumentReceiveLog(supplier);
			documentLog.ForClient = client;
			documentLog.LogTime = DateTime.Now;
			session.Save(documentLog);

			Open("ManagerReports");
			Click("Неразобранные накладные");
			AssertText("Неразобранные накладные");
			AssertText("Регион:");
			AssertNoText("Только неразобранные накладные");
			Click("Показать");
			AssertText("Номер документа");
			AssertNoText("Дата документа");
			AssertNoText("Дата отправки");
			AssertText(client.Name);
			AssertText(supplier.Name);
		}

		[Test]
		public void NoParsedWaybillsReport()
		{
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			var client = DataMother.CreateClientAndUsers();
			session.Save(client);
			var documentLog = new DocumentReceiveLog(supplier);
			documentLog.ForClient = client;
			session.Save(documentLog);

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
			var client = DataMother.CreateTestClientWithAddressAndUser();
			session.Save(client);
			var user = client.Users.First();
			var address = client.Addresses.First();
			user.AvaliableAddresses.Add(address);
			session.Save(user);
			session.Save(address);
			Open("ManagerReports");
			Click("Сравнительный анализ работы аптек");
			Css("#filter_Regions").SelectByValue(client.HomeRegion.Id.ToString());
			Click("Показать");
			Click("Код");
			Click("Код");
			browser.FindElementById(client.Id.ToString()).Click();
			AssertText(string.Format("Клиент: {0}", client.Name));
			AssertText(user.Id.ToString());
			AssertText(address.Name);
		}

		[Test]
		public void UserAndAddresseTest()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			Open("ManagerReports");
			Click("зарегистрированные пользователи и адреса");
			browser.FindElementById(client.Id.ToString()).Click();
			AssertText(string.Format("Клиент: {0}", client.Name));
			foreach (var user in client.Users) {
				AssertText(string.Format("{0} - ({1})", user.Name, user.Id));
			}
		}

		[Test]
		public void FormPositionFixture()
		{
			var supplier = DataMother.CreateSupplier();
			var price = supplier.Prices[0];
			var item = price.Costs[0].PriceItem;
			item.FormRule.FCode = "F1";
			item.FormRule.Format = session.Query<PriceFormat>().First(f => f.FileExtention == ".dbf");
			session.Save(supplier);

			Open("ManagerReports");
			Click("Отчет о состоянии формализуемых полей в прайс-листах поставщиков");
			AssertText("Отчет о состоянии формализуемых полей в прайс-листах поставщиков");
			AssertText("Выгрузить в Excel");
			Click("Показать");
			AssertText(price.Id.ToString());
		}
	}
}