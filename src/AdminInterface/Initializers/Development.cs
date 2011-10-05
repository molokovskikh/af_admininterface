using System;
using System.IO;
using System.Linq;
using System.Web;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;

namespace AdminInterface.Initializers
{
	public class Development : IEnvironment
	{
		public void Run()
		{
			ADHelper.Storage = new MemoryUserStorage();

			var path = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath);
			if (Directory.Exists(Path.Combine(path, "bin")))
				path = Path.Combine(path, "bin", "Promotions");
			else
				path = Path.Combine(path, "Promotions");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			Global.Config.PromotionsPath = path;

			using(new SessionScope())
			{
				if (Administrator.GetByName(Environment.UserName) == null)
					Administrator.CreateLocalAdministrator();
			}
		}
	}
}