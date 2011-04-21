import System
import System.Linq.Enumerable
import AdminInterface.Models.Billing

count = 0
for invoice in Invoice.FindAll().Where({i| i.Period == Period.May and i.Recipient.Id == 2}):
	continue unless invoice.Payer.Clients.Any({c| c.HomeRegion.Id == cast(UInt64, 32)})
	count++
	invoice.Delete()

print count
