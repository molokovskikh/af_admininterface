import AdminInterface.Models

for payer in Payer.FindAll():
	payer.RecalculateBalance()
	payer.Save()
