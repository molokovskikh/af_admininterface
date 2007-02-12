using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Reflection;

public class Utils
{
	public static string ExceptionToString(Exception exception)
	{
		StringBuilder builder = new StringBuilder();

		builder.AppendLine("----Error-----");
		do
		{
			builder.AppendLine("Message:");
			builder.AppendLine(exception.Message);
			builder.AppendLine("Stack Trace:");
			builder.AppendLine(exception.StackTrace);
			builder.AppendLine("--------------");
			exception = exception.InnerException;
		} while (exception != null);
		builder.AppendLine("--------------");

		builder.AppendLine(String.Format("Version : {0}", Assembly.GetExecutingAssembly().GetName().Version));
		return builder.ToString();
	}
}
