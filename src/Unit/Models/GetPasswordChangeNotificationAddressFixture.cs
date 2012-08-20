using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class GetPasswordChangeNotificationAddressFixture
	{
		private User user;
		private Client client;
		private Supplier supplier;

		[SetUp]
		public void Setup()
		{
			var payer = new Payer();
			client = new Client(payer, new Region()) {
				ContactGroupOwner = new ContactGroupOwner {
					ContactGroups = new List<ContactGroup> {
						new ContactGroup {
							Type = ContactGroupType.General,
							Persons = new List<Person>(),
							Contacts = new List<Contact> {
								new Contact {
									Type = ContactType.Email,
									ContactText = "g1@mail.ru",
								},
								new Contact {
									Type = ContactType.Email,
									ContactText = "g2@mail.ru",
								},
								new Contact {
									Type = ContactType.Phone,
									ContactText = "4732-456213",
								},
							}
						},
						new ContactGroup {
							Type = ContactGroupType.OrderManagers,
							Contacts = new List<Contact>(),
							Persons = new List<Person>(),
						},
						new ContactGroup {
							Type = ContactGroupType.ClientManagers,
							Contacts = new List<Contact>(),
							Persons = new List<Person>(),
						}
					}
				}
			};
			supplier = new Supplier();
			supplier.ContactGroupOwner = client.ContactGroupOwner;
			user = new User(client);
			client.AddUser(user);
		}

		[Test]
		public void GetPasswordChangeNotificationAddressWithoutAddress()
		{
			client.ContactGroupOwner.ContactGroups[0].Contacts.Clear();
			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo(""));
		}

		[Test]
		public void GetPasswordChangeNotificationAddressForDrugstoreTest()
		{
			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru, g2@mail.ru"));

			var orderGroup = client.ContactGroupOwner.ContactGroups[1];

			orderGroup.Contacts = new List<Contact> {
				new Contact {
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact {
					Type = ContactType.Email,
					ContactText = "o2@mail.ru",
				},
				new Contact {
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, o2@mail.ru"));
		}

		[Test]
		public void GetPasswordChangeNotificationAddressForSupplierTest()
		{
			user.RootService = supplier;
			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru, g2@mail.ru"));

			var orderGroup = supplier.ContactGroupOwner.ContactGroups[1];
			var clientGroup = supplier.ContactGroupOwner.ContactGroups[2];

			clientGroup.Contacts = new List<Contact> {
				new Contact {
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact {
					Type = ContactType.Email,
					ContactText = "c2@mail.ru",
				},
				new Contact {
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};


			orderGroup.Contacts = new List<Contact> {
				new Contact {
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact {
					Type = ContactType.Email,
					ContactText = "o2@mail.ru",
				},
				new Contact {
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, o2@mail.ru, c1@mail.ru, c2@mail.ru"));
		}

		[Test]
		public void ReadAddressFromPersons()
		{
			user.RootService = supplier;
			supplier.ContactGroupOwner.ContactGroups[0].Persons.Add(new Person {
				Contacts = new List<Contact> {
					new Contact {
						ContactText = "pg1@mail.ru",
						Type = ContactType.Email,
					}
				},
			});

			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru, g2@mail.ru, pg1@mail.ru"));
		}

		[Test]
		public void SkipEqualsAddress()
		{
			user.RootService = supplier;
			supplier.ContactGroupOwner.ContactGroups[0].Contacts[1].ContactText = "g1@mail.ru";

			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("g1@mail.ru"));

			var orderGroup = supplier.ContactGroupOwner.ContactGroups[1];
			var clientGroup = supplier.ContactGroupOwner.ContactGroups[2];

			clientGroup.Contacts = new List<Contact> {
				new Contact {
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact {
					Type = ContactType.Email,
					ContactText = "c1@mail.ru",
				},
				new Contact {
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};


			orderGroup.Contacts = new List<Contact> {
				new Contact {
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact {
					Type = ContactType.Email,
					ContactText = "o1@mail.ru",
				},
				new Contact {
					Type = ContactType.Phone,
					ContactText = "4732-456213",
				},
			};

			Assert.That(user.GetAddressForSendingClientCard(), Is.EqualTo("o1@mail.ru, c1@mail.ru"));
		}
	}
}