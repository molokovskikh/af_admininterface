﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.CssSelectorExtensions;

namespace Functional.Billing
{
	[TestFixture]
	public class InvoiceFixture : FunctionalFixture
	{
		private Payer payer;
		private Invoice invoice;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			invoice = new Invoice(payer, new Period(2011, Interval.January), new DateTime(2010, 12, 27));
			session.Save(invoice);
		}

		[Test]
		public void Cancel_invoice()
		{
			Open("Invoices/");
			AssertText("Реестр счетов");
			Css(String.Format("input[value='{0}']", invoice.Id)).Click();
			Css("input[name='delete']").Click();
			AssertText("Удалено");

			session.Clear();
			invoice = session.Get<Invoice>(invoice.Id);
			Assert.That(invoice, Is.Null);
		}

		[Test]
		public void View_printing_form()
		{
			Open("Invoices/");
			AssertText("Реестр счетов");
			Click("Для печати");
			AssertText("Реестр счетов");
		}

		[Test]
		public void Show_predefine_invoice_positions()
		{
			session.DeleteMany(session.Query<Nomenclature>().ToArray());
			Save(new Nomenclature("Мониторинг оптового фармрынка за июль"));

			Open(invoice, "Edit");
			AssertText("Редактирование счета");
			Assert.That(Css("#reference").SelectedItem, Is.EqualTo("Мониторинг оптового фармрынка за июль"));
			Click("Вставить услугу из справочника");
			Assert.That(Css("[name='invoice.parts[0].name']").Text, Is.EqualTo("Мониторинг оптового фармрынка за июль"));
		}

		[Test]
		public void Edit_invoice()
		{
			Open(invoice, "Edit");
			AssertText("Редактирование счета");
			Click("Добавить");
			Assert.That(Parts().Count(), Is.EqualTo(2));
			Css("[name='invoice.parts[2].name']").TypeText("Статистические услуги");
			Css("[name='invoice.parts[2].cost']").TypeText("1500");
			Css("[name='invoice.parts[2].count']").TypeText("2");
			Click("Сохранить");
			AssertText("Сохранено");

			session.Refresh(invoice);
			Assert.That(invoice.Parts.Count, Is.EqualTo(2));
			Assert.That(invoice.Parts[1].Name, Is.EqualTo("Статистические услуги"));
			Assert.That(invoice.Parts[1].Cost, Is.EqualTo(1500));
			Assert.That(invoice.Parts[1].Count, Is.EqualTo(2));
		}

		private IEnumerable<Element> Parts()
		{
			return browser.CssSelectAll("#bills tbody tr");
		}
	}
}