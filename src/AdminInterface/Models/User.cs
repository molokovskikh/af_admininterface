using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	public class LoginNotFoundException : Exception
	{
		public LoginNotFoundException(string message) : base(message) { }
	}

	public class CantChangePassword : Exception
	{
		public CantChangePassword(string message) : base(message) { }
	}

	[ActiveRecord("usersettings.OsUserAccessRight")]
	public class User : ActiveRecordBase<User>
	{ 
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[Property("OsUserName", NotNull = true)]
		public virtual string Login { get; set; }

		[BelongsTo("ClientCode", NotNull = true)]
		public Client Client { get; set; }

		[HasAndBelongsToMany(typeof (UserPermission),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "usersettings.AssignedPermissions",
			ColumnRef = "PermissionId")]
		public virtual IList<UserPermission> AssignedPermissions { get; set; }

		public static User GetByLogin(string login)
		{
			return ActiveRecordMediator<User>.FindOne(Expression.Eq("Login", login));
		}

		public static User GetById(uint id)
		{
			return ActiveRecordMediator<User>.FindByPrimaryKey(id);
		}

		public void CheckLogin()
		{
			if (!ADHelper.IsLoginExists(Login))
				throw new LoginNotFoundException(String.Format("”четна€ запись {0} не найдена", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword(String.Format("Ќе возможно изменить пароль дл€ учетной записи {0} поскольку она принадлежит пользователю из офиса", Login));
		}

		public override string ToString()
		{
			return Login;
		}

		public bool IsPermissionAssigned(UserPermission permission)
		{
			foreach (var userPermission in AssignedPermissions)
				if (permission.Shortcut == userPermission.Shortcut)
					return true;
			return false;
		}
	}
}