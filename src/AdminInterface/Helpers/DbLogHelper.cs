using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.ActiveRecord;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class DbLogHelper
	{
		public static void SavePersistentWithLogParams(string user, string host, ActiveRecordBase persistent)
		{
			using (new TransactionScope())
			{
				ISession session = ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(persistent.GetType());
				using (IDbCommand command = session.Connection.CreateCommand())
				{
					command.CommandText = @"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;";
					IDbDataParameter parameter = command.CreateParameter();
					parameter.Value = user;
					parameter.ParameterName = "?UserName";
					command.Parameters.Add(parameter);

					parameter = command.CreateParameter();
					parameter.Value = host;
					parameter.ParameterName = "?UserHost";
					command.Parameters.Add(parameter);

					command.ExecuteNonQuery();
				}
				persistent.SaveAndFlush();
			}
		}
	}
}
