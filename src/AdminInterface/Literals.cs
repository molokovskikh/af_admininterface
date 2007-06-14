using System.Configuration;
using System;
using System.Web;


namespace AddUser
{
	public class Literals
	{
		public static string GetConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
		}
	}

	public enum StatisticsType
	{
		UpdateCumulative = 2,
		UpdateNormal = 1,
		UpdateError = 3,
		UpdateBan = 0,
		InUpdateProcess = 4,
		Download = 5
	}
}
