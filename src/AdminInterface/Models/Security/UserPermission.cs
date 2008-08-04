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

	[ActiveRecord("usersettings.UserPermissions")]
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

		public static UserPermission[] FindPermissionsAvailableFor(Client client)
		{
			UserPermissionAvailability clientTypeFilter;
			if (client.Type == ClientType.Drugstore)
				clientTypeFilter = UserPermissionAvailability.Drugstore;
			else
				clientTypeFilter = UserPermissionAvailability.Supplier;

			return FindAll(Order.Asc("Name"), Expression.Eq("AvailableFor", UserPermissionAvailability.All)
											  || Expression.Eq("AvailableFor", clientTypeFilter));
		}
	}
}
