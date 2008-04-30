using NHibernate;

namespace AdminInterface.Helpers
{
	public class DbLogHelper
	{
		public static void SavePersistentWithLogParams(string user, string host, ISession session)
		{
			using (var command = session.Connection.CreateCommand())
			{
				command.CommandText = @"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;";
				var parameter = command.CreateParameter();
				parameter.Value = user;
				parameter.ParameterName = "?UserName";
				command.Parameters.Add(parameter);

				parameter = command.CreateParameter();
				parameter.Value = host;
				parameter.ParameterName = "?UserHost";
				command.Parameters.Add(parameter);

				command.ExecuteNonQuery();
			}
		}
	}
}
