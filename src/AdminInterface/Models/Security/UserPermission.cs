using System.ComponentModel;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Security
{
	public enum UserPermissionAvailability
	{
		All = 2,
		Supplier = 0,
		Drugstore = 1,
	}

	public enum UserPermissionTypes
	{
		[Description("Права доступа")] Base = 0,
		AnalitFExcel = 1,
		AnalitFPrint = 2,
		SupplierInterface = 3
	}

	[ActiveRecord("UserPermissions", Schema = "usersettings")]
	public class UserPermission : ActiveRecordBase<UserPermission>
	{
		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public string Shortcut { get; set; }

		[Property]
		public UserPermissionAvailability AvailableFor { get; set; }

		[Property]
		public UserPermissionTypes Type { get; set; }

		[Property]
		public bool AssignDefaultValue { get; set; }

		public static UserPermission[] FindPermissionsAvailableFor(Client client)
		{
			UserPermissionAvailability clientTypeFilter;
			if (client.IsDrugstore())
				clientTypeFilter = UserPermissionAvailability.Drugstore;
			else
				clientTypeFilter = UserPermissionAvailability.Supplier;

			return FindAll(Order.Asc("Name"), Expression.Eq("AvailableFor", UserPermissionAvailability.All)
											  || Expression.Eq("AvailableFor", clientTypeFilter));
		}

		public static UserPermission[] FindPermissionsByType(UserPermissionTypes type)
		{
			return FindAll(Expression.Eq("Type", type));
		}

		public static UserPermission[] GetDefaultPermissions()
		{
			return FindAll(Expression.Eq("AssignDefaultValue", true));
		}
	}
}
