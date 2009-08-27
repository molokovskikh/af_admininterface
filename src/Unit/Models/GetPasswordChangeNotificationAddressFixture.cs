using System;
using System.Collections.Generic;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using NUnit.Framework;


namespace Common.Web.Ui.Test
{
	[TestFixture]
	public class GetPasswordChangeNotificationAddressFixture
	{
		private Client _client;

		[SetUp]
		public void Setup()
		{
			_client = new Client
			{
				ContactGroupOwner = new ContactGroupOwner
				{
					ContactGroups = new List<ContactGroup>
					{
						new ContactGroup
						{
							Type = ContactGroupType.General,
							Persons = new List<Person>(),
							Contacts = new List<Contact>
							{
								new Contact
								{
									Type = ContactType.Email,
									ContactText = "g1@mail.ru",
								},
								new Contact
								{
									Type = ContactType.Email,
									ContactText = "g2@mail.ru",
								},
								new Contact
								{
									Type = ContactType.Phone,
									ContactText = "4732-456213",
								},
							}
						},
						new ContactGroup
						{
							Type = ContactGroupType.OrderManagers,
							Contacts = new List<Contact>(),
							Persons = new List<Person>(),
						},
						new ContactGroup
						{
							Type = ContactGroupType.ClientManagers,
							Contacts = new List<Contact>(),
							Persons = new List<Person>(),
						}
					}
				}
			};
		}

		[Test]
		public void GetPasswordChangeNotificationAddressWithoutAddress()
		{
			_client.ContactGroupOwner.ContactGroups[0].Contacts.Clear();
			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo(""));
		}

		[Test]
		public void GetPasswordChangeNotificationAddressForDrugstoreTest()
		{
			_client.Type = ClientType.Drugstore;
			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru, g2@mail.ru"));

			var orderGroup = _client.ContactGroupOwner.ContactGroups[1];

			orderGroup.Contacts = new List<Contact>
			{
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o2@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, o2@mail.ru"));
		}

		[Test]
		public void GetPasswordChangeNotificationAddressForSupplierTest()
		{
			_client.Type = ClientType.Supplier;
			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru, g2@mail.ru"));

			var orderGroup = _client.ContactGroupOwner.ContactGroups[1];
			var clientGroup = _client.ContactGroupOwner.ContactGroups[2];

			clientGroup.Contacts = new List<Contact>
			{
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "c2@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};


			orderGroup.Contacts = new List<Contact>
			{
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o2@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, o2@mail.ru, c1@mail.ru, c2@mail.ru"));
		}

		[Test]
		public void ReadAddressFromPersons()
		{
			_client.Type = ClientType.Supplier;
			_client.ContactGroupOwner.ContactGroups[0].Persons.Add(new Person
			{
				Contacts = new List<Contact>
				{
					new Contact
					{
						ContactText = "pg1@mail.ru",
						Type = ContactType.Email,
					}
				},
			});

			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("pg1@mail.ru, g1@mail.ru, g2@mail.ru"));
		}

		[Test]
		public void SkipEqualsAddress()
		{
			_client.Type = ClientType.Supplier;
			_client.ContactGroupOwner.ContactGroups[0].Contacts[1].ContactText = "g1@mail.ru";

			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru"));

			var orderGroup = _client.ContactGroupOwner.ContactGroups[1];
			var clientGroup = _client.ContactGroupOwner.ContactGroups[2];

			clientGroup.Contacts = new List<Contact>
			{
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};


			orderGroup.Contacts = new List<Contact>
			{
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact
				{
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(_client.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, c1@mail.ru"));
		}
	}
}
