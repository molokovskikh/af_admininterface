namespace AdminInterface.Models
{
	public enum PermissionType
	{
		ViewDrugstore = 1,
		ManageDrugstore = 2,
		RegisterDrugstore = 7,
		DrugstoreInterface = 12,

		ViewSuppliers = 4,
		ManageSuppliers = 5,
		RegisterSupplier = 6,
		SupplierInterface = 11,

		ManageAdministrators = 3,
		MonitorUpdates = 8,
		Billing = 9,
		CopySynonyms = 10,
		ChangePassword = 13,
		RegisterInvisible = 14,
		SendNotification = 15,
	}
}
