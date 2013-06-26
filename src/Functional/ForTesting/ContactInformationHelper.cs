using System;
using System.Collections.Generic;
using System.Linq;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Models;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.ForTesting
{
	public class ContactInformationHelper
	{
		public static void AddContact(Browser browser, ContactType contactType, string applyButtonText, uint clientId)
		{
			var validText = String.Empty;
			var invalidText = String.Empty;
			var validatorErrorMessage = String.Empty;
			switch (contactType) {
				case ContactType.Email: {
					validText = "test@mail.com";
					invalidText = "test.mail.com";
					validatorErrorMessage = "Некорректный адрес электронной почты";
					break;
				}
				case ContactType.Phone: {
					validText = "123-456789";
					invalidText = "435265";
					validatorErrorMessage = "Некорректный телефонный номер";
					break;
				}
			}
			browser.Link(Find.ById("addContactLink")).Click();
			var rowId = 0;
			if (contactType == ContactType.Phone) {
				var comboBox = browser.SelectList(Find.ByName(String.Format("contactTypes[{0}]", --rowId)));
				comboBox = browser.SelectLists.Last();
				comboBox.SelectByValue(comboBox.Options[1].Value);
				rowId++;
			}
			browser.TextField(String.Format("contacts[{0}].ContactText", --rowId)).TypeText(invalidText);
			browser.Button(Find.ByValue(applyButtonText)).Click();
			Assert.That(browser.Text, Is.StringContaining(validatorErrorMessage));
			browser.TextField(String.Format("contacts[{0}].ContactText", rowId)).TypeText(validText);
			browser.Button(Find.ByValue(applyButtonText)).Click();
		}

		public static void AddPerson(Browser browser, string personName, string applyButtonText, uint clientId)
		{
			var rowId = 0;
			browser.Link(Find.ById("addPersonLink")).Click();
			browser.TextField(String.Format("persons[{0}].Name", --rowId)).TypeText(personName);
			browser.Button(Find.ByValue(applyButtonText)).Click();
		}

		public static void CheckContactGroupInDb(ContactGroup contactGroup)
		{
			var contactGroupCount = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select count(*) from contacts.contact_groups where Id = :ContactGroupId")
					.SetParameter("ContactGroupId", contactGroup.Id)
					.UniqueResult());
			Assert.That(Convert.ToInt32(contactGroupCount), Is.EqualTo(1), "Не найдена группа контактной информации");
		}

		public static int GetCountContactsInDb(ContactGroup contactGroup)
		{
			var contactIds = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select Id from contacts.contacts where ContactOwnerId = :ownerId")
					.SetParameter("ownerId", contactGroup.Id)
					.List());
			return contactIds.Count;
		}

		public static IList<string> GetPersons(ContactGroup contactGroup)
		{
			var personNames = ArHelper.WithSession(s =>
				s.CreateSQLQuery("select Name from contacts.persons where ContactGroupId = :contactGroupId")
					.SetParameter("contactGroupId", contactGroup.Id)
					.List<string>());
			return personNames;
		}

		public static void DeleteContact(IE browser, ContactGroup contactGroup)
		{
		}
	}
}