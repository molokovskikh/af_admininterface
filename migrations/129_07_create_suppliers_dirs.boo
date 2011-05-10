import System.IO
import Common.Tools
import System.Security.Principal
import Castle.ActiveRecord
import AdminInterface.Models
import AdminInterface.Models.Suppliers

token = Win32.LogonUser("kvasovsam", "gjpjkjxtyysq", "analit")
using WindowsIdentity.Impersonate(token):
	for supplier in ActiveRecordMediator[of Supplier].FindAll():
		supplier.CreateDirs("""\\adc.analit.net\Inforoom\FTP\OptBox\""")
