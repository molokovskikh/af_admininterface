using System;
using System.Configuration;
using System.IO;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Logs;
using AdminInterface.MonoRailExtentions;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Secure]
	public class AddressesController : ARSmartDispatcherController
	{
		public void Show(uint id)
		{
			RedirectUsingRoute("Edit", new {id = id});
		}

		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			PropertyBag["client"] = Client.FindAndCheck(clientId);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([ARDataBind("delivery", AutoLoadBehavior.NewRootInstanceIfInvalidKey)] Address address,
			[DataBind("contacts")] Contact[] contacts,
			uint clientId,
			string comment)
		{
			var client = Client.FindAndCheck(clientId);
			client.AddAddress(address);
			address.UpdateContacts(contacts);
			address.SaveAndFlush();
			address.Maintain();

			address.CreateFtpDirectory();
			client.Users.Each(u => address.SetAccessControl(u.Login));
			Mailer.Registred(address, comment);
			Flash["Message"] = new Message("Адрес доставки создан");
			RedirectUsingRoute("client", "show", new { client.Id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id)
		{
			var address = Address.Find(id);
			PropertyBag["delivery"] = address;
			PropertyBag["client"] = address.Client;
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			if (String.IsNullOrEmpty(address.Registrant))
				PropertyBag["Registrant"] = null;
			else 
				PropertyBag["Registrant"] = Administrator.GetByName(address.Registrant);
			if ((address.ContactGroup != null) && (address.ContactGroup.Contacts != null))
				PropertyBag["ContactGroup"] = address.ContactGroup;
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("delivery", AutoLoadBehavior.Always, Expect = "delivery.AvaliableForUsers")] Address address, 
			[DataBind("contacts")] Contact[] contacts, [DataBind("deletedContacts")] Contact[] deletedContacts)
		{
			address.UpdateContacts(contacts, deletedContacts);

			var oldLegalEntity = address.OldValue(a => a.LegalEntity);
			if (address.Payer != address.LegalEntity.Payer)
			{
				address.Payer = address.LegalEntity.Payer;
				this.Mail().AddressMoved(address, address.Client, oldLegalEntity).Send();
			}

			address.Update();
			if (address.IsChanged(a => a.LegalEntity))
			{
				address.MoveAddressIntersection(address.Client, address.LegalEntity,
					address.Client, oldLegalEntity);
			}

			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("client", "show", new { address.Client.Id });
		}

		[AccessibleThrough(Verb.Post)]
		public void Notify(uint id)
		{
			var address = Address.Find(id);
			Mailer.NotifySupplierAboutAddressRegistration(address);
			Mailer.AddressRegistrationResened(address);
			Flash["Message"] = new Message("Уведомления отправлены");
			RedirectToReferrer();
		}
	}
}
