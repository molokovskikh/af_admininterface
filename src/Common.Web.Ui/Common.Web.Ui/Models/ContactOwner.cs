using Castle.ActiveRecord;
using System.Collections.Generic;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contact_owners", Schema = "contacts"), JoinedBase]
	public class ContactOwner : ActiveRecordValidationBase
	{
		private IList<Contact> _contacts;

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[HasMany(typeof(Contact), Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public virtual IList<Contact> Contacts
		{
			get
			{
				
				if (_contacts == null)
					_contacts = new List<Contact>();
				return _contacts;
			}
			set { _contacts = value; }
		}

		public virtual void AddContact(Contact contact)
		{
			contact.ContactOwner = this;
			Contacts.Add(contact);
		}

		public virtual Contact AddContact(ContactType type, string contactText)
		{
			var contact = new Contact(type, contactText);
			contact.ContactOwner = this;
			Contacts.Add(contact);
			return contact;
		}

	}
}
