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
		public static IViewEngineManager Init(Assembly assembly = null)
		{
			if (assembly == null)
				assembly = Assembly.GetEntryAssembly();

			ActiveRecordStarter.Initialize(
				new[] {
					Assembly.Load("AdminInterface"),
					Assembly.Load("Common.Web.Ui")
				},
				ActiveRecordSectionHandler.Instance);

			var config = new MonoRailConfiguration();
			config.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));

			var loader = new FileAssemblyViewSourceLoader("");
			loader.AddAssemblySource(new AssemblySourceInfo(assembly, assembly.GetName().Name + ".Views"));

			var provider = new FakeServiceProvider();
			provider.Services.Add(typeof(IMonoRailConfiguration), config);
			provider.Services.Add(typeof(IViewSourceLoader), loader);

			var manager = new DefaultViewEngineManager();
			manager.Service(provider);
			var options = ((BooViewEngine)manager.ResolveEngine(".brail")).Options;
			options.AssembliesToReference.Add(Assembly.Load("Common.Web.Ui"));
			var namespaces = options.NamespacesToImport;
			namespaces.Add("Boo.Lang.Builtins");
			namespaces.Add("AdminInterface.Helpers");
			namespaces.Add("Common.Web.Ui.Helpers");

			BaseMailer.ViewEngineManager = manager;
			return manager;
		}
	}
}