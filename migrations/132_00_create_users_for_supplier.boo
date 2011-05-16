import System.Security.Principal
import Common.Tools
import Castle.ActiveRecord
import AdminInterface.Models
import AdminInterface.Models.Logs
import AdminInterface.Models.Security
import AdminInterface.Models.Suppliers
import AdminInterface.Security

admin = Administrator.GetByName("kvasov")
SecurityContext.GetAdministrator = {return admin}

token = Win32.LogonUser("kvasovsam", "gjpjkjxtyysq", "analit")
using WindowsIdentity.Impersonate(token):
	for supplier in ActiveRecordMediator[of Supplier].FindAll():
		continue if supplier.Users.Count > 0
		user = User(supplier.Payer, supplier)
		user.Setup()
		user.Save()
		user.CreateInAd()
		ClientInfoLogEntity("служебный логин").Save()
