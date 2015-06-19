using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Tools;
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
	public class InvoiceGroupsFixture : FunctionalFixture
	{
		private Client client;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateClientAndUsers();
			payer = client.Payers.First();
			payer.Recipient = session.Query<Recipient>().First();
			payer.Users.Each(u => u.Accounting.ReadyForAccounting = true);
			session.SaveOrUpdate(client);
			session.Save(payer);
		}

		[Test]
		public void Change_invoice_groups_settings()
		{
			Open(payer);
			AssertText("Плательщик");
			Click("Настройка счетов");
			AssertText("Настройка счетов");

			var groups = GetGroups();
			Assert.That(groups.Count(), Is.EqualTo(1));
			var group = groups.Single();
			group.Css("[name='accounts[0].InvoiceGroup']").TypeText("1");
			Click("Сохранить");
			AssertText("Сохранено");

			groups = GetGroups();
			Assert.That(groups.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Show_preview()
		{
			Open(payer, "InvoiceGroups");
			AssertText("Настройка счетов");
			Click("Предварительный просмотр");
			AssertText("Образец заполнения платежного поручения");
		}

		[Test]
		public void Show_error_if_recipient_not_set()
		{
			payer.Recipient = null;
			session.Save(payer);

			Open(payer, "InvoiceGroups");
			AssertText("Настройка счетов");
			Click("Предварительный просмотр");
			AssertText("Настройка счетов");
			AssertText("У плательщика не указан получатель платежей, выберете получателя платежей.");
		}

		private IEnumerable<Element> GetGroups()
		{
			return browser.CssSelectAll(".DataTable");
		}
	}
}