﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Models;
using NHibernate;
using NUnit.Framework;
using Test.Support.Web;
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
					validText = "123-4567890";
					invalidText = "435265";
					validatorErrorMessage = "Некорректный телефонный номер";
					break;
				}
			}
			browser.Css("#addContactLink").Click();
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
			browser.Css("#addPersonLink").Click();
			browser.TextField(String.Format("persons[{0}].Name", --rowId)).TypeText(personName);
			browser.Button(Find.ByValue(applyButtonText)).Click();
		}

		public static void CheckContactGroupInDb(ISession session, ContactGroup contactGroup)
		{
			var contactGroupCount = session
				.CreateSQLQuery("select count(*) from contacts.contact_groups where Id = :ContactGroupId")
				.SetParameter("ContactGroupId", contactGroup.Id)
				.UniqueResult();
			Assert.That(Convert.ToInt32(contactGroupCount), Is.EqualTo(1), "Не найдена группа контактной информации");
		}

		public static int GetCountContactsInDb(ISession session, ContactGroup contactGroup)
		{
			var contactIds = session
				.CreateSQLQuery("select Id from contacts.contacts where ContactOwnerId = :ownerId")
				.SetParameter("ownerId", contactGroup.Id)
				.List();
			return contactIds.Count;
		}

		public static IList<string> GetPersons(ISession session, ContactGroup contactGroup)
		{
			return session.CreateSQLQuery("select Name from contacts.persons where ContactGroupId = :contactGroupId")
				.SetParameter("contactGroupId", contactGroup.Id)
				.List<string>();
		}
	}
}