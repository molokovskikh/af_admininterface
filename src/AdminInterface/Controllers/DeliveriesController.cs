using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Models;
using System;
using System.Collections.Generic;
using NHibernate.Mapping;
using Common.Web.Ui.Helpers;
using System.Collections;

namespace AdminInterface.Controllers
{
	[Layout("NewDefault")]
	public class DeliveriesController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			PropertyBag["client"] = Client.FindAndCheck(clientId);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("delivery")] Address address, [DataBind("contacts")] Contact[] contacts, [DataBind("contactTypes")] ContactType[] contactTypes, uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				address.Client = client;
				address.Save();

				UpdateContacts(address, contacts, contactTypes);

				address.MaitainIntersection();
				address.CreateFtpDirectory();
				client.Users.Each(u => address.SetAccessControl(u.Login));
				scope.VoteCommit();
			}
			Mailer.DeliveryAddressRegistred(address);
			Flash["Message"] = new Message("Адрес доставки создан");
			RedirectUsingRoute("client", "info", new { cc = client.Id });
		}
		
		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id)
		{
			var address = Address.Find(id);
			PropertyBag["delivery"] = address;
			PropertyBag["client"] = address.Client;
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			if ((address.ContactGroup != null) && (address.ContactGroup.Contacts != null))
				PropertyBag["ContactGroup"] = address.ContactGroup;
		}
				
		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("delivery", AutoLoadBehavior.Always, Expect = "delivery.AvaliableForUsers")] Address address, [DataBind("contacts")] Contact[] contacts, [DataBind("contactTypes")] ContactType[] contactTypes)
		{
			UpdateContacts(address, contacts, contactTypes);

			address.Update();
			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("client", "info", new {cc = address.Client.Id});
		}
		
		private bool UpdateContacts(Address address, Contact[] contacts, ContactType[] contactTypes)
		{
			if (address.ContactGroup == null)
				CreateContactGroup(address);
			address.ContactGroup.Contacts.Clear();
			for (var i = 0; i < contacts.Length; i++)
			{
				if (!AddContact(address.ContactGroup, contactTypes[i], contacts[i].ContactText))
					return false;
			}
			return true;
		}

		private bool AddContact(ContactGroup contactGroup, ContactType contactType, string contactText)
		{
			var result = true;
			if (!String.IsNullOrEmpty(contactText))
			{
				var contact = contactGroup.AddContact(contactType, contactText);
				if (ValidationHelper.IsInstanceHasValidationError(contact))
				{
					contactGroup.Contacts.Remove(contact);
					result = false;
				}
			}
			contactGroup.Save();
			return result;
		}
		
		private ContactGroup CreateContactGroup(Address address)
		{
			var groupOwner = new ContactGroupOwner();
			var group = groupOwner.AddContactGroup(ContactGroupType.General);
			groupOwner.Save();
			group.Save();
			address.ContactGroup = group;
			address.Save();
			return group;
		}
	}
}
