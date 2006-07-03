using System.Configuration;
using System;
using System.Web;

public class Literals
{
	public static string GetConnectionString()
	{
		return String.Format(ConfigurationManager.AppSettings["ConnectionString"], HttpContext.Current.Session["UserName"]);
	}

	public static string GetRootConnectionString()
	{
		return String.Format(ConfigurationManager.AppSettings["ConnectionString"], "root");		
	}
}
