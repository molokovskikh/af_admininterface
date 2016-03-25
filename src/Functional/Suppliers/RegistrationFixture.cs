using System.Linq;
using System.Threading;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using Test.Support.Web;
using WatiN.Core;

namespace Functional.Suppliers
{
	[TestFixture]
	public class RegistrationFixture2 : AdmSeleniumFixture
	{
		[Test]
		public void Test_validation()
		{
			Open();
			Click("Поставщик");
			AssertText("Регистрация поставщика");
			Css("#supplier_Name").SendKeys("тестовый!");
			Eval("$('#RegisterButton').click()");
			AssertText("Поле может содержать только");
			Assert.AreEqual("Заполнение поля обязательно", Css("label.error[for=options_FederalSupplier]").Text);
		}
	}

	[TestFixture]
	public class RegistrationFixture : FunctionalFixture
	{
		[Test]
		public void AddPhoneAndEmailTest()
		{
			Open();
			Click("Поставщик");
			AssertText("Регистрация поставщика");
			var phoneCount = browser.TableCells.Count(l => l.Text == "Номер телефона:");
			var emailCount = browser.TableCells.Count(l => l.Text == "Email:");
			Css("#supplieraddPhoneLink").Click();
			Css("#supplieraddEmailLink").Click();
			Assert.That(browser.TableCells.Count(l => l.Text == "Номер телефона:"), Is.EqualTo(phoneCount + 1));
			Assert.That(browser.TableCells.Count(l => l.Text == "Email:"), Is.EqualTo(emailCount + 1));
		}

		[Test]
		public void Register()
		{
			Open();
			Click("Поставщик");
			AssertText("Регистрация поставщика");

			Prepare();
			Click("Зарегистрировать");
			AssertText("Регистрация плательщика");
			Click("Сохранить");
			AssertText("Поставщик тестовый");
			AssertText("Список E-mail, с которых разрешена отправка писем клиентам АналитФармация");
		}

		[Test]
		public void Register_supplier_show_user_card()
		{
			Open("Register/RegisterSupplier");
			AssertText("Регистрация поставщика");
			Prepare();

			Css("#options_FillBillingInfo").Click();
			browser.Click("Зарегистрировать");

			AssertText("Регистрационная карта");
		}

		[Test]
		public void Register_supplier_with_additional_contact_info()
		{
			Open("Register/RegisterSupplier");
			AssertText("Регистрация поставщика");
			Prepare();
			Css("#ClientManagersContactPhone").TypeText("473-2606000");
			Css("#ClientManagersContactEmail").TypeText("manager1@analit.net");
			Css("input[name='ClientManagersPersons[0].Name']").TypeText("Родионов Максим Валерьевич");
			Css("#options_FillBillingInfo").Click();
			Css("#options_FederalSupplier").Select("Да");
			browser.Click("Зарегистрировать");
			AssertText("Регистрационная карта");

			var supplier = GetSupplier();
			var group = supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers);
			Assert.That(group.Persons.Select(p => p.Name).ToArray(),
				Is.EquivalentTo(new[] { "Родионов Максим Валерьевич" }));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Email).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new[] { "manager1@analit.net" }));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Phone).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new[] { "473-2606000" }));
			Assert.That(group.Contacts.Count, Is.EqualTo(2));
			Assert.IsTrue(supplier.IsFederal);
		}

		[Test]
		public void Register_supplier_with_order_delivery_info()
		{
			Open("Register/RegisterSupplier");
			AssertText("Регистрация поставщика");
			Prepare();
			Css("#options_FillBillingInfo").Click();

			Css("#browseRegion4").Click();
			Css("#browseRegion1").Click();

			Css("#orderDeliveryGroup_4_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[4].Contacts[2].ContactText']").TypeText("kvasovtest@analit.net");
			Css("#orderDeliveryGroup_4_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[4].Contacts[3].ContactText']").TypeText("kvasovtest2@analit.net");

			Css("#orderDeliveryGroup_1_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[1].Contacts[2].ContactText']").TypeText("kvasovtest1@analit.net");
			Click("Зарегистрировать");

			AssertText("Регистрационная карта");
			var supplier = GetSupplier();
			var contactGroups = supplier.ContactGroupOwner.ContactGroups.OfType<RegionalDeliveryGroup>().ToList();
			Assert.That(contactGroups.Count, Is.EqualTo(3));

			var group1 = contactGroups.First(g => g.Region.Id == 1ul);
			Assert.That(group1.Contacts.Count, Is.EqualTo(1));
			Assert.That(group1.Contacts[0].ContactText, Is.EqualTo("kvasovtest1@analit.net"));

			var group2 = contactGroups.First(g => g.Region.Id == 4ul);
			Assert.That(group2.Contacts.Count, Is.EqualTo(2));
			Assert.That(group2.Contacts[0].ContactText, Is.EqualTo("kvasovtest@analit.net"));
			Assert.That(group2.Contacts[1].ContactText, Is.EqualTo("kvasovtest2@analit.net"));
		}

		[Test]
		public void Register_user_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			Open(supplier);
			Click("Новый пользователь");
			Css("#user_Name").AppendText("test_comment");
			browser.TextField(Find.ByName("mails")).AppendText("kvasovtest@analit.net");
			Click("Создать");
			AssertText("Пользователь создан");
			AssertText($"Поставщик {supplier.Name}, Код {supplier.Id}");
		}

		private Supplier GetSupplier()
		{
			return session.Query<Supplier>().OrderByDescending(s => s.Id).First();
		}

		private void Prepare()
		{
			Css("#supplier_FullName").TypeText("тестовый поставщик");
			Css("#supplier_Name").TypeText("тестовый");
			Css("#SupplierContactPhone").TypeText("473-2606000");
			Css("#SupplierContactEmail").TypeText("kvasovtest@analit.net");
			Css("#ClientManagersContactEmail").TypeText("kvasovtest@analit.net");
			Css("#OrderManagersContactEmail").TypeText("kvasovtest@analit.net");
			Css("#user_Name").TypeText("Тестовый пользователь");
			Css("#options_FederalSupplier").Select("Нет");
		}
	}
}