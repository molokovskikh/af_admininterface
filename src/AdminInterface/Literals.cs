using System.Configuration;

public class Literals
{
	public static string GetConnectionString()
	{
		return ConfigurationManager.AppSettings["ConnectionString"];
	}
}
