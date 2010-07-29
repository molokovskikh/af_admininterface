using AdminInterface.Helpers;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Models;

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
				DbLogHelper.SetupParametersForTriggerLogging();

				client.RegisterDeliveryAddress(address);
				address.UpdateContacts(contacts);
				address.Save();
				address.MaitainIntersection();

				scope.VoteCommit();
			}

			address.CreateFtpDirectory();
			client.Users.Each(u => address.SetAccessControl(u.Login));

			Mailer.AddressRegistred(address);
			Mailer.NotifySupplierAboutAddressRegistration(address);
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
				DbLogHelper.SetupParametersForTriggerLogging();
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
			Mailer.NotifySupplierAboutAddressRegistration(address);
			Mailer.AddressRegistrationResened(address);
			Flash["Message"] = new Message("Уведомления отправлены");
			RedirectToReferrer();
		}

		public void MoveAddressToAnotherClient(uint addressId, uint clientId, bool moveWithUser)
		{
			CancelLayout();
			CancelView();
			var newClient = Client.Find(clientId);
			var address = Address.Find(addressId);
			// Если нужно перенести вместе с пользователем,
			// адрес привязан только к этому пользователю и у пользователя нет других адресов,
			// тогда переносим пользователя
			if (moveWithUser &&
				(address.AvaliableForUsers.Count == 1) &&
				(address.AvaliableForUsers[0].AvaliableAddresses.Count == 1) &&
				(address.AvaliableForUsers[0].AvaliableAddresses[0].Id == addressId))
			{
				address.AvaliableForUsers[0].MoveToAnotherClient(newClient);
			}
			address.MoveToAnotherClient(newClient);
			Flash["Message"] = Message.Notify("Адрес доставки успешно перемещен");
			RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
		}
	}
}
