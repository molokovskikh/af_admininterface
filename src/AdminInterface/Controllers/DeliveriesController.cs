using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;
using AdminInterface.Services;
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
		public void Add([DataBind("delivery")] Address address, [DataBind("contacts")] Contact[] contacts, uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging<Address>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				address.Client = client;
				address.Enabled = true;
				address.Save();

				address.UpdateContacts(contacts);

				address.MaitainIntersection();
				address.CreateFtpDirectory();
				client.Users.Each(u => address.SetAccessControl(u.Login));
                client.UpdateBeAccounted();
				scope.VoteCommit();
			}
			Mailer.DeliveryAddressRegistred(address);
			new NotificationService().NotifySupplierAboutAddressRegistration(address);
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
		public void Update([ARDataBind("delivery", AutoLoadBehavior.Always, Expect = "delivery.AvaliableForUsers")] Address address, 
			[DataBind("contacts")] Contact[] contacts, [DataBind("deletedContacts")] Contact[] deletedContacts)
		{
			using (var scope = new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<Address>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				address.UpdateContacts(contacts, deletedContacts);
				address.Update();
				scope.VoteCommit();
			}
			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("client", "info", new {cc = address.Client.Id});
		}

		[AccessibleThrough(Verb.Post)]
		public void Notify(uint id)
		{
			var address = Address.Find(id);
			new NotificationService().NotifySupplierAboutAddressRegistration(address);
			Mailer.AddressRegistrationResened(address.Client, address.Value);
			Flash["Message"] = new Message("Уведомления отправлены");
			RedirectToReferrer();
		}
	}
}
