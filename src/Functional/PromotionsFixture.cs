using System;
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
	left join usersettings.SupplierPromotions sp on sp.CatalogId = catalogId
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
				Catalog = catalog,
				Annotation = catalog.Name
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


		[Test(Description = "��������� ����������� �������� �������� � �������")]
		public void OpenIndexPage()
		{
			using (var browser = Open("/"))
			{
				var link = browser.Link(Find.ByText("�����-�����"));
				Assert.That(link, Is.Not.Null, "�� ������� ������ � �����-�������");
				link.Click();

				Assert.That(browser.Text, Is.StringContaining("������ �����"));
				Assert.That(browser.Text, Is.StringContaining("������� ����� ��� ������"));
				Assert.That(browser.Text, Is.StringContaining("������:"));
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				Assert.That(list.Exists, Is.True);
				Assert.That(list.SelectedItem, Is.EqualTo("����������")); 
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
			Assert.That(row.OwnTableCell(Find.ByText(_promotion.Catalog.Name)).Exists, Is.True);
			Assert.That(row.OwnTableCells[1].CheckBoxes.Count, Is.EqualTo(1));
			Assert.That(row.OwnTableCells[1].CheckBoxes[0].Checked, Is.EqualTo(promotion.Enabled));
			Assert.That(row.OwnTableCells[2].CheckBoxes.Count, Is.EqualTo(1));
			Assert.That(row.OwnTableCells[2].CheckBoxes[0].Checked, Is.EqualTo(promotion.AgencyDisabled));
			return row;
		}

		[Test(Description = "��������� ������ ������� �� �������� �����")]
		public void CheckFilterStatus()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�����-�����")).Click();

				//������ ��� ��������� ����� ������ ���� � ������
				RowPromotionExists(browser, _promotion);

				//�� �� ������ ���� � ������ �����������
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("�����������");
				browser.Button(Find.ByValue("��������")).Click();

				RowPromotionNotExists(browser, _promotion);

				//��� ������ ���� � ������ "���"
				list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("���");
				browser.Button(Find.ByValue("��������")).Click();

				RowPromotionExists(browser, _promotion);
			}

		}

		[Test(Description = "��������� ���������/���������� �����")]
		public void DisabledPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�����-�����")).Click();

				//��������� �����
				var row = RowPromotionExists(browser, _promotion);
				row.OwnTableCells[1].CheckBoxes[0].Click();

				//��������� ����������
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.False);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionNotExists(browser, _promotion);

				//������� �� � ������ �����������
				var list = browser.SelectList(Find.ByName("filter.PromotionStatus"));
				list.Select("�����������");
				browser.Button(Find.ByValue("��������")).Click();
				row = RowPromotionExists(browser, _promotion);

				//�������� �������
				row.OwnTableCells[1].CheckBoxes[0].Click();
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionNotExists(browser, _promotion);
			}
		}

		[Test(Description = "��������� ���������/���������� ����� ��������������� �� �������")]
		public void AgencyDisabledPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�����-�����")).Click();

				//��������� �����
				var row = RowPromotionExists(browser, _promotion);
				row.OwnTableCells[2].CheckBoxes[0].Click();

				//��������� ����������
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.True);
				row = RowPromotionExists(browser, _promotion);

				//�������� �������
				row.OwnTableCells[2].CheckBoxes[0].Click();
				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.True);
				Assert.That(_promotion.AgencyDisabled, Is.False);
				RowPromotionExists(browser, _promotion);
			}
		}

		[Test(Description = "�������� �����")]
		public void DeletePromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�����-�����")).Click();

				var row = RowPromotionExists(browser, _promotion);

				row.Button(Find.ByValue("�������")).Click();

				var deletedPromotion = SupplierPromotion.TryFind(_promotion.Id);
				Assert.That(deletedPromotion, Is.Null);

				RowPromotionNotExists(browser, _promotion);
			}
		}

		[Test(Description = "�������������� �����")]
		public void EditPromotion()
		{
			using (var browser = Open("/"))
			{
				browser.Link(Find.ByText("�����-�����")).Click();

				var row = RowPromotionExists(browser, _promotion);

				row.Link(Find.ByText("�������������")).Click();

				Assert.That(browser.Text, Is.StringContaining("�������������� ����� �" + _promotion.Id));
				Assert.That(browser.Text, Is.StringContaining(_promotion.Catalog.Name));
				Assert.That(browser.Text, Is.StringContaining(_promotion.Supplier.Name));

				Console.WriteLine(browser.Html);

				browser.CheckBox(Find.ByName("promotion.Enabled")).Click();
				browser.CheckBox(Find.ByName("promotion.AgencyDisabled")).Click();
				browser.TextField(Find.ByName("promotion.Annotation")).TypeText("����� ������ ��������");

				browser.Button(Find.ByValue("���������")).Click();

				Assert.That(browser.Text, Is.StringContaining("������ �����"));
				RowPromotionNotExists(browser, _promotion);

				RefreshPromotion(_promotion);
				Assert.That(_promotion.Enabled, Is.False);
				Assert.That(_promotion.AgencyDisabled, Is.True);
				Assert.That(_promotion.Annotation, Is.EqualTo("����� ������ ��������"));
			}
		}

	}
}