using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Helpers
{
	public class ContactInfo
	{
		public string ContactText { get; set; }
		public ContactType Type { get; set; }
		public bool Deleted { get; set; }
		public int Id { get; set; }
	}

	public class ContactHelper
	{
		public static void UpdateContacts(ContactGroup contactGroup, ContactInfo[] contacts)
		{
			var existsContacts = new List<Contact>();

			foreach (var contact in contactGroup.Contacts)
				existsContacts.Add(contact);

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				foreach (var existsContact in existsContacts)
				{
					var deleted = contacts.Where(contact => (existsContact.Id == contact.Id) && (contact.Deleted));
					if ((deleted != null) && (deleted.Count() > 0))
					{
						var tempGroup = existsContact.ContactOwner;
						tempGroup.Contacts.Remove(existsContact);
					}
				}

				foreach (var contact in contacts)
				{
					if (contact.Id < 0)
					{
						if (!String.IsNullOrEmpty(contact.ContactText))
						{
							var newContact = contactGroup.AddContact(contact.Type, contact.ContactText);
							newContact.Save();
						}
					}
					else
					{
						var editContacts = existsContacts.Where(existsContact => existsContact.Id == contact.Id);
						if ((editContacts == null) || (editContacts.Count() == 0))
							continue;
						if (!String.Equals(contact.ContactText, editContacts.First().ContactText))
						{
							editContacts.First().ContactText = contact.ContactText;
							editContacts.First().Save();
						}
					}
				}
				scope.VoteCommit();
			}
		}
	}
}
