import AdminInterface.Models.Billing

for act in Act.FindAll():
	act.CalculateSum()
	act.Save()
