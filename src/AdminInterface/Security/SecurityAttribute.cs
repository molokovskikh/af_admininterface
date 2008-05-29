using AdminInterface.Models;
using Castle.MonoRail.Framework;

namespace AdminInterface.Security
{
	public class SecurityAttribute : FilterAttribute
	{
		public SecurityAttribute(params PermissionType[] permissionTypes)
			: base(ExecuteWhen.BeforeAction, typeof(SecurityFilter))
		{
			PermissionTypes = permissionTypes;
		}

		public PermissionType[] PermissionTypes { get; set; }

		public Required Required { get; set; }
	}
}
