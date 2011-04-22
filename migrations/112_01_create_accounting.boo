import AdminInterface.Models.Billing
import AdminInterface.Models

for user in User.FindAll():
	continue if user.Accounting
	user.Accounting = UserAccounting(user)
	user.Save()
