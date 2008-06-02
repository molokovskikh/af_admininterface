using System;
using System.Web;
using AdminInterface.Models;

namespace AdminInterface.Security
{
	public static class SecurityContext
	{
		private const string AdministratorKey = "Admin";

		public static Administrator Administrator
		{
			get
			{
				Administrator administrator = null;
#if !DEBUG
				administrator = (Administrator)HttpContext.Current.Session[AdministratorKey];
#endif
				if (administrator == null)
				{
					administrator = Administrator.GetByName(HttpContext.Current.User.Identity.Name);
#if !DEBUG
					if (administrator != null)
					{
						HttpContext.Current.Session["UserName"] = administrator.UserName;
						HttpContext.Current.Session[AdministratorKey] = administrator;
					}
#endif			
				}
				
				return administrator;
			}
		}

		public static void CheckIsUserAuthorized()
		{
			if (Administrator == null)
				throw new NotAuthorizedException();
		}
	}
}