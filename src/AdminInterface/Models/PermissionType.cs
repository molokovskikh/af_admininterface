namespace AdminInterface.Models
{
	public enum PermissionType
	{
		ViewDrugstore = 1,
		ManageDrugstore = 2,
		ManageAdministrators = 3,
		DrugstoreInterface = 12,

		ViewSuppliers = 4,
		ManageSuppliers = 5,
		RegisterSupplier = 6,
		SupplierInterface = 11,

		RegisterDrugstore = 7,
		MonitorUpdates = 8,
		BillingPermision = 9,
		CopySynonyms = 10,
		ChangePassword = 13,
		CreateInvisible = 14,
	}
}
