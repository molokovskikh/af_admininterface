using System;
using System.Reflection;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class DbLogHelper : DbLogHelperBase
	{
		public static void SetupParametersForTriggerLogging()
		{
			ArHelper.WithSession(session => SetupParametersForTriggerLogging(new {
					InUser = SecurityContext.Administrator.UserName,
					InHost = SecurityContext.Administrator.Host
				},
				session));
		}
	}
}
