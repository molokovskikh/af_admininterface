import AdminInterface.Models
import AdminInterface.Models.Billing

for user in User.FindAll():
	continue if user.Accounting
	user.Accounting = UserAccounting(user)
	user.Save()