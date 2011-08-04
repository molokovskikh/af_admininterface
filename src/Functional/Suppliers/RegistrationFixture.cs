using System;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using NUnit.Framework;

namespace Functional.Suppliers
{
	[TestFixture]
	public class RegistrationFixture : WatinFixture2
	{
		[Test]
		public void Register()
		{
			Open();
			browser.Click("Поставщик");
			Assert.That(browser.Text, Is.StringContaining("Регистрация поставщика"));

			Prepare();

			browser.Click("Зарегистрировать");
			Assert.That(browser.Text, Is.StringContaining("Регистрация плательщика"));
			browser.Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Поставщик тестовый"));
			Console.WriteLine(browser.Url);
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

			var supplier = Supplier.Queryable.OrderByDescending(s => s.Id).First();
			var group = supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers);
			Assert.That(group.Persons.Select(p => p.Name).ToArray(),
				Is.EquivalentTo(new [] {"Родионов Максим Валерьевич"}));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Email).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new [] {"manager1@analit.net"}));
			Assert.That(group.Contacts.Where(c => c.Type == ContactType.Phone).Select(p => p.ContactText).ToArray(),
				Is.EquivalentTo(new [] {"473-2606000"}));
			Assert.That(group.Contacts.Count, Is.EqualTo(2));
		}

		private void Prepare()
		{
			Css("#JuridicalName").TypeText("тестовый поставщик");
			Css("#ShortName").TypeText("тестовый");
			Css("#ClientContactPhone").TypeText("473-2606000");
			Css("#ClientContactEmail").TypeText("kvasovtest@analit.net");
		}

	}
}