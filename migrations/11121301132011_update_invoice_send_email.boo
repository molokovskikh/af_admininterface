import AdminInterface.Models.Billing

for invoice in Invoice.FindAll():
	if invoice.Payer.InvoiceSettings.EmailInvoice:
		invoice.SendToEmail = true
		invoice.Update()
