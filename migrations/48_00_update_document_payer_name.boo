import AdminInterface.Models.Billing

for act in Act.FindAll():
	act.PayerName = act.Payer.JuridicalName
	act.Save()

for invoice in Invoice.FindAll():
	invoice.PayerName = invoice.Payer.JuridicalName
	invoice.Save()
