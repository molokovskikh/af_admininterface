import System
import System.Linq.Enumerable
import AdminInterface.Models.Suppliers
import Common.Web.Ui.Models

for supplier in Supplier.Queryable.ToList():
	if supplier.Payer and (supplier.Payer.Id == 921) and not supplier.ContactGroupOwner.HaveGroup(ContactGroupType.MiniMails):
		print "${supplier.Id} ${supplier.Name}"
		miniMailsGroup = supplier.ContactGroupOwner.AddContactGroup(ContactGroupType.MiniMails, false)
		supplier.ContactGroupOwner.Save()