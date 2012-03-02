﻿using System.IO;
using System.Reflection;
using AdminInterface.Initializers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Views.Brail;
using IgorO.ExposedObjectProject;
using log4net.Config;

namespace Integration.ForTesting
{
	public class ForTest
	{
		public static void InitialzeAR()
		{
			XmlConfigurator.Configure();
			if (!ActiveRecordStarter.IsInitialized)
				new ActiveRecord().Initialize(ActiveRecordSectionHandler.Instance);
		}

		public static IViewEngineManager GetViewManager()
		{
			var config = new MonoRailConfiguration();
			config.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));
			config.ViewEngineConfig.ViewPathRoot = Path.Combine(@"..\..\..\AdminInterface", "Views");

			var provider = new FakeServiceProvider();
			var loader = new FileAssemblyViewSourceLoader(config.ViewEngineConfig.ViewPathRoot);
			provider.Services.Add(typeof(IMonoRailConfiguration), config);
			provider.Services.Add(typeof(IViewSourceLoader), loader);

			var manager = new DefaultViewEngineManager();
			manager.Service(provider);
			var options = ExposedObject.From(manager).viewEnginesFastLookup[0].Options;
			var namespaces = options.NamespacesToImport;
			namespaces.Add("Boo.Lang.Builtins");
			namespaces.Add("AdminInterface.Helpers");
			namespaces.Add("Common.Web.Ui.Helpers");
			options.AssembliesToReference.Add(Assembly.Load("AdminInterface"));
			return manager;
		}

		public static void InitializeMailer()
		{
			BaseMailer.ViewEngineManager = GetViewManager();
		}
	}
}
