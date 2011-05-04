import System.Security.Principal
import Castle.ActiveRecord
import Common.Tools
import AdminInterface.Models.Suppliers

token = Win32.LogonUser("kvasovsam", "gjpjkjxtyysq", "analit")
using WindowsIdentity.Impersonate(token):
		for supplier in ActiveRecordMediator[of Supplier].FindAll():
			print supplier.Id
			supplier.CreateDirs()
