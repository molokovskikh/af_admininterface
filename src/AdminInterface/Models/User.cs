using System;
using AdminInterface.Helpers;
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
	public class User
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[Property("OsUserName", NotNull = true)]
		public virtual string Login { get; set; }

		[BelongsTo("ClientCode", NotNull = true)]
		public Client Client { get; set; }

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
				throw new LoginNotFoundException(String.Format("������� ������ {0} �� �������", Login));

			if (ADHelper.IsBelongsToOfficeContainer(Login))
				throw new CantChangePassword(String.Format("�� �������� �������� ������ ��� ������� ������ {0} ��������� ��� ����������� ������������ �� �����", Login));
		}

		public DateTime? GetLastLogonIntoClientInterface()
		{
			return null;
/*
			var query = new ScalarQuery<DateTime?>(typeof (User),
			                                       QueryLanguage.Sql,
												   "select max(logtime) from logs.internetlog where username like :login or username like :loginWithAnalitPrefix");
			query.SetParameter("login", Login);
			query.SetParameter("loginWithAnalitPrefix", "Analit\\" + Login);
			return query.Execute();
*/
		}

		public override string ToString()
		{
			return Login;
		}
	}
}