using System;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class SupplierBillingFixture : WatinFixture2
	{
		[Test]
		public void Show_supplier_on_off_logs()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.MakeNameUniq();
			supplier.Save();

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
				c.AddUser(new User((Service)c) {
					Name = "Тестовый пользователь",
				});
			});
			supplier.MakeNameUniq();
			supplier.Save();
			client.Save();

			Open(payer);
			Click(supplier.Name);
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
				c.AddUser(new User((Service)c) {
					Name = "Тестовый пользователь",
				});
			});
			supplier.MakeNameUniq();
			supplier.Save();
			client.Save();

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
			var div = browser.Div(Find.ByText("Статистика включений/выключений"));
			var table = ((IElementContainer) div.Parent).Tables.First();
			return table;
		}
	}
}