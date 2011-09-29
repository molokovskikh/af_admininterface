import AdminInterface.Models.Billing

for payer in Payer.FindAll():
	payer.UpdatePaymentSum()
	payer.Save()
