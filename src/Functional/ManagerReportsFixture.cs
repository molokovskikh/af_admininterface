using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using Functional.ForTesting;
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
			Click("Пользователи и адреса");
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
	}
}