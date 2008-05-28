using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("accessright.permissions", Lazy = false)]
	public class Permission
	{
		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public PermissionType Type { get; set; }
	}
}
