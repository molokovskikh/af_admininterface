import System
import System.Collections.Generic
import System.Linq.Enumerable from System.Core

import Castle.Core from "lib/Castle/Castle.Core.dll"
import Castle.DynamicProxy from "lib/Castle/Castle.DynamicProxy2.dll"
import Castle.ActiveRecord from "lib/Castle/Castle.ActiveRecord.dll"
import AdminInterface.Models from "src/AdminInterface/bin/AdminInterface.dll"
import AdminInterface.Models.Billing


for address in List[of Address](Address.FindAll()).Where({a as Address| not a.Accounting}):
	address.Accounting = AddressAccounting(address)
	address.Save()
	print "${address.Value} accounted"

for user in List[of User](User.FindAll()).Where({u as User| not u.Accounting}):
	user.Accounting = UserAccounting(user)
	user.Save()
	print "${user.Login} accounted"