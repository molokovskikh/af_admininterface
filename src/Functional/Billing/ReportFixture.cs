﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class ReportFixture : FunctionalFixture
	{
		private Client client;
		private Report report;
		private Payer payer;
		private ReportAccount account;

		[SetUp]
		public void SetUp()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			payer = client.Payers.First();

			account = DataMother.Report(payer);
			report = account.Report;
			session.Save(account);

			payer.Recipient = session.Query<Recipient>().First();
			payer.Reports.Add(report);

			Open(payer);
			AssertText("Плательщик");
		}

		[Test]
		public void Short_report_for_payer()
		{
			AssertText("Отчеты");
			AssertText("тестовый отчет");
		}

		[Test]
		public void Disable_report()
		{
			var element = (CheckBox)ElementFor(account, r => r.Status);
			element.Click();
			Css("input[name=AddComment]").AppendText("Disable_report");
			ConfirmDialog();
			session.Refresh(report);
			Wait(() => !report.Allow, "Не дождались");
			Assert.That(report.Allow, Is.False);
		}

		[Test]
		public void Show_unaccounted_report()
		{
			Open("/Accounts/Index");
			var link = browser.Links.FirstOrDefault(l => l.Text == "Последняя »");
			if(link != null)
				link.Click();
			AssertText("тестовый отчет");
		}

		[Test]
		public void Show_accounted_report()
		{
			account.Accounted();
			session.Save(account);

			Open("/Accounts/Index");
			Click("История поставленных на учет");
			AssertText("тестовый отчет");
		}

		[Test]
		public void Edit_report_account_description()
		{
			Click("#reports", "Редактировать");
			AssertText("Отчет, тестовый отчет");
			Assert.That(Css("[name='account.Description']").Value, Is.EqualTo("Статистический отчет по фармрынку за {0}"));
			Css("[name='account.Description']").TypeText("Стат. отчет");
			Click("Сохранить");
			AssertText("Сохранено");

			session.Refresh(account);
			Assert.That(account.Description, Is.EqualTo("Стат. отчет"));
		}

		private Element ElementFor<T>(T item, Func<T, object> property)
		{
			var id = item.GetType().GetProperty("Id").GetValue(item, null);
			var idElement = (Element)browser.Css(String.Format("input[type=hidden][name=id][value='{0}']", id));
			var propertyName = "status";
			var row = (TableRow)idElement.Parents().OfType<TableRow>().First();
			return row.CheckBox(Find.ByName(propertyName));
		}
	}
}