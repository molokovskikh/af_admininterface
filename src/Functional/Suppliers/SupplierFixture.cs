using System;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Suppliers
{
	public class SupplierFixture : WatinFixture2
	{
		private User user;
		private Supplier supplier;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
			scope.Flush();
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText(user.Id.ToString());
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining(user.Login));
/*
 *			срабатывает автоматичский вход в пользователя
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining("Поставщик"));
*/
		}

		[Test]
		public void Change_user_permissions()
		{
			Open(user, "Edit");
			Assert.That(browser.Text, Is.StringContaining("Настройка"));
			Click("Настройка");
			Assert.That(browser.Text, Is.StringContaining("Настройки пользователя"));
			var permission = GetPermission("Управлять заказами");
			Assert.That(permission.Checked, Is.True);
			permission.Click();
			Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Click("Настройка");
			permission = GetPermission("Управлять заказами");
			Assert.That(permission.Checked, Is.False);
		}

		private CheckBox GetPermission(string name)
		{
			var permission = (CheckBox)browser.Label(Find.ByText(name)).PreviousSibling;
			return permission;
		}

		[Test]
		public void Search_supplier()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText("Тестовый поставщик");
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining("Тестовый поставщик"));

			browser.Link(Find.ByText("Тестовый поставщик")).Click();
			Assert.That(browser.Text, Is.StringContaining("Поставщик"));
		}

		[Test]
		public void Update_supplier_name()
		{
			Open(supplier);
			browser.Css("#supplier_Name").TypeText("Тестовый поставщик обновленный");
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
		}

		[Test]
		public void Add_user()
		{
			Open(supplier);
			browser.Click("Новый пользователь");
			Assert.That(browser.Text, Is.StringContaining("Новый пользователь"));
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
		}

		[Test]
		public void Register()
		{
			Open();
			browser.Click("Поставщик");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));

			Prepare();

			browser.Click("Зарегистрировать");
			Assert.That(browser.Text, Is.StringContaining("Регистрация плательщика"));
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Поставщик тестовый"));
		}

		[Test]
		public void Register_supplier_show_user_card()
		{
			Open("Register/RegisterSupplier");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));
			Prepare();

			browser.Css("#FillBillingInfo").Click();
			browser.Click("Зарегистрировать");

			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
		}

		private void Prepare()
		{
			Css("#JuridicalName").TypeText("тестовый поставщик");
			Css("#ShortName").TypeText("тестовый");
			Css("#ClientContactPhone").TypeText("473-2606000");
			Css("#ClientContactEmail").TypeText("kvasovtest@analit.net");
		}
	}
}