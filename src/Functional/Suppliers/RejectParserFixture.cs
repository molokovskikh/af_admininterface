using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Functional.Drugstore;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional.Suppliers
{
	[TestFixture]
	public class RejectParser : FunctionalFixture
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
		public void Success_price_delete()
		{
			const string ruleName = "Тестовое правило отказа";
			const string ruleColumn = "PRICE";
			const string ruleProperty = "Cost";

			var listToClean =  session.Query<AdminInterface.Models.RejectParser>().Where(s=>s.Supplier.Id == supplier.Id).ToList();
			session.DeleteEach(listToClean);
			session.Flush();

			Open($"Suppliers/{supplier.Id}");
			Click("Разбор отказов");
			AssertText("Настройка разбора отказов");
			Open($"RejectParser/Add?supplierId={supplier.Id}");
			AssertText("Добавление правила");
			Css("[name='Name']").TypeText(ruleName);
			Click("Добавить");
			Css("[name='Lines[0].Src']").TypeText(ruleColumn);
			Css("[name='Lines[0].Dst']").SelectByValue(ruleProperty);
			Click("Сохранить");

			var listToCheck = session.Query<AdminInterface.Models.RejectParser>().Where(s => s.Supplier.Id == supplier.Id).ToList();
			var itemToCheck = listToCheck.FirstOrDefault();
			Assert.AreEqual(1, listToCheck.Count);
			Assert.AreEqual(1, itemToCheck.Lines.Count);
			Assert.AreEqual(ruleName, itemToCheck.Name);
			Assert.AreEqual(ruleColumn, itemToCheck.Lines.First().Src);
			Assert.AreEqual(ruleProperty, itemToCheck.Lines.First().Dst);

		}

	}
}