using System;
using System.Text;
using System.Reflection;

namespace AdminInterface.Helpers
{
	public class Utils
	{
		public static string ExceptionToString(Exception exception)
		{
			var builder = new StringBuilder();

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
}