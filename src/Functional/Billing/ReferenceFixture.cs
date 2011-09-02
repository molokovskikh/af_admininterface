using AdminInterface.Models.Billing;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class ReferenceFixture : WatinFixture2
	{
		[SetUp]
		public void Setup()
		{
			Nomenclature.DeleteAll();
			scope.Flush();

			Open("References");
			AssertText("Справочники");
		}

		[Test]
		public void Create_new_nomenclature()
		{
			Click("Номенклатура");
			Css("#Nomenclature-tab a.new").Click();
			Css("#Nomenclature-tab input[name='items[0].Name']").TypeText("Мониторинг оптового фармрынка за июнь");
			Css("#Nomenclature-tab input[type='submit']").Click();
			AssertText("Сохранено");
			Assert.That((string)Css("#Nomenclature").ClassName, Is.EqualTo("selected"));
			Assert.That((string)Css("#Nomenclature-tab input[name='items[0].Name']").Text, Is.EqualTo("Мониторинг оптового фармрынка за июнь"));

			var nomenclatures = Nomenclature.FindAll();
			Assert.That(nomenclatures.Length, Is.EqualTo(1));
			Assert.That(nomenclatures[0].Name, Is.EqualTo("Мониторинг оптового фармрынка за июнь"));
		}

		[Test]
		public void Delete_nomenclature()
		{
			new Nomenclature("Мониторинг оптового фармрынка за июнь").Save();
			Refresh();

			Click("Номенклатура");
			Css("#Nomenclature-tab a.delete").Click();
			var table = (Table)Css("#Nomenclature-tab .DataTable");
			Assert.That(table.TableRows.Count, Is.EqualTo(1));
			Css("#Nomenclature-tab input[type='submit']").Click();
			AssertText("Сохранено");

			table = (Table)Css("#Nomenclature-tab .DataTable");
			Assert.That(table.TableRows.Count, Is.EqualTo(1));

			var nomenclatures = Nomenclature.FindAll();
			Assert.That(nomenclatures.Length, Is.EqualTo(0));
		}

		[Test]
		public void Validate_nomenclature()
		{
			Click("Номенклатура");
			Css("#Nomenclature-tab a.new").Click();
			Css("#Nomenclature-tab input[type='submit']").Click();
			AssertText("Заполнение поля обязательно");

			var nomenclatures = Nomenclature.FindAll();
			Assert.That(nomenclatures.Length, Is.EqualTo(0));

			Css("#Nomenclature-tab input[name='items[0].Name']").TypeText("Мониторинг оптового фармрынка за июнь");
			Css("#Nomenclature-tab input[type='submit']").Click();
			AssertText("Сохранено");

			nomenclatures = Nomenclature.FindAll();
			Assert.That(nomenclatures.Length, Is.EqualTo(1));
			Assert.That(nomenclatures[0].Name, Is.EqualTo("Мониторинг оптового фармрынка за июнь"));
		}
	}
}