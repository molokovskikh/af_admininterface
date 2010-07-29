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
		[Description("Общая информация")] General = 0,
		[Description("Администратор клиентов в автоматизированной системе")] ClientManagers = 1,
		[Description("Менеджер прайс листов(заказов)")] OrderManagers = 2,
		[Description("Бухгалтер по расчетам с АК \"Инфорум\"")] AccountantManagers = 3,
		[Description("Контактная информация для биллинга")] Billing = 4,
		[Description("Известные телефоны")] KnownPhones,
		Custom = 5
	}

	[ActiveRecord("contact_groups", Schema = "contacts")]
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

		[Property, ValidateNonEmpty("Название не может быть пустым")]
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
			if (String.IsNullOrEmpty(name))
				return;
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

		public static void DeletePerson(uint personId)
		{
			var person = Person.Find(personId);
			person.DeleteAndFlush();
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
			// Сливаем в один массив отображаемые контакты (существующие + добавленные) и те, 
			// которые не отображены (могли быть добавлены другими пользователями)
			var contacts = displayedContacts.Concat(hiddenContacts);

            AbstractContactController.UpdateContactForContactOwner(contacts.ToArray(), this);
            Save();
		}

		public void UpdateContacts(Contact[] displayedContacts)
		{
			UpdateContacts(displayedContacts, null);
		}

		public void UpdatePersons(Person[] persons)
		{
			UpdatePersons(persons, null);
		}

		public void UpdatePersons(Person[] displayedPersons, Person[] deletedPersons)
		{
			if (((Persons == null) || (Persons.Count == 0)) && (displayedPersons.Length > 0))
			{
				foreach (var person in displayedPersons)
					AddPerson(person.Name);
				return;
			}
			var hiddenPersons = new List<Person>();
			foreach (var contact in Persons)
			{
				if (deletedPersons != null)
				{
					var deleted = deletedPersons.Where(c => c.Id == contact.Id);
					if (deleted.Count() > 0)
						continue;
				}
				var displayed = displayedPersons.Where(c => c.Id == contact.Id);
				if (displayed.Count() > 0)
					continue;
				hiddenPersons.Add(contact);
			}
			// Сливаем в один массив отображаемые контакты (существующие + добавленные) и те, 
			// которые не отображены (могли быть добавлены другими пользователями)
			var contacts = displayedPersons.Concat(hiddenPersons);

			UpdatePersonForContactOwner(contacts.ToArray(), this);
			Save();
		}

		public static void UpdatePersonForContactOwner(Person[] persons, ContactGroup contactGroup)
		{
			var toRemove = new List<Person>();
			foreach (var existsPerson in contactGroup.Persons)
			{
				var newPerson = FindPerson(persons, existsPerson.Id);
				if (newPerson == null)
					toRemove.Add(existsPerson);
				else if (String.IsNullOrEmpty(newPerson.Name))
					toRemove.Add(existsPerson);
				else if (existsPerson.Name != newPerson.Name)
				{
					existsPerson.Name = newPerson.Name;
				}
			}
			foreach (var newPerson in persons)
				if (newPerson.Id == 0 && !String.IsNullOrEmpty(newPerson.Name))
				{
					newPerson.ContactGroup = contactGroup;
					newPerson.SaveAndFlush();
				}
			foreach (var person in toRemove)
			{
				contactGroup.Persons.Remove(person);
				contactGroup.UpdateAndFlush();
				DeletePerson(person.Id);
			}
		}

		protected static Person FindPerson(IEnumerable<Person> persons, uint personId)
		{
			foreach (var contact in persons)
				if (contact.Id == personId)
					return contact;
			return null;
		}
	}
}
