using System;
using AdminInterface.Models;

namespace AdminInterface.Security
{
	public enum Required
	{
		All,
		AnyOf,
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true), Serializable]
	public class RequiredPermissionAttribute : Attribute
	{
		public RequiredPermissionAttribute(params PermissionType[] permissionTypes)
		{
			PermissionTypes = permissionTypes;
			Required = Required.All;
		}

		public PermissionType[] PermissionTypes { get; set; }

		public Required Required { get; set; }
	}
}