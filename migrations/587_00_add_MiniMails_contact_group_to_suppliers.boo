import Common.Tools
import System.Security.Principal
import Castle.ActiveRecord
import AdminInterface.Models.Suppliers
import Common.Web.Ui.Models


for supplier in ActiveRecordMediator[of Supplier].FindAll():
	if supplier.Payer and not supplier.ContactGroupOwner.HaveGroup(ContactGroupType.MiniMails):
		print "${supplier.Id} ${supplier.Name}"
		supplier.ContactGroupOwner.AddContactGroup(ContactGroupType.MiniMails, false)
		supplier.ContactGroupOwner.Save()
