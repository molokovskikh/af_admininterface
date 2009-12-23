using Castle.ActiveRecord;
using System.Collections.Generic;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contacts.contact_owners"), JoinedBase]
	public class ContactOwner : ActiveRecordValidationBase
	{
		private IList<Contact> _contacts;

		[PrimaryKey]
		public uint Id { get; set; }

		[HasMany(typeof(Contact), Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public IList<Contact> Contacts
		{
			get
			{
				
				if (_contacts == null)
					_contacts = new List<Contact>();
				return _contacts;
			}
			set { _contacts = value; }
		}

		public void AddContact(Contact contact)
		{
			contact.ContactOwner = this;
			Contacts.Add(contact);
		}

		public Contact AddContact(ContactType type, string contactText)
		{
			var contact = new Contact(type, contactText);
			contact.ContactOwner = this;
			Contacts.Add(contact);
			return contact;
		}

	}
}
