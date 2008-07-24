using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Test.Helpers;
using NUnit.Framework;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class UserFixture
	{
		[Test]
		[ExpectedException(typeof(CantChangePassword), ExpectedMessage = "Не возможно изменить пароль для учетной записи test546116879 поскольку она принадлежит пользователю из офиса")]
		public void Throw_cant_change_password_exception_if_user_from_office()
		{
			using (var testUser = new TestADUser("test546116879", "LDAP://OU=Офис,DC=adc,DC=analit,DC=net"))
			{
				var user = new User { Login = testUser.Login };
				user.CheckLogin();				
			}
		}

		[Test]
		[ExpectedException(typeof(LoginNotFoundException), ExpectedMessage = "Учетная запись test546116879 не найдена")]
		public void Throw_not_found_exception_if_login_not_exists()
		{
			var user = new User { Login = "test546116879" };
			ADHelper.Delete(user.Login);
			user.CheckLogin();
		}
	}
}
