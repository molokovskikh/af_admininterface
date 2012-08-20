using System.ComponentModel;
using System.Linq;
using Castle.ActiveRecord;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace AdminInterface.Models.Security
{
	public enum UserPermissionAvailability
	{
		Supplier = 2,
		Drugstore = 1,
	}

	public enum UserPermissionTypes
	{
		[Description("Права доступа")] Base = 0,
		AnalitFExcel = 1,
		AnalitFPrint = 2,
		SupplierInterface = 3
	}

	[ActiveRecord(Schema = "usersettings")]
	public class UserPermission
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

		public static UserPermission[] DefaultPermissions(ISession session, UserPermissionAvailability availability)
		{
			return session.Query<UserPermission>()
				.Where(p => p.AvailableFor == availability && p.AssignDefaultValue)
				.ToArray();
		}

		public static UserPermission[] FindPermissionsByType(ISession session, UserPermissionTypes type)
		{
			return session.Query<UserPermission>()
				.Where(p => p.Type == type)
				.ToArray();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}