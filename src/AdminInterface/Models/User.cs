using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.MySql;
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

	[ActiveRecord("future.Users")]
	public class User : ActiveRecordBase<User>
	{ 
		[PrimaryKey("Id")]
		public virtual uint Id { get; set; }

		[Property("Name", NotNull = true)]
		public virtual string Login { get; set; }

		[BelongsTo("ClientCode", NotNull = true)]
		public Client Client { get; set; }

		[HasAndBelongsToMany(typeof (UserPermission),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "usersettings.AssignedPermissions",
			ColumnRef = "PermissionId")]
		public virtual IList<UserPermission> AssignedPermissions { get; set; }

		[HasAndBelongsToMany(typeof (Address),
			Lazy = true,
			ColumnKey = "UserId",
			Table = "future.UserAddress",
			ColumnRef = "AddressId")]
		public virtual IList<Address> AvaliableAddresses { get; set; }

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
				throw new LoginNotFoundException(String.Format("Пользователь {0} не найден", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword("Запрещено изменять пароль для пользователей из офиса.");
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

		public bool IsChangePasswordByOneself()
		{
			var log = PasswordChangeLogEntity.FindFirst(
				DetachedCriteria
					.For<PasswordChangeLogEntity>()
					.Add(Expression.Eq("TargetUserName", Login))
					.AddOrder(Order.Desc("LogTime")));
			if (log == null)
				return false;
			return log.IsChangedByOneSelf();
		}
	}
}