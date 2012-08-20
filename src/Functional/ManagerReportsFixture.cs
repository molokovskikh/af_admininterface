using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
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
		}
	}
}