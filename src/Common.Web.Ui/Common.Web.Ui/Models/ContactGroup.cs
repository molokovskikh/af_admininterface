using System;
using System.Collections;
using System.Linq;
using Castle.ActiveRecord;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.Components.Validator;
using Common.Web.Ui.Controllers;

namespace Common.Web.Ui.Models
{
	public enum ContactGroupType
	{
		[Description("����� ����������")] General = 0,
		[Description("������������� �������� � ������������������ �������")] ClientManagers = 1,
		[Description("�������� ����� ������(�������)")] OrderManagers = 2,
		[Description("��������� �� �������� � �� \"�������\"")] AccountantManagers = 3,
		[Description("���������� ���������� ��� ��������")] Billing = 4,
		[Description("��������� ��������")] KnownPhones,
		Custom = 5
	}

	[ActiveRecord("contacts.contact_groups")]
	public class ContactGroup : ContactOwner
	{
		public ContactGroup()
		{}

		public ContactGroup(ContactGroupType type, string name)
		{
			Type = type;
			Name = name;
		}

		[JoinedKey("Id")]
		protected uint ContactGroupId { get; set; }

		[Property, ValidateNonEmpty("�������� �� ����� ���� ������")]
		public string Name { get; set; }

		[Property]
		public ContactGroupType Type { get; set; }

		[HasMany(ColumnKey = "ContactGroupId", Lazy = true, Inverse = true, OrderBy = "Name")]
		public IList<Person> Persons { get; set; }

		[BelongsTo(Column = "ContactGroupOwnerId")]
		public ContactGroupOwner ContactGroupOwner { get; set; }

		[Property]
		public bool Specialized { get; set; }

		public void AddPerson(string name)
		{
			if (Persons == null)
				Persons = new List<Person>();
			var person = new Person {
				Name = name,
				ContactGroup = this
			};
			person.Save();
			Persons.Add(person);
			Save();
		}

		public bool ShowMailingAddress
		{
			get { return Type == ContactGroupType.Billing; }
		}

		public static ContactGroup Find(uint id)
		{
			return (ContactGroup)FindByPrimaryKey(typeof (ContactGroup), id);
		}

		public void UpdateContacts(Contact[] displayedContacts, Contact[] deletedContacts)
		{
			var hiddenContacts = new List<Contact>();
			foreach (var contact in Contacts)
			{
				if (deletedContacts != null)
				{
					var deleted = deletedContacts.Where(c => c.Id == contact.Id);
					if (deleted.Count() > 0)
						continue;
				}
				var displayed = displayedContacts.Where(c => c.Id == contact.Id);
				if (displayed.Count() > 0)
					continue;
				hiddenContacts.Add(contact);
			}
			// ������� � ���� ������ ������������ �������� (������������ + �����������) � ��, 
			// ������� �� ���������� (����� ���� ��������� ������� ��������������)
			var contacts = displayedContacts.Concat(hiddenContacts);

            AbstractContactController.UpdateContactForContactOwner(contacts.ToArray(), this);
            Save();
		}

		public void UpdateContacts(Contact[] displayedContacts)
		{
			UpdateContacts(displayedContacts, null);
		}

		public void UpdatePersons(Person[] newPersons)
		{
			for (var i = 0; i < Persons.Count; i++)
			{
				for (var j = 0; j < newPersons.Length; j++)
				{
					if ((Persons[i].Id == newPersons[i].Id) &&
						(!Persons[i].Name.Equals(newPersons[j].Name)) &&
						(!String.IsNullOrEmpty(newPersons[j].Name)))
					{
						Persons[i].Name = newPersons[j].Name;
						Persons[i].Update();
					}
				}
			}
		}
	}
}
