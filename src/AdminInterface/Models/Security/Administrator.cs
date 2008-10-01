using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Security
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

		[Property]
		public string ManagerName { get; set; }

		[Property]
		public string PhoneSupport { get; set; }

		[HasAndBelongsToMany(typeof(Permission), 
			RelationType.Bag, 
			Table = "accessright.AdminsPermissions", 
			ColumnKey = "AdminId", 
			ColumnRef = "PermissionId", 
			Lazy = false)]
		public IList<Permission> AllowedPermissions { get; set; }

		public static Administrator GetByName(string name)
		{
			return ActiveRecordMediator<Administrator>.FindOne(Expression.Eq("UserName",
			                                                                 name.Replace("ANALIT\\", "")));
		}

		public static Administrator GetById(uint id)
		{
			return ActiveRecordMediator<Administrator>.FindByPrimaryKey(id);
		}

		public static IList<Administrator> FindAll()
		{
			return ActiveRecordMediator<Administrator>.FindAll(new[] { Order.Asc("UserName") });
		}

		public void Delete()
		{
			ActiveRecordMediator<Administrator>.Delete(this);
		}

		public void Save()
		{
			ActiveRecordMediator<Administrator>.Save(this);
		}

		public void Update()
		{
			ActiveRecordMediator<Administrator>.Update(this);
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

		public string GetClientFilterByType(string alias)
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

		public Administrator CheckClientType(ClientType clientType)
		{
			if (clientType == ClientType.Drugstore && !HavePermisions(PermissionType.ViewDrugstore))
				throw new NotHavePermissionException();
			if (clientType == ClientType.Supplier && !HavePermisions(PermissionType.ViewSuppliers))
				throw new NotHavePermissionException();

			return this;
		}

		public Administrator CheckClientHomeRegion(ulong homeRegionId)
		{
			if ((homeRegionId & RegionMask) == 0)
				throw new NotHavePermissionException();

			return this;
		}

		public bool AlowChangePassword
		{
			get { return HavePermision(PermissionType.ChangePassword); }
		}

		public bool CanRegisterClientWhoWorkForFree
		{
			get { return HavePermisions(PermissionType.CanRegisterClientWhoWorkForFree);  }
		}

		public bool ManageDrugstore
		{
			get { return HavePermisions(PermissionType.ManageDrugstore); }
		}

		public bool ManageSuppliers
		{
			get { return HavePermisions(PermissionType.ManageSuppliers); }
		}

		public bool CanViewBilling
		{
			get { return HavePermisions(PermissionType.Billing); }
		}

		public bool CanViewDrugstoreInterface
		{
			get { return HavePermisions(PermissionType.DrugstoreInterface); }
		}

		public bool CanViewSupplierInterface
		{
			get { return HavePermisions(PermissionType.SupplierInterface); }
		}

		public bool CreateUserInAd(string password)
		{
			var isLoginExists = ADHelper.IsLoginExists(UserName);

			var adminGroupPath = "LDAP://CN=Региональные администраторы,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net";
			if (isLoginExists)
			{
				var entry = ADHelper.FindDirectoryEntry(UserName);

				var member = entry.Properties["memberOf"]
					.OfType<string>()
					.FirstOrDefault(mebmer => mebmer.Equals(adminGroupPath));

				if (String.IsNullOrEmpty(member))
				{
					var adminGroup = new DirectoryEntry(adminGroupPath);
					adminGroup.Invoke("Add", entry.Path);
					adminGroup.CommitChanges();
					entry.CommitChanges();
				}
				return false;
			}
//#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Региональные администраторы,OU=Управляющие,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Пользователи офиса,OU=Уровни доступа,OU=Офис,DC=adc,DC=analit,DC=net");
			var adminGroup1 = new DirectoryEntry(adminGroupPath);
			var user = root.Children.Add("CN=" + UserName, "user");
			user.Properties["samAccountName"].Value = UserName;
			if (!String.IsNullOrEmpty(ManagerName.Trim()))
				user.Properties["sn"].Value = ManagerName;
			user.Properties["logonHours"].Value = ADHelper.LogonHours();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.CommitChanges();

			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();

			adminGroup1.Invoke("Add", user.Path);
			adminGroup1.CommitChanges();

			root.CommitChanges();
//#endif
			return true;
		}

		public void CheckClientPermission(Client client)
		{
			CheckClientHomeRegion(client.HomeRegion.Id);
			CheckClientType(client.Type);
		}
	}
}