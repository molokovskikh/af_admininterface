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
			if (httpContext == null)
				throw new Exception("HttpContext не инициализирован");

			var admin = (Administrator)httpContext.Items[AdministratorKey];
			if (admin == null) {
				var username = httpContext.User.Identity.Name;
#if DEBUG
				if (String.IsNullOrEmpty(username))
					username = Environment.UserName;
#endif
				admin = Administrator.GetByName(username);
				if (admin != null)
					httpContext.Items[AdministratorKey] = admin;
			}

			return admin;
		};

		public static Administrator Administrator
		{
			get
			{
				var admin = GetAdministrator();
				if (admin == null)
					throw new NotAuthorizedException();

				return admin;
			}
		}
	}
}