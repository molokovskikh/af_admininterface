using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Security
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
		CanRegisterClientWhoWorkForFree = 16,
	}

	[ActiveRecord("accessright.permissions", Lazy = false)]
	public class Permission
	{
		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public PermissionType Type { get; set; }

		[Property]
		public string Shortcut { get; set; }

		public static IList<Permission> FindAll()
		{
			return ActiveRecordMediator<Permission>.FindAll(new [] { Order.Asc("Name") });
		}
	}
}