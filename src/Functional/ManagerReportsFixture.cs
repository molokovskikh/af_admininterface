using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;

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
		public void ClientAddressMonitorTest()
		{
			Open("ManagerReports");
			Click("Клиенты и адреса, по которым не принимаются накладные");
			AssertText("Клиенты и адреса в регионе, по которым не принимаются накладные");
		}
	}
}