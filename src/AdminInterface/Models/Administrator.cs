using System;
using System.Collections.Generic;
using AdminInterface.Filters;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	[ActiveRecord("accessright.regionaladmins", Lazy = false)]
	public class Administrator
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string Email { get; set; }

		[Property]
		public ulong RegionMask { get; set; }

		[HasAndBelongsToMany(typeof(Permission), RelationType.Bag, Table = "accessright.AdminsPermissions", ColumnKey = "AdminId", ColumnRef = "PermissionId", Lazy = false)]
		public IList<Permission> AllowedPermissions { get; set; }

		public static Administrator GetByName(string name)
		{
			return ActiveRecordMediator<Administrator>.FindOne(Expression.Eq("UserName", name.Replace("ANALIT\\", "")));
		}

		private bool HavePermision(PermissionType permission)
		{
			foreach (var allowedPermission in AllowedPermissions)
				if (allowedPermission.Type == permission)
					return true;
			return false;
		}

		public bool HavePermisions(params PermissionType[] permissions)
		{
			foreach (var permission in permissions)
				if (!HavePermision(permission))
					return false;
			return true;
		}

		public bool HaveAnyOfPermissions(params PermissionType[] permissions)
		{
			foreach (var permission in permissions)
				if (HavePermision(permission))
					return true;
			return false;

		}

		public string ClientTypeFilter(string alias)
		{
			var allowViewSuppliers = HavePermisions(PermissionType.ViewSuppliers);
			var allowViewDrugstore = HavePermisions(PermissionType.ViewDrugstore);

			if (allowViewDrugstore && allowViewSuppliers)
				return " ";

			if (!String.IsNullOrEmpty(alias))
				alias = alias + ".";

			if (allowViewDrugstore)
				return String.Format(" and {0}FirmType = 1 ", alias);

			if (allowViewSuppliers)
				return String.Format(" and {0}FirmType = 0 ", alias);

			throw new NotHavePermissionException();
		}

		public void CheckPermisions(params PermissionType[] permissions)
		{
			foreach (var permission in permissions)
				if (!HavePermision(permission))
					throw new NotHavePermissionException();
		}

		public void CheckAnyOfPermissions(params PermissionType[] permissions)
		{
			foreach (var permission in permissions)
				if (HavePermision(permission))
					return;

			throw new NotHavePermissionException();
		}

		public void CheckClientType(ClientType clientType)
		{
			if (clientType == ClientType.Drugstore && !HavePermisions(PermissionType.ViewDrugstore))
			    throw new NotHavePermissionException();
			if (clientType == ClientType.Supplier && !HavePermisions(PermissionType.ViewSuppliers))
				throw new NotHavePermissionException();
		}

		public void CheckClientHomeRegion(ulong homeRegionId)
		{
			if ((homeRegionId & RegionMask) == 0)
		        throw new NotHavePermissionException();
		}

		public bool AlowChangePassword
		{
			get { return HavePermision(PermissionType.ChangePassword); }
		}
	}
}
