using System;
using System.Reflection;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class DbLogHelper
	{
		public static void SetupParametersForTriggerLogging()
		{
			ArHelper.WithSession(session => SetupParametersForTriggerLogging(new { InUser = SecurityContext.Administrator.UserName, InHost = SecurityContext.Administrator.GetHost() }, session));
		}

		public static void SetupParametersForTriggerLogging(string user, string host)
		{
			ArHelper.WithSession(session => SetupParametersForTriggerLogging(new { InUser = user, InHost = host }, session));
		}

		public static void SetupParametersForTriggerLogging(object parameters)
		{
			ArHelper.WithSession(session => SetupParametersForTriggerLogging(parameters, session));
		}

		private static void SetupParametersForTriggerLogging(object parameters, ISession session)
		{
			using (var command = session.Connection.CreateCommand())
			{
				foreach (var property in parameters.GetType().GetProperties(BindingFlags.GetProperty
																					 | BindingFlags.Public
																					 | BindingFlags.Instance))
				{
					var value = property.GetValue(parameters, null);
					command.CommandText += String.Format(" SET @{0} = ?{0}; ", property.Name);
					var parameter = command.CreateParameter();
					parameter.Value = value;
					parameter.ParameterName = "?" + property.Name;
					command.Parameters.Add(parameter);
				}
				if (command.Parameters.Count == 0)
					return;

				command.ExecuteNonQuery();
			}
		}
	}
}
