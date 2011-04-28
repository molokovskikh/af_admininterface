using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Models.Security
{
	public enum Department
	{
		[Description("����������")] Administration = 0,
		[Description("�����������")] Billing = 1,
		[Description("IT")] IT = 2,
		[Description("���������")] Processing = 3,
		[Description("������������")] Support = 4,
		[Description("����� ������������� ��������")] Manager = 5,
	}

	[ActiveRecord("Regionaladmins", Schema = "accessright", Lazy = false)]
	public class Administrator
	{
		public static Func<string> GetHost = () => HttpContext.Current.Request.UserHostAddress;

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

		public string Name
		{
			get { return ManagerName; }
		}

		[Property]
		public string PhoneSupport { get; set; }

		[Property]
		public string InternalPhone { get; set; }

		[Property]
		public Department Department { get; set; }

		[HasAndBelongsToMany(typeof(Permission), 
			Table = "AdminsPermissions", 
			Schema = "accessright",
			ColumnKey = "AdminId", 
			ColumnRef = "PermissionId", 
			Lazy = true)]
		public IList<Permission> AllowedPermissions { get; set; }

		public static Administrator GetByName(string name)
		{
			name = name.Replace("ANALIT\\", "");
			var admin = ActiveRecordLinq.AsQueryable<Administrator>().FirstOrDefault(a => a.UserName == name);
			if (admin != null)
				NHibernateUtil.Initialize(admin.AllowedPermissions);

			return admin;
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
			var user = RedmineUser.Queryable.Where(r => r.Login == UserName).FirstOrDefault();
			if (user != null)
			{
				user.Status = 3;
				user.Update();
			}
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

		public bool HavePermision(PermissionType permission)
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
			if (permissions.Any(p => !HavePermision(p)))
				throw new NotHavePermissionException();
		}

		public void CheckAnyOfPermissions(params PermissionType[] permissions)
		{
			if (permissions.Any(HavePermision))
				return;

			throw new NotHavePermissionException();
		}

		public Administrator CheckClientType(ServiceType clientType)
		{
			if (clientType == ServiceType.Drugstore && !HavePermisions(PermissionType.ViewDrugstore))
				throw new NotHavePermissionException();
			if (clientType == ServiceType.Supplier && !HavePermisions(PermissionType.ViewSuppliers))
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
			var entry = ADHelper.FindDirectoryEntry(UserName);

			var adminGroupPath = "LDAP://CN=������������ ��������������,OU=������,OU=�������,DC=adc,DC=analit,DC=net";
			var root = new DirectoryEntry("LDAP://OU=����,DC=adc,DC=analit,DC=net");

			if (entry != null)
			{
				entry.Properties["userAccountControl"][0] = AccountControl.NormalAccount;
				// ���������� pwdLastSet � ������� ����
				entry.Properties["pwdLastSet"][0] = -1;
				// ������� ������
				entry.Invoke("SetPassword", password);
				entry.CommitChanges();

				var member = entry.Properties["memberOf"]
					.OfType<string>()
					.FirstOrDefault(mebmer => mebmer.Equals(adminGroupPath));

				if (String.IsNullOrEmpty(member))
				{
					var adminGroup = new DirectoryEntry(adminGroupPath);
					adminGroup.Invoke("Add", entry.Path);
					adminGroup.CommitChanges();
				}
				entry.MoveTo(root);
				entry.CommitChanges();
				return false;
			}

			var userGroup = new DirectoryEntry("LDAP://CN=������������ �����,OU=������ �������,OU=����,DC=adc,DC=analit,DC=net");
			var adminGroup1 = new DirectoryEntry(adminGroupPath);
			var user = root.Children.Add("CN=" + UserName, "user");
			user.Properties["samAccountName"].Value = UserName;
			if (!String.IsNullOrEmpty(ManagerName.Trim()))
				user.Properties["sn"].Value = ManagerName;
			user.Properties["logonHours"].Value = ADHelper.LogonHours();
			user.CommitChanges();
			user.Properties["userAccountControl"][0] = AccountControl.NormalAccount;
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.CommitChanges();

			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();

			adminGroup1.Invoke("Add", user.Path);
			adminGroup1.CommitChanges();

			root.CommitChanges();
			return true;
		}

		public void CheckClientPermission(Client client)
		{
			CheckClientHomeRegion(client.HomeRegion.Id);
			CheckClientType(client.Type);
		}

		public override string ToString()
		{
			return UserName;
		}

		public string Host
		{
			get { return GetHost(); }
		}

		public static void SetLogonHours(string login, bool[] weekLogonHours)
		{
			// ������ �������������� ������� 7x24
			var logonHours = new bool[7, 24];
			for (var i = 0; i < 7; i++)
			{
				var index = 0;
				for (var j = 0; j < 24; j += 2)
				{
					logonHours[i, j] = weekLogonHours[i * 12 + index];
					logonHours[i, j + 1] = weekLogonHours[i * 12 + index];
					index++;
				}
			}
			ADHelper.SetLogonHours(login, logonHours);
		}

		public bool HaveAccessTo(string controller, string action)
		{
			return AllowedPermissions.Any(p => p.HaveAccessTo(controller, action));
		}
	}
}