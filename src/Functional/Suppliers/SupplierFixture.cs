using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using Test.Support;
using WatiN.Core.Native.Windows;
using WaybillSourceType = AdminInterface.Models.WaybillSourceType;

namespace Functional.Suppliers
{
	public class SupplierFixture : WatinFixture2
	{
		private User user;
		private Supplier supplier;
		private Payer payer;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
			payer = DataMother.CreatePayer();
			session.Save(payer);
		}

		[Test]
		public void Make_orders_test()
		{
			Open(supplier);
			Click("Сформировать заказы");
			browser.TextField("email").Clear();
			browser.TextField("email").AppendText("kvasovtest@analit.net");
			Click("Получить файлы");
			AssertText("Вы не указали номера заказов");
			browser.TextField("ordersNames").AppendText("0000");
			Click("Получить файлы");
			AssertText("Не удалось получить все файлы от сервиса, некоторые заказы не были сформированы, проверте почту");
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			Css("#filter_SearchText").TypeText(user.Id.ToString());
			Click("Поиск");
			AssertText(user.Login);
/*
 *			срабатывает автоматичский вход в пользователя
			ClickLink(user.Login);
			AssertText("Поставщик");
*/
		}

		[Test]
		public void Change_user_permissions()
		{
			Open(user, "Edit");
			AssertText("Настройка");
			Click("Настройка");
			AssertText("Настройки пользователя");
			var permission = GetPermission("Статистика заказов");
			Assert.That(permission.Checked, Is.True);
			permission.Click();
			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");
			permission = GetPermission("Статистика заказов");
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
			Css("#filter_SearchText").TypeText("Тестовый поставщик");
			Click("Поиск");
			AssertText("Тестовый поставщик");

			ClickLink("Тестовый поставщик");
			AssertText("Поставщик");
		}

		[Test]
		public void Update_supplier_name()
		{
			Open(supplier);
			Css("#supplier_Name").TypeText("Тестовый_поставщик_обновленный");
			browser.Click("Сохранить");
			AssertText("Сохранено");
			Assert.That(Css("#supplier_Name").Text, Is.EqualTo("Тестовый_поставщик_обновленный"));
			session.Refresh(supplier);
			Assert.That(supplier.Name, Is.EqualTo("Тестовый_поставщик_обновленный"));
		}

		[Test, Ignore("Не реализовано")]
		public void Add_user()
		{
			Open(supplier);
			browser.Click("Новый пользователь");
			AssertText("Новый пользователь");
			browser.Click("Сохранить");
			AssertText("Сохранено");
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
			var option = selectList.Options.First();
			selectList.SelectByValue(option.Value);
			Click("Изменить");
			AssertText("Изменено");
		}

		[Test]
		public void Vip_Price()
		{
			Open(supplier);
			Click("Настройка");
			browser.SelectList("MainContentPlaceHolder_PricesGrid_PriceTypeList_0").SelectByValue(((int)AdminInterface.Models.Suppliers.PriceType.Vip).ToString());
			Click("Применить");
			AssertText("Все клиенты были отключены от VIP прайсов");
		}


		[Test]
		public void Set_sertificate_source()
		{
			Open(supplier);
			Thread.Sleep(3000);
			var newCertificate = new CertificateSource {
				Name = "Test_Source",
				SourceClassName = "Test_class_Name"
			};
			session.Save(newCertificate);
			Flush();
			browser.Button("editChangerButton").Click();
			browser.SelectList(Find.ByName("sertificateSourceId")).SelectByValue(newCertificate.Id.ToString());
			browser.Button("saveCertificateSourceButton").Click();
			AssertText("Сохранено");
			session.Refresh(supplier);
			Assert.That(supplier.GetSertificateSource().Name, Is.StringContaining("Test_Source"));
			session.Delete(newCertificate);
		}

		[Test]
		public void Change_pricelist_type_to_assortment()
		{
			Open(supplier);
			Click("Настройка");
			//создаем ассортиментный прайс
			browser.Button("MainContentPlaceHolder_PricesGrid_AddButton").Click();
			browser.SelectList("MainContentPlaceHolder_PricesGrid_PriceTypeList_1").SelectByValue(((int)AdminInterface.Models.Suppliers.PriceType.Assortment).ToString());
			Click("Применить");
			session.Refresh(supplier);
			var source = session.CreateSQLQuery(@"
Select fs.RequestInterval 
From farm.Sources fs
	Join PriceItems pi on pi.SourceId = fs.Id
	Join PricesCosts pc on pc.PriceItemId = pi.Id
Where pc.PriceCode = :PriceId1")
				.SetParameter("PriceId1", supplier.Prices[1].Id)
				.ToList<TestSource>()
				.First();
			//проверяем RequestInterval
			Assert.That(source.RequestInterval, Is.EqualTo(86400));
		}

		[Test]
		public void Waybill_exclude_file_base_test()
		{
			Open(supplier);
			Click("Настройка");
			Click("Файлы, исключенные из разбора в качестве накладных");
			Click("Добавить");
			AssertText("Это поле необходимо заполнить.");
			browser.TextField("addNewExcludeFileInput").AppendText("1234");
			Click("Добавить");
			AssertText("Сохранено");
		}

		[Test]
		public void Edit_waybill_exlude_file()
		{
			Open("Suppliers/WaybillExcludeFiles?supplierId=" + supplier.Id);
			browser.TextField("addNewExcludeFileInput").AppendText("1234");
			Click("Добавить");
			browser.TextField(Find.ByClass("excludeFileMask")).Clear();
			browser.TextField(Find.ByClass("excludeFileMask")).AppendText("0000");
			Click("Сохранить");
			Refresh();
			Assert.AreEqual(browser.TextField(Find.ByClass("excludeFileMask")).Text, "0000");
		}

		[Test]
		public void Waybill_Source_Settings_Test()
		{
			var waybillSource = new WaybillSource { SourceType = WaybillSourceType.FtpSupplier, ReaderClassName = "testReaderClass", Supplier = supplier };
			session.Save(waybillSource);
			Flush();
			Open("Suppliers/WaybillSourceSettings?supplierId=" + supplier.Id);
			AssertText("Настройка данных для доступа к FTP");
			browser.TextField("source_WaybillUrl").AppendText("testUrl");
			browser.TextField("source_RejectUrl").AppendText("testUrl");
			browser.TextField("userName").AppendText("testUser");
			browser.TextField("source_Password").AppendText("testPassword");
			browser.TextField("source_downloadInterval").AppendText("5");
			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void Delete_waybill_exclude_file()
		{
			Open("Suppliers/WaybillExcludeFiles?supplierId=" + supplier.Id);
			browser.TextField("addNewExcludeFileInput").AppendText("1234");
			Click("Добавить");
			Click("Удалить");
			AssertText("Вы уверены, что хотите удалить маску \"1234\" ?");
		}

		[Test]
		public void SetAllWorkRegionTest()
		{
			Open(user);
			browser.TextField(Find.ByName("user.Name")).Value = "Тестовый";
			Click("Настройка");
			browser.CheckBox(Find.ByName("WorkRegions[1]")).Checked = true;
			Click("Сохранить");
			AssertText("$$$Изменено 'Регионы работы' Удалено 'Все регионы' Добавлено 'Воронеж'");
			Click("Настройка");
			browser.CheckBox(Find.ByName("WorkRegions[0]")).Checked = true;
			Click("Сохранить");
			AssertText("$$$Изменено 'Регионы работы' Удалено 'Воронеж' Добавлено 'Все регионы'");
		}
	}
}