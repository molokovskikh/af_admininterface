using System;
using System.Web;
using AdminInterface.Models;

namespace AdminInterface.Security
{
	public static class SecurityContext
	{
		private const string AdministratorKey = "Admin";

		public static Func<Administrator> GetAdministrator
			= () =>
			  	{
			  		Administrator administrator = null;
#if !DEBUG
					administrator = (Administrator)HttpContext.Current.Session[AdministratorKey];
#endif
			  		if (administrator == null)
			  		{
			  			administrator = Administrator.GetByName(HttpContext.Current.User.Identity.Name);
			  			if (administrator != null)
			  			{
			  				HttpContext.Current.Session["UserName"] = administrator.UserName;
			  				HttpContext.Current.Session[AdministratorKey] = administrator;
			  			}
			  		}

			  		return administrator;

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