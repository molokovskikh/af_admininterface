using System;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using AddUser;
using AdminInterface.Filters;
using AdminInterface.Models;

namespace AdminInterface.Helpers
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

	public class StateHelper
	{
		private const string SessionIdKey = "SessionIdKey";

		public static void CheckSession(Page page, StateBag ViewState)
		{
			if (page.IsPostBack)
			{
				if (ViewState[SessionIdKey] == null)
					throw new SessionOutDateException();

				if (page.Session.LCID != Convert.ToInt32(ViewState[SessionIdKey]))
					throw new SessionOutDateException();				
			}
			else
			{
				ViewState[SessionIdKey] = page.Session.LCID;
			}
		}
	}
}
