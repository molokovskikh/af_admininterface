using System;
using System.Collections.Generic;
using System.Linq;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;

namespace Functional
{
	[TestFixture]
	public class PromotionsFixture : WatinFixture2
	{
		private PromotionOwnerSupplier _supplier;
		private Catalog _catalog;
		private SupplierPromotion _promotion;

		[TestFixtureSetUp]
		public void Init()
		{
			session.CreateQuery(@"delete SupplierPromotion").ExecuteUpdate();
		}

		private PromotionOwnerSupplier CreateSupplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			supplier.Name += " " + supplier.Id;
			Save(supplier);
			Flush();
			return PromotionOwnerSupplier.Find(supplier.Id);
		}

		private Catalog FindFirstFreeCatalog()
		{
			DataMother.CreateCatelogProduct();
var catalogId = session.CreateSQLQuery(@"
select
	catalog.Id
from
	catalogs.catalog
	left join usersettings.PromotionCatalogs pc on pc.CatalogId = catalog.Id
	left join usersettings.SupplierPromotions sp on sp.Id = pc.PromotionId
	left join Customers.Suppliers s on s.Id = sp.SupplierId
where
	catalog.Hidden = 0
and s.Id is null
limit 1")
				.UniqueResult<uint>();
			return Catalog.Find(catalogId);
		}

		private SupplierPromotion CreatePromotion(PromotionOwnerSupplier supplier, Catalog catalog)
		{
			var supplierPromotion = new SupplierPromotion
			{
				Enabled = true,
				PromotionOwnerSupplier = supplier,
				Annotation = catalog.Name,
				Name = catalog.Name,
				Begin = DateTime.Now.Date.AddDays(-7),
				End = DateTime.Now.Date,
				Catalogs = new List<Catalog> { catalog }
			};
			supplierPromotion.Save();
			return supplierPromotion;
		}

		private void RefreshPromotion(SupplierPromotion promotion)
		{
			promotion.Refresh();
		}

		[SetUp]
		public void SetUp()
		{
			_supplier = CreateSupplier();
			_catalog = FindFirstFreeCatalog();
			_promotion = CreatePromotion(_supplier, _catalog);

			Open();
			Click("Промо-акции");
			AssertText("Список акций");
		}

		[Test(Description = "проверяем отображение основной страницы с акциями")]
		public void OpenIndexPage()
		{
			Assert.That(browser.Text, Is.StringContaining("Список акций"));
			Assert.That(browser.Text, Is.StringContaining("Введите текст для поиска"));
			Assert.That(browser.Text, Is.StringContaining("Статус:"));
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			Assert.That(list.Exists, Is.True);
			Assert.That(list.SelectedItem, Is.EqualTo("Включенные")); 
		}

		public void RowPromotionNotExists(SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + _promotion.Id);
			Assert.That(row.Exists, Is.False);
		}

