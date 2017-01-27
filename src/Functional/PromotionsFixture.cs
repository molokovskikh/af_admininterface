using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class PromotionsFixture : FunctionalFixture
	{
		private PromotionOwnerSupplier _supplier;
		private Catalog _catalog;
		private SupplierPromotion _promotion;
		private SupplierPromotion _promotionWithFile;
		private SupplierPromotion _promotionNotModerated;

		private PromotionOwnerSupplier CreateSupplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			supplier.Name += " " + supplier.Id;
			Save(supplier);
			Flush();
			return session.Load<PromotionOwnerSupplier>(supplier.Id);
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
			return session.Load<Catalog>(catalogId);
		}

		private SupplierPromotion CreatePromotion(PromotionOwnerSupplier supplier, Catalog catalog, bool moderated = true, bool withFile = false)
		{
			var supplierPromotion = new SupplierPromotion {
				Enabled = true,
				PromotionOwnerSupplier = supplier,
				Annotation = catalog.Name,
				Name = catalog.Name,
				Begin = DateTime.Now.Date.AddDays(-7),
				End = DateTime.Now.Date.AddDays(1),
				Moderated = moderated,
				Catalogs = new List<Catalog> { catalog }
			};
			if (withFile)
			supplierPromotion.PromoFile =  "testName.jpg";
			supplierPromotion.UpdateStatus();
			session.Save(supplierPromotion);
			return supplierPromotion;
		}

		private void RefreshPromotion(SupplierPromotion promotion)
		{
			session.Refresh(promotion);
		}

		[SetUp]
		public void SetUp()
		{
			session.CreateQuery(@"delete SupplierPromotion").ExecuteUpdate();

			_supplier = CreateSupplier();
			_catalog = FindFirstFreeCatalog();
			_promotion = CreatePromotion(_supplier, _catalog);
			_promotionWithFile = CreatePromotion(_supplier, _catalog, withFile: true);
			_promotionNotModerated = CreatePromotion(_supplier, _catalog, false);
			Open();
			Click("Промо-акции");
			AssertText("Список акций");
		}

		[Test(Description = "проверяем отображение основной страницы с акциями")]
		public void OpenIndexPage()
		{
			AssertText("Промо-акции, ожидающие подтверждения");
			AssertText("Список акций");
			AssertText("Наименование");
			AssertText("Поставщик");
			AssertText("Статус:");
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			Assert.That(list.Exists, Is.True);
			Assert.That(list.SelectedItem, Is.EqualTo("Включенные"));
		}

		public void RowPromotionNotExists(SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + promotion.Id);
			Assert.That(row.Exists, Is.False);
		}

		public TableRow RowPromotionExists(SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + promotion.Id);
			Assert.That(row.Exists, Is.True);
			Assert.That(row.OwnTableCell(Find.ByText(promotion.PromotionOwnerSupplier.Name)).Exists, Is.True);
			Assert.That(row.OwnTableCell(Find.ByText(promotion.Name)).Exists, Is.True);
			Assert.That(row.OwnTableCells[1].CheckBoxes.Count, Is.EqualTo(1));
			Assert.That(row.OwnTableCells[1].CheckBoxes[0].Checked, Is.EqualTo(promotion.Enabled));
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
			ClickButton("Показать");

			RowPromotionNotExists(_promotion);

			//она должна быть в списке "Все"
			list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Все");
			ClickButton("Показать");

			RowPromotionExists(_promotion);
		}

		[Test(Description = "проверяем работу фильтра по поставцикам")]
		public void CheckFilterModerationSupplierSurch()
		{
			RowPromotionExists(_promotionNotModerated);
			RowPromotionExists(_promotion);

			browser.TextField(Find.ByName("filter.SearchSupplier")).TypeText(_promotion.PromotionOwnerSupplier.Name + "NOT");
			ClickButton("Показать");

			RowPromotionExists(_promotionNotModerated);
			RowPromotionNotExists(_promotion);

			browser.TextField(Find.ByName("filter.SearchSupplier")).TypeText(_promotion.PromotionOwnerSupplier.Name);
			ClickButton("Показать");

			RowPromotionExists(_promotionNotModerated);
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
			ClickButton("Показать");
			row = RowPromotionExists(_promotion);

			//включаем обратно
			row.OwnTableCells[1].CheckBoxes[0].Click();
			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.True);
			Assert.That(_promotion.AgencyDisabled, Is.False);
			RowPromotionNotExists(_promotion);
		}

		[Test(Description = "проверяем включение/выключение модерации акций")]
		public void PromotionModeration()
		{
			WaitForText("Список акций");
			//проверка отмены подтверждение Промоакции
			Assert.That(_promotion.Moderated, Is.True);
			Assert.That(_promotion.IsActive(), Is.True);
			WaitForText(_promotion.Name);
			Click("Редактировать");

			//проверка подтверждение Промоакции
			Open();

			Click("Промо-акции");
			WaitForText(_promotion.Name);
			Click("Редактировать");
			WaitForText("Акция не активна");
			ClickButton("Подтвердить");
			session.Refresh(_promotion);
			Assert.That(_promotion.Moderated, Is.True);
			Assert.That(_promotion.IsActive(), Is.True);

			//снова отменяем
			Open();
			Click("Промо-акции");
			WaitForText(_promotion.Name);
			Click("Редактировать");
			browser.TextField(Find.ByName("reason")).TypeText("Надо");
			ClickButton("Отменить подтверждение Промоакции");
			session.Refresh(_promotion);
			Assert.That(_promotion.Moderated, Is.False);
			Assert.That(_promotion.IsActive(), Is.False);

			//проверка отказа в публикации Промоакции
			Open();
			Click("Промо-акции");
			//ее не должно быть в списке отключенных
			var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
			list.Select("Отключенные");
			ClickButton("Показать");

			WaitForText(_promotion.Name);
			Click("Редактировать");
			WaitForText("Акция не активна");
			ClickButton("Отказать");
			WaitForText("Необходимо указать причину отказа!");
			browser.TextField(Find.ByName("reason")).TypeText("Надо");
			ClickButton("Отказать");
			session.Refresh(_promotion);
			Assert.That(_promotion.Moderated, Is.False);
			Assert.That(_promotion.IsActive(), Is.False);

			_promotion.Moderator = null;
			session.Save(_promotion);
			session.Flush();
			//проверка отказа в публикации Промоакции
			Open();
			Click("Промо-акции");
			WaitForText(_promotion.Name);
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
			ClickButton("Показать");
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
			ClickButton("Да");

			session.Clear();
			var deletedPromotion = session.Get<SupplierPromotion>(_promotion.Id);
			Assert.That(deletedPromotion, Is.Null);

			RowPromotionNotExists(_promotion);
		}

		[Test(Description = "редактирование акции")]
		public void EditPromotion()
		{
			var row = RowPromotionExists(_promotion);

			row.Link(Find.ByText("Редактировать")).Click();

			AssertText("Редактирование акции №" + _promotion.Id);
			AssertText(_promotion.Name);
			AssertText(_promotion.PromotionOwnerSupplier.Name);
			//нет загруженного файла
			AssertText("не установлен");

			browser.CheckBox(Find.ByName("promotion.Enabled")).Click();
			browser.CheckBox(Find.ByName("promotion.AgencyDisabled")).Click();
			browser.TextField(Find.ByName("promotion.Annotation")).TypeText("новое крутое описание");

			ClickButton("Сохранить");
			ClickButton("Да");

			AssertText("Список акций");
			RowPromotionNotExists(_promotion);

			RefreshPromotion(_promotion);
			Assert.That(_promotion.Enabled, Is.False);
			Assert.That(_promotion.AgencyDisabled, Is.True);
			Assert.That(_promotion.Annotation, Is.EqualTo("новое крутое описание"));
		}

		[Test(Description = "редактирование акции")]
		public void EditPromotionFileRemoveCheck()
		{
			Open($"Promotions/Edit?id={_promotionWithFile.Id}");

			AssertText("Редактирование акции №" + _promotionWithFile.Id);
			AssertText(_promotionWithFile.Name);
			AssertText(_promotionWithFile.PromotionOwnerSupplier.Name);
			//нет загруженного файла

			browser.CheckBox(Find.ByName("promotion.Enabled")).Click();
			browser.CheckBox(Find.ByName("promotion.AgencyDisabled")).Click();
			browser.TextField(Find.ByName("promotion.Annotation")).TypeText("новое крутое описание");

			//есть загруженный файл
			AssertText(_promotionWithFile.PromoFile);

			ClickButton("Сохранить");
			Click("Да");

			AssertText("Список акций");
			RowPromotionNotExists(_promotionWithFile);

			RefreshPromotion(_promotionWithFile);
			session.Refresh(_promotionWithFile);
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

			AssertText("Редактирование акции №" + _promotion.Id);
			AssertText(_promotion.Name);
			AssertText(_promotion.PromotionOwnerSupplier.Name);

			//Переходим на форму редактирования списка
			ClickLink("Редактировать список препаратов");

			AssertText("Редактирование списка препаратов акции №" + _promotion.Id);

			//Выбираем наименования и отмечаем их в таблице
			var chaBoxes = browser.CheckBoxes.Where(cb => cb.Name.StartsWith("cha")).ToList();
			Assert.That(chaBoxes.Count, Is.GreaterThan(3), "Не найдено достаточное количество наименований в каталоге");
			if (chaBoxes.Count > 3)
				for (int i = 0; i < 3; i++) {
					chaBoxes[i].Click();
				}

			//Производим сохранение
			var addBtn = browser.Button(Find.ById("addBtn"));
			Assert.That(addBtn.Exists, Is.True, "Не найдена кнопка добавления наименований к списку выбранных прератов");
			addBtn.Click();
			AssertText("Редактирование списка препаратов акции №" + _promotion.Id);
			AssertText("Сохранено");

			RefreshPromotion(_promotion);
			var promoBoxes = browser.CheckBoxes.Where(cb => cb.Name.StartsWith("chd")).ToList();
			Assert.That(promoBoxes.Count, Is.EqualTo(_promotion.Catalogs.Count), "Не совпадает количество доступных для удаления препаратов со списом препаратов акции");
			foreach (var catalog in _promotion.Catalogs) {
				Assert.That(promoBoxes.Exists(box => box.Name == "chd" + catalog.Id), Is.True, "Не найден checkbox для удаления препарата с Id:{0} Name:{1}", catalog.Id, catalog.Name);
			}

			var parentPromo = browser.Link(Find.ByText("Редактирование акции"));
			Assert.That(parentPromo.Exists, "Не найдена ссылка на родительскую промо-акцию");
			parentPromo.Click();

			AssertText("Редактирование акции №" + _promotion.Id);
			Assert.That(browser.Text, Is.Not.StringContaining("Сохранено"));
		}
	}
}