import AdminInterface.Models.Billing
import Castle.ActiveRecord

session = SessionScope()
try:
	for invoice in Invoice.FindAll():
		if not invoice.Parts.Count:
			invoice.Parts = invoice.BuildParts()
			invoice.Save()
ensure:
	session.Dispose()
