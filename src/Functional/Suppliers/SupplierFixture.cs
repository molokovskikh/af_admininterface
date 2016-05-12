using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using Test.Support;
using WatiN.Core.Native.Windows;
using ContactGroupType = Common.Web.Ui.Models.ContactGroupType;
using PriceType = AdminInterface.Models.Suppliers.PriceType;

namespace Functional.Suppliers
{
	public class SupplierFixture : FunctionalFixture
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
			MakeNameUniq(payer);
		}

		[Test]
		public void Make_orders_test()
		{
			Open(supplier);
			Click("Сформировать заказы");
			Css("#email").Clear();
			Css("#email").AppendText("kvasovtest@analit.net");
			Click("Получить файлы");
			AssertText("Вы не указали номера заказов");
			Css("#ordersNames").AppendText("0000");
			Click("Получить файлы");
			AssertText("Не удалось получить все файлы от сервиса, некоторые заказы не были сформированы, проверьте почту");
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
			WaitForText("Тестовый поставщик");

			ClickLink("Тестовый поставщик");
			WaitForText("Поставщик");
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

		[Test]
		public void Change_Payer()
		{
			Open(supplier);
			var selectList = SearchV2Root(Css("#ChangePayer"), payer.Name);
			Assert.That(selectList.Options.Count, Is.GreaterThan(0));
			var option = selectList.Options.First();
			selectList.SelectByValue(option.Value);
			Click("Изменить");
			AssertText("Изменено");
		}

		[Test]
		public void Vip_Price()
		{
			var client = DataMother.CreateTestClientWithUser(supplier.PricesRegions.First());
			session.Save(client);

			//Тут нужно понимать, что предупреждалка после обновления страницы, появляется только в том, случае
			//Если клиенты действительно были отключены от вип прайса, что случается не всегда
			//Поэтому необходимо добавить интерсекции
			var intersection = new Intersection();
			intersection.Price = supplier.Prices.First();
			intersection.Client = client;
			intersection.Region = supplier.PricesRegions.First();
			var payer = new Payer("dasdas", "dasdasda");
			session.Save(payer);
			var legal = new LegalEntity("DASDA", "DASD", payer);
			session.Save(legal);
			intersection.Org = legal;
			session.Save(intersection);

			Open(supplier);
			Click("Настройка");
			Css("#MainContentPlaceHolder_PricesGrid_PriceTypeList_0").SelectByValue(((int)PriceType.Vip).ToString());
			Click("Применить");
			AssertText("Все клиенты были отключены от VIP прайсов");
		}

		[Test]
		public void Set_sertificate_source()
		{
			var newCertificate = new CertificateSource {
				Name = "Test_Source",
				SourceClassName = "Test_class_Name"
			};
			session.Save(newCertificate);

			Open(supplier);
			Css("#editChangerButton").Click();
			Css("select[name='sertificateSourceId']").SelectByValue(newCertificate.Id.ToString());
			Css("#saveCertificateSourceButton").Click();
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
			Css("#MainContentPlaceHolder_PricesGrid_AddButton").Click();
			Css("#MainContentPlaceHolder_PricesGrid_PriceTypeList_1").SelectByValue(((int)AdminInterface.Models.Suppliers.PriceType.Assortment).ToString());
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
			Css("#addNewExcludeFileInput").AppendText("1234");
			Click("Добавить");
			AssertText("Сохранено");
		}

		[Test]
		public void Edit_waybill_exlude_file()
		{
			Open("Suppliers/WaybillExcludeFiles?supplierId=" + supplier.Id);
			Css("#addNewExcludeFileInput").AppendText("1234");
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

			Open("Suppliers/WaybillSourceSettings?supplierId=" + supplier.Id);
			AssertText("Настройка данных для доступа к FTP");
			Css("#source_WaybillUrl").AppendText("testUrl");
			Css("#source_RejectUrl").AppendText("testUrl");
			Css("#userName").AppendText("testUser");
			Css("#source_Password").AppendText("testPassword");
			Css("#source_downloadInterval").AppendText("5");
			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void Delete_waybill_exclude_file()
		{
			Open("Suppliers/WaybillExcludeFiles?supplierId=" + supplier.Id);
			Css("#addNewExcludeFileInput").AppendText("1234");
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

		[Test]
		public void Add_region()
		{
			var regions = session.Query<Region>().ToList();
			var region = regions
				.Except(regions.Where(r => (r.Id & supplier.RegionMask) > 0).Concat(new[] { supplier.HomeRegion }))
				.First();
			Open("managep.aspx?cc={0}", supplier.Id);
			Click("Подключить регион");
			Css("#edit_Region_Id").SelectByValue(region.Id.ToString());
			Css("#edit_PermitedBy").AppendText("Тест Т.Т.");
			Css("#edit_RequestedBy").AppendText("Тест 1 Т.Т.");
			Css("#addContactLink").Click();
			Css("input.email").AppendText("test@analit.net");
			Click("Добавить");
			AssertText("Регион добавлен");

			session.Refresh(supplier);
			Assert.That(supplier.RegionMask & region.Id, Is.GreaterThan(0));
			Assert.AreEqual("test@analit.net", supplier.ContactGroupOwner.Group(ContactGroupType.General).Persons[0].Contacts[0].ContactText);
		}

		[Test]
		public void Disable_region()
		{
			var regions = session.Query<Region>().ToList();
			var region = regions
				.Except(regions.Where(r => (r.Id & supplier.RegionMask) > 0).Concat(new[] { supplier.HomeRegion }))
				.First();
			supplier.AddRegion(region, session);

			Open("managep.aspx?cc={0}", supplier.Id);
			var table = (Table)Css("#MainContentPlaceHolder_Regions");
			var row = table.FindRow(region.Name, 1);
			Click(row, "Отключить");
			table = (Table)Css("#MainContentPlaceHolder_Regions");
			Assert.That(table.Text, Is.Not.Contains(region.Name));

			session.Refresh(supplier);
			Assert.AreEqual(0, supplier.RegionMask & region.Id);
		}
	}
}