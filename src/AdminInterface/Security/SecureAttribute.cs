using System;
using AdminInterface.Models.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Security
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true), Serializable]
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
