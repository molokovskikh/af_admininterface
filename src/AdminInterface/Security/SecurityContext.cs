using System;
using System.Web;
using AdminInterface.Models.Security;

namespace AdminInterface.Security
{
	public static class SecurityContext
	{
		private const string AdministratorKey = "AdminInterface.Security.Admin";

		public static Func<Administrator> GetAdministrator = () => {
			var httpContext = HttpContext.Current;
			var admin = (Administrator)httpContext.Items[AdministratorKey];
#if !DEBUG
			administrator = httpContext.Session[AdministratorKey];
#endif
			if (admin == null)
			{
				admin = Administrator.GetByName(httpContext.User.Identity.Name);
				if (admin != null)
				{
					httpContext.Session["UserName"] = admin.UserName;
					httpContext.Session[AdministratorKey] = admin;
					httpContext.Items[AdministratorKey] = admin;
				}
			}

			return admin;
		};

		public static Administrator Administrator
		{
			get
			{
				return GetAdministrator();
			}
		}

		public static void CheckIsUserAuthorized()
		{
			if (Administrator == null)
				throw new NotAuthorizedException();
		}
	}
}