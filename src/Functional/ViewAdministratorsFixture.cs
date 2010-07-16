using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class ViewAdministratorsFixture : WatinFixture
	{
		[Test, Ignore("Неактуален. Тесты для региональных администраторов находятся в RegionalAdminFixture")]
		public void SetupAdministrator()
		{
			using (var browser = Open("ViewAdministrators.aspx"))
			{
				Assert.That(browser.Text, Is.StringContaining("Пользователи офиса"));
				browser.Link(Find.ByText("kvasov")).Click();
				Assert.That(browser.Text, Is.StringContaining("Редактирование регионального администратора"));
				var checkBox = browser.CheckBox(Find.ById("ctl00_MainContentPlaceHolder_PermissionSelector_1"));
				var result = !checkBox.Checked;
				browser.CheckBox(Find.ById("ctl00_MainContentPlaceHolder_PermissionSelector_1")).Checked = result;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователи офиса"));
				Assert.That(browser.CheckBox(Find.ById("ctl00_MainContentPlaceHolder_Administrators_ctl11_CheckBox15")).Checked,
					Is.EqualTo(result));
			}
		}
	}
}
