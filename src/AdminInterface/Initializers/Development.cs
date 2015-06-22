﻿using System;
using System.IO;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.MonoRailExtentions;
using log4net;

namespace AdminInterface.Initializers
{
	public class Development : IEnvironment
	{
		public void Run()
		{
			ADHelper.Storage = new MemoryUserStorage();
			BaseRemoteRequest.Runner = new StubRequestRunner();

			var config = Global.Config;
			var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

			config.UserPreparedDataDirectory = Path.Combine(dataPath, config.UserPreparedDataDirectory);
			config.AptBox = Path.Combine(dataPath, config.AptBox);
			config.OptBox = Path.Combine(dataPath, config.OptBox);
			config.PromotionsPath = Path.Combine(dataPath, config.PromotionsPath);
			config.CertificatesPath = Path.Combine(dataPath, config.CertificatesPath);
			config.AttachmentsPath = Path.Combine(dataPath, config.AttachmentsPath);
			config.DocsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Docs");
			config.NewSupplierMailFilePath = Path.Combine(dataPath, config.NewSupplierMailFilePath);

			config.PrinterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.PrinterPath);

			InitDirs(
				dataPath,
				config.AttachmentsPath,
				config.CertificatesPath,
				config.PromotionsPath,
				config.UserPreparedDataDirectory);

			new Seed().Run();
		}

		public void InitDirs(params string[] dirs)
		{
			foreach (var dir in dirs) {
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
		}
	}
}