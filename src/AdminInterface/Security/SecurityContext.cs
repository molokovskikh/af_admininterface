using System;
using System.Web;
using AdminInterface.Models;

namespace AdminInterface.Security
{
	public static class SecurityContext
	{
		private const string AdministratorKey = "Admin";

		[ThreadStatic] 
		private static Administrator _administrator;

		public static Administrator Administrator
		{
			get
			{
				if (_administrator != null)
					return _administrator;
				Administrator administrator = null;
#if !DEBUG
				var administrator = (Administrator)HttpContext.Current.Session[AdministratorKey];
#endif
				if (administrator == null)
				{
					administrator = Administrator.GetByName(HttpContext.Current.User.Identity.Name);
#if !DEBUG
					HttpContext.Current.Session[AdministratorKey] = administrator;
#endif
				}

				_administrator = administrator;
				return _administrator;
			}
		}

		public static void CheckIsUserAuthorized()
		{
			if (Administrator == null)
				throw new NotAuthorizedException();
		}
	}
}