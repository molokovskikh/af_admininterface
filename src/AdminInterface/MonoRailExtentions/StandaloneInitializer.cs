using System.Reflection;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Views.Brail;

namespace AdminInterface.MonoRailExtentions
{
	public class StandaloneInitializer
	{
		public static IViewEngineManager Init()
		{
			ActiveRecordStarter.Initialize(
				new[] {
					Assembly.Load("AdminInterface"),
					Assembly.Load("Common.Web.Ui")
				},
				ActiveRecordSectionHandler.Instance);

			var config = new MonoRailConfiguration();
			config.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));

			var loader = new FileAssemblyViewSourceLoader("");
			loader.AddAssemblySource(new AssemblySourceInfo(Assembly.GetExecutingAssembly(), "AdminInterface.Background.Views"));

			var provider = new FakeServiceProvider();
			provider.Services.Add(typeof(IMonoRailConfiguration), config);
			provider.Services.Add(typeof(IViewSourceLoader), loader);

			var manager = new DefaultViewEngineManager();
			manager.Service(provider);
			var namespaces = ((BooViewEngine)manager.ResolveEngine(".brail")).Options.NamespacesToImport;
			namespaces.Add("Boo.Lang.Builtins");
			namespaces.Add("AdminInterface.Helpers");
			BaseMailer.ViewEngineManager = manager;
			return manager;
		}
	}
}