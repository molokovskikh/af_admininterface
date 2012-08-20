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
			browser.Css("#supplier_Name").TypeText("Тестовый_поставщик_обновленный");
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.That(browser.Css("#supplier_Name").Text, Is.EqualTo("Тестовый_поставщик_обновленный"));
			session.Refresh(supplier);
			Assert.That(supplier.Name, Is.EqualTo("Тестовый_поставщик_обновленный"));
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
			newCertificate.Save();
			Flush();
			browser.Button("editChangerButton").Click();
			browser.SelectList(Find.ByName("sertificateSourceId")).SelectByValue(newCertificate.Id.ToString());
			browser.Button("saveCertificateSourceButton").Click();
			AssertText("Сохранено");
			session.Refresh(supplier);
			Assert.That(supplier.GetSertificateSource().Name, Is.StringContaining("Test_Source"));
			newCertificate.Delete();
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
	}
}