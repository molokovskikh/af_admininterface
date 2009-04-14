using System.Configuration;

namespace AdminInterface.Helpers
{
	public class Literals
	{
		public static string GetConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
		}
	}

	public enum StatisticsType
	{
		UpdateCumulative = 2,
		UpdateNormal = 1,
		UpdateError = 3,
		UpdateBan = 0
	}
}