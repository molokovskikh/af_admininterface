using System;
using System.Web.UI;
using AddUser;

namespace AdminInterface.Helpers
{
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
