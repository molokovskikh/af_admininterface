using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using NUnit.Framework;
using Test.Support.Web;

namespace Functional.Suppliers
{
	[TestFixture]
	public class RegistrationFixture : WatinFixture2
	{
		[Test]
		public void Register()
		{
			Open();
			Click("Поставщик");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));

			Prepare();

			Click("Зарегистрировать");
			Assert.That(browser.Text, Is.StringContaining("Регистрация плательщика"));
			Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Поставщик тестовый"));
		}

		[Test]
		public void Register_supplier_show_user_card()
		{
			Open("Register/RegisterSupplier");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));
			Prepare();

			browser.Css("#FillBillingInfo").Click();
			browser.Click("Зарегистрировать");

			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
		}

		[Test]
		public void Register_supplier_with_additional_contact_info()
		{
			Open("Register/RegisterSupplier");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));
			Prepare();
			Css("#ClientManagersContactPhone").TypeText("473-2606000");
			Css("#ClientManagersContactEmail").TypeText("manager1@analit.net");
			Css("input[name='ClientManagersPersons[0].Name']").TypeText("Родионов Максим Валерьевич");
			browser.Css("#FillBillingInfo").Click();
			browser.Click("Зарегистрировать");
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));

			var supplier = GetSupplier();
			var group = supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers);
			Assert.That(group.Persons.Select(p => p.Name).ToArray(),
				Is.EquivalentTo(new [] {"Родионов Максим Валерьевич"}));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Email).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new [] {"manager1@analit.net"}));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Phone).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new [] {"473-2606000"}));
			Assert.That(group.Contacts.Count, Is.EqualTo(2));
		}

		[Test]
		public void Register_supplier_with_order_delivery_info()
		{
			Open("Register/RegisterSupplier");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));
			Prepare();
			browser.Css("#FillBillingInfo").Click();

			Css("#browseRegion4").Click();
			Css("#browseRegion1").Click();

			Css("#orderDeliveryGroup_4_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[4].Contacts[2].ContactText']").TypeText("kvasovtest@analit.net");
			Css("#orderDeliveryGroup_4_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[4].Contacts[3].ContactText']").TypeText("kvasovtest2@analit.net");

			Css("#orderDeliveryGroup_1_addEmailLink").Click();
			Css("input[name='orderDeliveryGroup[1].Contacts[2].ContactText']").TypeText("kvasovtest1@analit.net");
			Click("Зарегистрировать");

			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
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

		private static Supplier GetSupplier()
		{
			return Supplier.Queryable.OrderByDescending(s => s.Id).First();
		}

		private void Prepare()
		{
			Css("#JuridicalName").TypeText("тестовый поставщик");
			Css("#ShortName").TypeText("тестовый");
			Css("#ClientContactPhone").TypeText("473-2606000");
			Css("#ClientContactEmail").TypeText("kvasovtest@analit.net");
			Css("#user_Name").TypeText("Тестовый пользователь");
		}
	}
}