		public TableRow RowPromotionExists(SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + _promotion.Id);
			Assert.That(row.Exists, Is.True);
			Assert.That(row.OwnTableCell(Find.ByText(_promotion.PromotionOwnerSupplier.Name)).Exists, Is.True);
			Assert.That(row.OwnTableCell(Find.ByText(_promotion.Name)).Exists, Is.True);
			Assert.That(row.OwnTableCells[1].CheckBoxes.Count, Is.EqualTo(1));
			Assert.That(row.OwnTableCells[1].CheckBoxes[0].Checked, Is.EqualTo(promotion.Enabled));
			Assert.That(row.OwnTableCells[2].CheckBoxes.Count, Is.EqualTo(1));
			Assert.That(row.OwnTableCells[2].CheckBoxes[0].Checked, Is.EqualTo(promotion.AgencyDisabled));
			return row;
		}

		[Test(Description = "проверяем работу фильтра по статусам акций")]
		public void CheckFilterStatus()
		{
			//Только что созданная акция должна быть в списке
			RowPromotionExists(_promotion);

			//ее не должно быть в списке отключенных
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Отключенные");
			browser.Button(Find.ByValue("Показать")).Click();

			RowPromotionNotExists(_promotion);

			//она должна быть в списке "Все"
			list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Все");
			browser.Button(Find.ByValue("Показать")).Click();

			RowPromotionExists(_promotion);
		}

		[Test(Description = "проверяем включение/выключение акций")]
		public void DisabledPromotion()
		{
			//отключаем акцию
			var row = RowPromotionExists(_promotion);
			row.OwnTableCells[1].CheckBoxes[0].Click();

			//проверяем отключение
			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.False);
			Assert.That(_promotion.AgencyDisabled, Is.False);
			RowPromotionNotExists(_promotion);

			//находим ее в списке отключенных
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Отключенные");
			browser.Button(Find.ByValue("Показать")).Click();
			row = RowPromotionExists(_promotion);

			//включаем обратно
			row.OwnTableCells[1].CheckBoxes[0].Click();
			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.True);
			Assert.That(_promotion.AgencyDisabled, Is.False);
			RowPromotionNotExists(_promotion);
		}

		[Test(Description = "проверяем включение/выключение акций административно АК Инфорум")]
		public void AgencyDisabledPromotion()
		{
			//отключаем акцию
			var row = RowPromotionExists(_promotion);
			row.OwnTableCells[2].CheckBoxes[0].Click();

			//проверяем отключение
			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.True);
			Assert.That(_promotion.AgencyDisabled, Is.True);
			RowPromotionNotExists(_promotion);

			//находим ее в списке отключенных
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Отключенные");
			browser.Button(Find.ByValue("Показать")).Click();
			row = RowPromotionExists(_promotion);

			//включаем обратно
			row.OwnTableCells[2].CheckBoxes[0].Click();
			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.True);
			Assert.That(_promotion.AgencyDisabled, Is.False);
			RowPromotionNotExists(_promotion);
		}

		[Test(Description = "удаление акции")]
		public void DeletePromotion()
		{
			var row = RowPromotionExists(_promotion);

			row.Button(Find.ByValue("Удалить")).Click();

			Reopen();
			var deletedPromotion = SupplierPromotion.TryFind(_promotion.Id);
			Assert.That(deletedPromotion, Is.Null);

			RowPromotionNotExists(_promotion);
		}

		[Test(Description = "редактирование акции")]
		public void EditPromotion()
		{
			var row = RowPromotionExists(_promotion);

			row.Link(Find.ByText("Редактировать")).Click();

			Assert.That(browser.Text, Is.StringContaining("Редактирование акции №" + _promotion.Id));
			Assert.That(browser.Text, Is.StringContaining(_promotion.Name));
			Assert.That(browser.Text, Is.StringContaining(_promotion.PromotionOwnerSupplier.Name));

			browser.CheckBox(Find.ByName("promotion.Enabled")).Click();
			browser.CheckBox(Find.ByName("promotion.AgencyDisabled")).Click();
			browser.TextField(Find.ByName("promotion.Annotation")).TypeText("новое крутое описание");

			browser.Button(Find.ByValue("Сохранить")).Click();

			Assert.That(browser.Text, Is.StringContaining("Список акций"));
			RowPromotionNotExists(_promotion);

			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.False);
			Assert.That(_promotion.AgencyDisabled, Is.True);
			Assert.That(_promotion.Annotation, Is.EqualTo("новое крутое описание"));
		}

		[Test(Description = "проверяем создание новой акции")]
		public void CreateNewPromotion()
		{
			var addButton = browser.Button(Find.ByValue("Добавить новую акцию"));
			Assert.That(addButton.Exists, Is.True, "Не найдена кнопка добавления акции");
		}

		[Test(Description = "редактирование списка препартов акции")]
		public void EditPromotionCatalogs()
		{
			var row = RowPromotionExists(_promotion);

			row.Link(Find.ByText("Редактировать")).Click();

			Assert.That(browser.Text, Is.StringContaining("Редактирование акции №" + _promotion.Id));
			Assert.That(browser.Text, Is.StringContaining(_promotion.Name));
			Assert.That(browser.Text, Is.StringContaining(_promotion.PromotionOwnerSupplier.Name));

			//Переходим на форму редактирования списка
			browser.Link(Find.ByText("Редактировать список препаратов")).Click();

			Assert.That(browser.Text, Is.StringContaining("Редактирование списка препаратов акции №" + _promotion.Id));

			//Выбираем наименования и отмечаем их в таблице
			var chaBoxes = browser.CheckBoxes.Where(cb => cb.Name.StartsWith("cha")).ToList();
			Assert.That(chaBoxes.Count, Is.GreaterThan(3), "Не найдено достаточное количество наименований в каталоге");
			if (chaBoxes.Count > 3)
				for (int i = 0; i < 3; i++)
					chaBoxes[i].Click();

			//Производим сохранение
			var addBtn = browser.Button(Find.ById("addBtn"));
			Assert.That(addBtn.Exists, Is.True, "Не найдена кнопка добавления наименований к списку выбранных прератов");
			addBtn.Click();
			Assert.That(browser.Text, Is.StringContaining("Редактирование списка препаратов акции №" + _promotion.Id));
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			RefreshPromotion(_promotion);
			var promoBoxes = browser.CheckBoxes.Where(cb => cb.Name.StartsWith("chd")).ToList();
			Assert.That(promoBoxes.Count, Is.EqualTo(_promotion.Catalogs.Count), "Не совпадает количество доступных для удаления препаратов со списом препаратов акции");
			foreach (var catalog in _promotion.Catalogs)
				Assert.That(promoBoxes.Exists(box => box.Name == "chd" + catalog.Id), Is.True, "Не найден checkbox для удаления препарата с Id:{0} Name:{1}", catalog.Id, catalog.Name);

			var parentPromo = browser.Link(Find.ByText("Редактирование акции"));
			Assert.That(parentPromo.Exists, "Не найдена ссылка на родительскую промо-акцию");
			parentPromo.Click();

			Assert.That(browser.Text, Is.StringContaining("Редактирование акции №" + _promotion.Id));
			Assert.That(browser.Text, Is.Not.StringContaining("Сохранено"));
		}
	}
}