using Topshelf.Configuration.Dsl;
using Topshelf.Shelving;
using log4net.Config;

namespace AdminInterface.Background
{
	public class Bootstrapper : Bootstrapper<Waiter>
	{
		public void InitializeHostedService(IServiceConfigurator<Waiter> cfg)
		{
			XmlConfigurator.Configure();
			cfg.HowToBuildService(n => new Waiter());
			cfg.WhenStarted(s => s.DoStart());
			cfg.WhenStopped(s => s.Stop());
		}
	}
}