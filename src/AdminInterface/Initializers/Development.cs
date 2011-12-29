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
			var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

			config.AptBox = Path.Combine(dataPath, config.AptBox);
			config.OptBox = Path.Combine(dataPath, config.OptBox);
			config.PromotionsPath = Path.Combine(dataPath, config.PromotionsPath);
			config.CertificatesPath = Path.Combine(dataPath, config.CertificatesPath);
			config.AttachmentsPath = Path.Combine(dataPath, config.AttachmentsPath);

			config.PrinterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.PrinterPath);

			InitDirs(
				dataPath,
				config.AttachmentsPath,
				config.CertificatesPath,
				config.PromotionsPath
			);

			using(new SessionScope())
			{
				if (Administrator.GetByName(Environment.UserName) == null)
					Administrator.CreateLocalAdministrator();
			}
		}

		public void InitDirs(params string[] dirs)
		{
			foreach (var dir in dirs)
			{
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
		}
	}
}