import System.IO
import Common.Tools
import System.Security.Principal
import AdminInterface.Models

token = Win32.LogonUser("kvasovsam", "gjpjkjxtyysq", "analit")
using WindowsIdentity.Impersonate(token):
	for address in Address.FindAll():
		if not Directory.Exists(Path.Combine("""\\adc.analit.net\Inforoom\AptBox\""", address.Id.ToString())):
			address.CreateFtpDirectory("""\\adc.analit.net\Inforoom\AptBox\""")
