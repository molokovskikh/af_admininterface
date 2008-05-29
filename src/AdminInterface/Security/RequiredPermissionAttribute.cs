using System;
using AdminInterface.Models;

namespace AdminInterface.Security
{
	public class RequiredPermissionAttribute : Attribute
	{
		public RequiredPermissionAttribute(PermissionType permissionType)
		{
			throw new NotImplementedException();
		}
	}
}