using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Controllers
{
	[Secure]
	public class AddressesController : AdminInterfaceController
	{
		public AddressesController()
		{
			SetARDataBinder();
		}

		public void Show(uint id)
		{
			RedirectUsingRoute("Edit", new { id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck<Client>(clientId);
			var organisations = client.Orgs().ToArray();
			if(organisations.Length == 1) {
				PropertyBag["address"] = new Address(client) { LegalEntity = organisations.First() };
			}
			else {
				PropertyBag["address"] = new Address(client);
			}
			PropertyBag["Organisations"] = organisations;
			PropertyBag["client"] = client;
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["defaultSettings"] = Defaults;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add(
			[DataBind("contacts")] Contact[] contacts,
			uint clientId,
			string comment)
		{
			var client = Client.FindAndCheck<Client>(clientId);
			var address = new Address(client);
			RecreateOnlyIfNullBinder.Prepare(this);
			BindObjectInstance(address, "address", AutoLoadBehavior.NewRootInstanceIfInvalidKey);
			if(client.Payers.Count > 1 && address.AvaliableForUsers != null && address.AvaliableForUsers.Count > 0) {
				address.AvaliableForUsers.Each(u => DbSession.Refresh(u));
				if(address.AvaliableForUsers.All(u => u.Payer.Id != address.LegalEntity.Payer.Id)) {
					Add(client.Id);
					PropertyBag["client"] = client;
					PropertyBag["address"] = address;
					Error("Ошибка регистрации: попытка зарегистрировать пользователя и адрес в различных Плательщиках");
					return;
				}
			}
			client.AddAddress(address);
			address.UpdateContacts(contacts);
			DbSession.Save(address);
			address.Maintain();

			address.CreateFtpDirectory();
			address.AddBillingComment(comment);
			new Mailer(DbSession).Registred(address, comment, Defaults);
			Notify("Адрес доставки создан");
			RedirectUsingRoute("client", "show", new { client.Id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id)
		{
			var address = DbSession.Load<Address>(id);
			PropertyBag["address"] = address;
			PropertyBag["client"] = address.Client;
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["defaultSettings"] = Defaults;
			if (address.ContactGroup != null && address.ContactGroup.Contacts != null)
				PropertyBag["ContactGroup"] = address.ContactGroup;
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("address", AutoLoadBehavior.Always, Expect = "address.AvaliableForUsers")] Address address,
			[DataBind("contacts")] Contact[] contacts, [DataBind("deletedContacts")] Contact[] deletedContacts)
		{
			address.UpdateContacts(contacts, deletedContacts);

			var oldLegalEntity = address.OldValue(a => a.LegalEntity);
			if (address.Payer != address.LegalEntity.Payer) {
				address.Payer = address.LegalEntity.Payer;
				Mail().AddressMoved(address, address.Client, oldLegalEntity);
			}

			DbSession.Save(address);
			if (address.IsChanged(a => a.LegalEntity)) {
				address.MoveAddressIntersection(address.Client, address.LegalEntity,
					address.Client, oldLegalEntity);
			}

			Notify("Сохранено");
			RedirectUsingRoute("client", "show", new { address.Client.Id });
		}

		[AccessibleThrough(Verb.Post)]
		public void Notify(uint id)
		{
			var address = DbSession.Load<Address>(id);
			new Mailer(DbSession).NotifySupplierAboutAddressRegistration(address, Defaults);
			Mailer.AddressRegistrationResened(address);
			DbSession.Save(new AuditRecord(string.Format("Разослано повторное уведомление о регистрации адреса {0}", address.Name), address));
			Notify("Уведомления отправлены");
			RedirectToReferrer();
		}
	}
}