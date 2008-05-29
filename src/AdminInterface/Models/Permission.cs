using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

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

		[Property]
		public string Shortcut { get; set; }

		public static IList<Permission> FindAll()
		{
			return ActiveRecordMediator<Permission>.FindAll(new [] { Order.Asc("Name") });
		}
	}
}
