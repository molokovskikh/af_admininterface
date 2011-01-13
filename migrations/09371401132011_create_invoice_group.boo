import System.Collections.Generic
import System.Linq.Enumerable
import AdminInterface.Models
import AdminInterface.Models.Billing
import Common.Web.Ui.Models

for payer in Payer.FindAll():
	continue unless payer.Addresses.Count

	invoiceGroup = payer.ContactGroupOwner.ContactGroups.FirstOrDefault({g| g.Type == ContactGroupType.Invoice})
	continue if invoiceGroup
	mails = List of string()
	for group in payer.ContactGroupOwner.ContactGroups:
		for contact in group.Contacts:
			if contact.Type == ContactType.Email:
				mails.Add(contact.ContactText) 
	
	invoiceGroup = payer.ContactGroupOwner.AddContactGroup(ContactGroupType.Invoice)
	for mail in mails:
		invoiceGroup.AddContact(ContactType.Email, mail)
	payer.ContactGroupOwner.Save()
