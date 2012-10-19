using System;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class SupplierBillingFixture : WatinFixture2
	{
		[Test]
		public void Show_supplier_on_off_logs()
		{
			var supplier = DataMother.CreateSupplier(s => session.Save(s));
			supplier.Disabled = !supplier.Disabled;
			session.SaveOrUpdate(supplier);

			Open(supplier.Payer);

			var table = GetLogTable();
			Assert.That(table.Text, Is.StringContaining("Поставщик"));
			Assert.That(table.Text, Is.StringContaining(supplier.Name));
		}

		[Test]
		public void Filter_log_message_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			var client = DataMother.TestClient(c => {
				c.Payers.Clear();
				c.Payers.Add(payer);
				c.AddUser(new User(c) {
					Name = "Тестовый пользователь",
				});
			});
			MakeNameUniq(supplier);
			session.SaveOrUpdate(client);

			Open(payer);
			Click(supplier.Name);

			browser.CheckBox("filter_Types").Checked = true;

			var table = GetLogTable();
			Assert.That(table.Text, Is.StringContaining("Поставщик"));
			Assert.That(table.Text, Is.StringContaining(supplier.Name));
			GetRow(table, "Тестовый пользователь");
		}

		[Test]
		public void Show_all_show_current()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			var client = DataMother.TestClient(c => {
				c.Payers.Clear();
				c.Payers.Add(payer);
				c.AddUser(new User(c) {
					Name = "Тестовый пользователь",
				});
			});
			MakeNameUniq(supplier);
			session.SaveOrUpdate(client);

			Open(payer);
			Click(supplier.Name);

			var table = GetLogTable();
			var row = GetRow(table, supplier.Name);
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("table-row"));
			row = GetRow(table, "Тестовый пользователь");
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("none"));

			Click("Показать для всех");
			row = GetRow(table, supplier.Name);
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("table-row"));
			row = GetRow(table, "Тестовый пользователь");
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("table-row"));

			Click("Показать только для текущего");
			row = GetRow(table, supplier.Name);
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("table-row"));
			row = GetRow(table, "Тестовый пользователь");
			Assert.That(row.Style.GetAttributeValue("display"), Is.EqualTo("none"));
		}

		private TableRow GetRow(Table table, string text)
		{
			return table.TableCell(Find.ByText(text)).ContainingTableRow;
		}

		private Table GetLogTable()
		{
			var div = browser.Div(Find.ByText("История"));
			var table = ((IElementContainer)div.Parent).Tables.First();
			return table;
		}
	}
}