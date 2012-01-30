﻿using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
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
		private Payer  payer;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
			payer = DataMother.CreatePayer();
			payer.Save();
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			browser.Css("#filter_SearchText").TypeText(user.Id.ToString());
			Click("Поиск");
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
			browser.Css("#filter_SearchText").TypeText("Тестовый поставщик");
			Click("Поиск");
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
			Assert.That(browser.Css("#supplier_Name").Text, Is.EqualTo("Тестовый поставщик обновленный"));
			ActiveRecordMediator.Refresh(supplier);
			Assert.That(supplier.Name, Is.EqualTo("Тестовый поставщик обновленный"));
		}

		[Test, Ignore("Не реализовано")]
		public void Add_user()
		{
			Open(supplier);
			browser.Click("Новый пользователь");
			Assert.That(browser.Text, Is.StringContaining("Новый пользователь"));
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
		}

		[Test]
		public void Change_Payer()
		{
			Open(supplier);
			browser.TextField(Find.ByClass("term")).AppendText("Тестовый");
			browser.Button(Find.ByClass("search")).Click();
			var selectList = browser.Div(Find.ByClass("search")).SelectLists.First();
			Assert.IsNotNull(selectList);
			Assert.That(selectList.Options.Count, Is.GreaterThan(0));
			selectList.SelectByValue(payer.Id.ToString());
			Click("Изменить");
			AssertText("Изменено");
		}
	}
}