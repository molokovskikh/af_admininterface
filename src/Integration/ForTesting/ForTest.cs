using AdminInterface.Initializers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
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
	}
}
