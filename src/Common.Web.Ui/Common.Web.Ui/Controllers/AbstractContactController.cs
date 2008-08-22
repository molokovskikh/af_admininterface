using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace Common.Web.Ui.Controllers
{
	[Helper(typeof(BindingHelper))]
	public class AbstractContactController : SmartDispatcherController
	{
		public void EditContactGroup(uint contactGroupId)
		{
			var contactGroup = ContactGroup.Find(contactGroupId);
			PropertyBag["CurrentContactGroup"] = contactGroup;
		}

		public virtual void UpdateContactGroup(uint contactGroupId,
		                                       [DataBind("Contacts")] Contact[] contacts)
		{
			var contactGroup = ContactGroup.Find(contactGroupId);
			if (ValidationHelper.IsCollectionHasNotValideObject(contacts))
			{
				PropertyBag["CurrentContactGroup"] = contactGroup;
				PropertyBag["Contacts"] = CleanUp(contacts);
				PropertyBag["Invalid"] = true;
				RenderView("EditContactGroup");
			}
			else
			{
				using (new TransactionScope())
				{
					UpdateContactForContactOwner(contacts, contactGroup);
					contactGroup.Save();
				}
				RedirectToAction("EditContactGroup", "contactGroupId=" + contactGroup.Id);
			}
		}

		public void EditPerson(uint personId)
		{
			var person = Person.Find(personId);
			PropertyBag["CurrentPerson"] = person;
		}

		public virtual void UpdatePerson([DataBind("CurrentPerson")] Person person,
		                                 [DataBind("Contacts")] Contact[] contacts)
		{
			if (ValidationHelper.IsInstanceHasValidationError(person)
				|| ValidationHelper.IsCollectionHasNotValideObject(contacts))
			{
				PropertyBag["CurrentPerson"] = person;
				PropertyBag["Contacts"] = CleanUp(contacts);
				PropertyBag["Invalid"] = true;
				RenderView("EditPerson");
			}
			else
			{
				var existsPerson = Person.Find(person.Id);
				existsPerson.Name = person.Name;
				using (new TransactionScope())
				{
					UpdateContactForContactOwner(contacts, existsPerson);
					existsPerson.SaveAndFlush();
				}
				RedirectToAction("EditContactGroup", "contactGroupId=" + existsPerson.ContactGroup.Id);
			}
		}

		public void NewPerson(uint contactGroupId)
		{
			PropertyBag["ContactGroupId"] = contactGroupId;
			PropertyBag["Contacts"] = new Contact[0];
		}

		public virtual void AddPerson(uint contactGroupId,
		                              [DataBind("CurrentPerson")] Person person,
		                              [DataBind("Contacts")] Contact[] contacts)
		{
			if (ValidationHelper.IsInstanceHasValidationError(person) 
				|| ValidationHelper.IsCollectionHasNotValideObject(contacts))
			{
				PropertyBag["CurrentPerson"] = person;
				PropertyBag["Contacts"] = CleanUp(contacts);
				PropertyBag["Invalid"] = true;
				RenderView("NewPerson");
			}
			else
			{
				var contactGroup = ContactGroup.Find(contactGroupId);
				person.ContactGroup = contactGroup;

				foreach (var contact in contacts)
					if (!String.IsNullOrEmpty(contact.ContactText))
						person.AddContact(contact);

				using (new TransactionScope())
					person.SaveAndFlush();

				RedirectToAction("EditContactGroup", "contactGroupId=" + contactGroup.Id);
			}
		}

		public void DeletePerson(uint personId)
		{
			var person = Person.Find(personId);
			var contactGroupId = person.ContactGroup.Id;
			person.DeleteAndFlush();
			RedirectToAction("EditContactGroup", "contactGroupId=" + contactGroupId);
		}

		protected static Contact[] CleanUp(Contact[] contacts)
		{
			var cleanContacts = new List<Contact>(contacts);
			cleanContacts.RemoveAll(obj => String.IsNullOrEmpty(obj.ContactText));
			return cleanContacts.ToArray();
		}

		protected void UpdateContactForContactOwner(Contact[] contacts, ContactOwner contactOwner)
		{
			var toRemove = new List<Contact>();
			foreach (var existsContact in contactOwner.Contacts)
			{
				var newContact = FindContact(contacts, existsContact.Id);
				if (newContact == null)
					toRemove.Add(existsContact);
				else if (String.IsNullOrEmpty(newContact.ContactText))
					toRemove.Add(existsContact);
				else if (existsContact.ContactText != newContact.ContactText
						 || existsContact.Type != newContact.Type
						 || existsContact.Comment != newContact.Comment)
				{
					existsContact.ContactText = newContact.ContactText;
					existsContact.Type = newContact.Type;
					existsContact.Comment = newContact.Comment;
					new ContactLogEntity(existsContact,
										 Session["UserName"].ToString(),
										 Request.UserHostAddress,
										 OperationType.Update)
					.Save();
				}
			}
			foreach (var newContact in contacts)
				if (newContact.Id == 0 && !String.IsNullOrEmpty(newContact.ContactText))
				{
					newContact.ContactOwner = contactOwner;
					new ContactLogEntity(newContact, 
										 Session["UserName"].ToString(),
										 Request.UserHostAddress, 
										 OperationType.Add)
						.Save();
				}
			foreach (var contact in toRemove)
			{
				new ContactLogEntity(contact, 
									 Session["UserName"].ToString(),
									 Request.UserHostAddress, 
									 OperationType.Delete).Save();
				contactOwner.Contacts.Remove(contact);
			}
		}

		protected static Contact FindContact(IEnumerable<Contact> contacts, uint contactId)
		{
			foreach (var contact in contacts)
				if (contact.Id == contactId)
					return contact;
			return null;
		}
	}
}
