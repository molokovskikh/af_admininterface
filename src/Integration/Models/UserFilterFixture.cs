using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class UserFilterFixture : Test.Support.IntegrationFixture
	{
		private UserFilter filter;
		private Random random;

		[SetUp]
		public void Setup()
		{
			random = new Random();
			filter = new UserFilter(session);
		}

		[Test]
		public void Search_in_contacts_and_cout_results()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			session.SaveOrUpdate(client);
			var firstContact = new ContactGroup(ContactGroupType.General);
			var phone1 = RandomPhone();
			var phone2 = RandomPhone();
			var phone3 = RandomPhone();
			firstContact.AddContact(ContactType.Phone, phone1);
			firstContact.AddContact(ContactType.Phone, phone2);
			client.ContactGroupOwner.AddContactGroup(firstContact);
			var user = client.Users.FirstOrDefault();
			user.ContactGroup = new ContactGroup(ContactGroupType.General) { ContactGroupOwner = client.ContactGroupOwner };
			user.ContactGroup.AddContact(ContactType.Phone, phone3);
			var newUser = new User(client);
			newUser.Setup();
			client.AddUser(newUser);
			session.SaveOrUpdate(newUser);
			session.SaveOrUpdate(client);
			Flush();
			filter.SearchBy = SearchUserBy.Auto;
			filter.SearchText = phone1;
			var result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(2));
			filter.SearchText = phone3;
			result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public void Search_by_address_mail()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			var address = client.Addresses.First();
			address.AvaliableForUsers.Add(client.Users[0]);
			Flush();

			filter.SearchBy = SearchUserBy.AddressMail;
			var addr = address.Id.ToString();
			filter.SearchText = String.Format("{0}@waybills.analit.net", addr.Substring(0, addr.Length - 1));
			var result = filter.Find();

			Assert.That(result.Count, Is.EqualTo(0), result.Implode());

			filter.SearchBy = SearchUserBy.AddressMail;
			filter.SearchText = String.Format("{0}@waybills.analit.net", address.Id);
			result = filter.Find();

			Assert.That(result.Count, Is.EqualTo(1), result.Implode());
			Assert.That(result[0].UserId, Is.EqualTo(user.Id));
		}

		[Test]
		public void Disabled_if_user_disabled()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			user.Enabled = false;
			Save(user);
			Flush();

			filter.SearchBy = SearchUserBy.ByUserId;
			filter.SearchText = user.Id.ToString();
			var result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(1), result.Implode());
			Assert.That(result[0].SelfDisabled, Is.True);
		}

		[Test]
		public void Search_by_supplier_contact_phone()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;

			var phone = RandomPhone();
			supplier.ContactGroupOwner.ContactGroups[0].AddContact(ContactType.Phone, phone);
			Save(supplier);
			Flush();

			filter.SearchText = phone;
			var result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].ClientId, Is.EqualTo(supplier.Id));
		}

		[Test]
		public void Do_not_search_contacts_in_disabled_order_delivery_groups()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;

			var phone = RandomPhone();
			var orderGroup = new RegionalDeliveryGroup(Region.Find(2ul));
			orderGroup.AddContact(ContactType.Phone, phone);
			supplier.ContactGroupOwner.AddContactGroup(orderGroup, true);
			Save(supplier);
			Flush();

			filter.SearchText = phone;
			var result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(0), result.Implode(r => r.ClientId));
		}

		[Test]
		public void Search_email_in_user()
		{
			var user = DataMother.CreateSupplierUser();
			var supplier = (Supplier)user.RootService;

			var email = @"testmailQWERTYS@mail.ru";
			var contactGroup = new ContactGroup {
				Type = ContactGroupType.General,
				Specialized = true,
				Name = "testGroup",
				ContactGroupOwner = supplier.ContactGroupOwner
			};
			contactGroup.Save();
			user.ContactGroup = contactGroup;
			user.ContactGroup.AddContact(ContactType.Email, email);
			session.SaveOrUpdate(user);

			var user2 = new User(supplier.Payer, supplier);
			user2.Setup();
			session.SaveOrUpdate(user2);

			Flush();
			filter.SearchBy = SearchUserBy.ByContacts;
			filter.SearchText = email;
			var result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].ClientId, Is.EqualTo(supplier.Id));

			var email2 = "testmailQWERTYS2@mail.ru";
			supplier.ContactGroupOwner.ContactGroups[0].AddContact(ContactType.Email, email2);
			Save(supplier);

			Flush();

			filter.SearchText = email2;
			result = filter.Find();
			Assert.That(result.Count, Is.EqualTo(2));

			user.Delete();
			user2.Delete();
			Flush();
		}

		[Test]
		public void Work_Region_Test()
		{
			var client = DataMother.CreateClientAndUsers();
			var newRegion = Region.Queryable.FirstOrDefault(r => r.Id == 2UL);
			if (newRegion != null) {
				client.UpdateRegionSettings(new[] {
					new RegionSettings {
						Id = newRegion.Id,
						IsAvaliableForBrowse = true
					}
				});
				session.SaveOrUpdate(client);
				Flush();
				filter.Region = newRegion;
				filter.ClientType = SearchClientType.Drugstore;
				var result = filter.Find();
				Assert.That(result.Count(r => r.ClientId == client.Id), Is.EqualTo(2));
			}
			else {
				throw new Exception("Не найден альтернативный регион работы");
			}
		}

		private string RandomPhone()
		{
			return String.Format("{0}-{1}", RandomDigits(random, 4).Implode(""), RandomDigits(random, 6).Implode(""));
		}

		private static IEnumerable<string> RandomDigits(Random random, int count)
		{
			return Enumerable.Range(1, count).Select(i => random.Next(0, 9).ToString());
		}
	}
}