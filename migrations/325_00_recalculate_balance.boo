import AdminInterface.Models.Billing

for payer in Payer.FindAll():
	payer.RecalculateBalance()
	payer.Save()