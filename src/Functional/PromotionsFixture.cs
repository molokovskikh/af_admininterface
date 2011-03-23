﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class PromotionsFixture : WatinFixture
	{

		private Supplier _supplier;
		private Catalog _catalog;
		private SupplierPromotion _promotion;

		[TestFixtureSetUp]
		public void Init()
		{
			ArHelper.WithSession(s =>
									{
										s.CreateSQLQuery(
											@"
delete from 
	usersettings.SupplierPromotions
where
	SupplierId in (select FirmCode from usersettings.ClientsData where ShortName like 'Test supplier%')")
											.ExecuteUpdate();
			});

		}

		private Supplier CreateSupplier()
		{
			var supplier = DataMother.CreateTestSupplier();
			supplier.Name += " " + supplier.Id;
			supplier.Save();
			return supplier;
		}

		private Catalog FindFirstFreeCatalog()
		{
			var catalogId = ArHelper.WithSession<uint>(
				s => 
					s.CreateSQLQuery(
				@"
select
	catalog.Id
from
	catalogs.catalog
	left join usersettings.PromotionCatalogs pc on pc.CatalogId = catalogId
	left join usersettings.SupplierPromotions sp on sp.Id = pc.PromotionId
	left join usersettings.clientsdata cd on cd.FirmCode = sp.SupplierId and cd.ShortName like 'Test supplier%'
where
	catalog.Hidden = 0
and cd.FirmCode is null
limit 1")
						.UniqueResult<uint>());
			return Catalog.Find(catalogId);
		}

		private SupplierPromotion CreatePromotion(Supplier supplier, Catalog catalog)
		{
			var supplierPromotion = new SupplierPromotion
			{
				Enabled = true,
				Supplier = supplier,
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
			using (new SessionScope())
			{
				promotion.Refresh();
				NHibernateUtil.Initialize(promotion);
				NHibernateUtil.Initialize(promotion.Supplier);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_supplier = CreateSupplier();
			_catalog = FindFirstFreeCatalog();
			_promotion = CreatePromotion(_supplier, _catalog);
		}


		[Test(Description = "проверяем отображение основной страницы с акциями")]
		public void OpenIndexPage()
		{
			using (var browser = Open("/"))
			{
				var link = browser.Link(Find.ByText("Промо-акции"));
				Assert.That(link, Is.Not.Null, "Не найдена ссылка с промо-акциями");
				link.Click();

				Assert.That(browser.Text, Is.StringContaining("Список акций"));
				Assert.That(browser.Text, Is.StringContaining("Введите текст для поиска"));
				Assert.That(browser.Text, Is.StringContaining("Статус:"));
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				Assert.That(list.Exists, Is.True);
				Assert.That(list.SelectedItem, Is.EqualTo("Включенные")); 
			}
		}

		public void RowPromotionNotExists(IE browser, SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + _promotion.Id);
			Assert.That(row.Exists, Is.False);
		}

		public TableRow RowPromotionExists(IE browser, SupplierPromotion promotion)
		{
			var row = browser.TableRow("SupplierPromotionRow" + _promotion.Id);
			Assert.That(row.Exists, Is.True);
			Assert.That(row.OwnTableCell(Find.ByText(_promotion.Supplier.Name)).Exists, Is.True);
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
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				//Только что созданная акция должна быть в списке
				RowPromotionExists(browser, _promotion);

				//ее не должно быть в списке отключенных
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("Отключенные");
				browser.Button(Find.ByValue("Показать")).Click();

				RowPromotionNotExists(browser, _promotion);

				//она должна быть в списке "Все"
				list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("Все");
				browser.Button(Find.ByValue("Показать")).Click();

				RowPromotionExists(browser, _promotion);
			}

		}

		[Test(Description = "проверяем включение/выключение акций")]
		public void DisabledPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				//отключаем акцию
				var row = RowPromotionExists(browser, _promotion);
				row.OwnTableCells[1].CheckBoxes[0].Click();

				//проверяем отключение
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.False);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionNotExists(browser, _promotion);

				//находим ее в списке отключенных
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("Отключенные");
				browser.Button(Find.ByValue("Показать")).Click();
				row = RowPromotionExists(browser, _promotion);

				//включаем обратно
				row.OwnTableCells[1].CheckBoxes[0].Click();
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionNotExists(browser, _promotion);
			}
		}

		[Test(Description = "проверяем включение/выключение акций административно АК Инфорум")]
		public void AgencyDisabledPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				//отключаем акцию
				var row = RowPromotionExists(browser, _promotion);
				row.OwnTableCells[2].CheckBoxes[0].Click();

				//проверяем отключение
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.True);
				row = RowPromotionExists(browser, _promotion);

				//включаем обратно
				row.OwnTableCells[2].CheckBoxes[0].Click();
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionExists(browser, _promotion);
			}
		}

		[Test(Description = "удаление акции")]
		public void DeletePromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				var row = RowPromotionExists(browser, _promotion);

				row.Button(Find.ByValue("Удалить")).Click();

				var deletedPromotion = SupplierPromotion.TryFind(_promotion.Id);
				Assert.That(deletedPromotion, Is.Null);

				RowPromotionNotExists(browser, _promotion);
			}
		}

		[Test(Description = "редактирование акции")]
		public void EditPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				var row = RowPromotionExists(browser, _promotion);

				row.Link(Find.ByText("Редактировать")).Click();

				Assert.That(browser.Text, Is.StringContaining("Редактирование акции №" + _promotion.Id));
				Assert.That(browser.Text, Is.StringContaining(_promotion.Name));
				Assert.That(browser.Text, Is.StringContaining(_promotion.Supplier.Name));

				Console.WriteLine(browser.Html);

				browser.CheckBox(Find.ByName("promotion.Enabled")).Click();
				browser.CheckBox(Find.ByName("promotion.AgencyDisabled")).Click();
				browser.TextField(Find.ByName("promotion.Annotation")).TypeText("новое крутое описание");

				browser.Button(Find.ByValue("Сохранить")).Click();

				Assert.That(browser.Text, Is.StringContaining("Список акций"));
				RowPromotionNotExists(browser, _promotion);

				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.False);
				Assert.That(_promotion.AgencyDisabled, Is.True);
				Assert.That(_promotion.Annotation, Is.EqualTo("новое крутое описание"));
			}
		}

		[Test(Description = "проверяем создание новой акции")]
		public void CreateNewPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("Промо-акции")).Click();

				var addButton = browser.Button(Find.ByValue("Добавить новую акцию"));
				Assert.That(addButton.Exists, Is.True, "Не найдена кнопка добавления акции");
			}
		}
	}
}