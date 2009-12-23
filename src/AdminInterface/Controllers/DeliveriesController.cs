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
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("delivery")] Address address, uint clientId, string contactEmailText, string contactPhoneText)
		{
			var client = Client.FindAndCheck(clientId);
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				address.Client = client;
				address.Save();

				var group = CreateContactGroup(address);
				if (!UpdateContactInformation(group, contactPhoneText, contactEmailText))
				{
					Flash["Message"] = Message.Error("Введены неверные данные");
					RedirectToReferrer();
					return;
				}

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

			if ((address.ContactGroup == null) || (address.ContactGroup.Contacts == null))
				return;
			foreach (var contact in address.ContactGroup.Contacts)
			{
				switch (contact.Type)
				{
					case ContactType.Email:
							PropertyBag["ContactGroupEmail"] = contact.ContactText;
							break;
					case ContactType.Phone:
							PropertyBag["ContactGroupPhone"] = contact.ContactText;
							break;
				}
			}
		}
				
		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("delivery", AutoLoadBehavior.Always, Expect = "delivery.AvaliableForUsers")] Address address, string contactEmailText, string contactPhoneText)
		{			
			if (address.ContactGroup == null)
				CreateContactGroup(address);
			if (!UpdateContactInformation(address.ContactGroup, contactPhoneText, contactEmailText))
			{
				Flash["Message"] = Message.Error("Введены неверные данные");
				RedirectToReferrer();
				return;				
			}
			address.Update();
			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("client", "info", new {cc = address.Client.Id});
		}

		private bool UpdateContactInformation(ContactGroup contactGroup, string contactPhone, string contactEmail)
		{
			contactGroup.Contacts.Clear();

			if (!AddContact(contactGroup, ContactType.Email, contactEmail))
				return false;
			if (!AddContact(contactGroup, ContactType.Phone, contactPhone))
				return false;
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
