using System.Configuration;
using System;
using System.Web;

public class Literals
{
	public static string GetConnectionString()
	{
		return ConfigurationManager.AppSettings["ConnectionString"];
	}
}
