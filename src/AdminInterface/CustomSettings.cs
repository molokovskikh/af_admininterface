using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace AdminInterface
{
	public static class CustomSettings
	{
		public static string UserPreparedDataFormatString
		{
			get { return ConfigurationManager.AppSettings["UserPreparedDataFormatString"]; }
		}
	}
}
