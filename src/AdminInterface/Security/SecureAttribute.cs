using AdminInterface.Models.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Security
{
	public class SecureAttribute : FilterAttribute
	{
		public SecureAttribute(params PermissionType[] permissionTypes)
			: base(ExecuteWhen.BeforeAction, typeof(SecurityFilter))
		{
			PermissionTypes = permissionTypes;
		}

		public PermissionType[] PermissionTypes { get; set; }

		public Required Required { get; set; }
	}
}
