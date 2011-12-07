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

			var config = Global.Config;
			config.AptBox = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin", config.AptBox);
			config.OptBox = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin", config.OptBox);
			config.PromotionsPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin", config.PromotionsPath);
			config.PrinterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.PrinterPath);

			if (!Directory.Exists(config.PromotionsPath))
				Directory.CreateDirectory(config.PromotionsPath);

			using(new SessionScope())
			{
				if (Administrator.GetByName(Environment.UserName) == null)
					Administrator.CreateLocalAdministrator();
			}
		}
	}
}