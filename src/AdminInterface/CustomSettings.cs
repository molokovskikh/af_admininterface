using System;
using System.Collections.Generic;
using System.IO;
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

		public static string PromotionsPath()
		{
#if !DEBUG
			return ConfigurationManager.AppSettings["PromotionsPath"];
#else
			var path = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath);
			if (Directory.Exists(Path.Combine(path, "bin")))
				path = Path.Combine(path, "bin", "Promotions");
			else
				path = Path.Combine(path, "Promotions");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return path;
#endif
		}
	}
}
