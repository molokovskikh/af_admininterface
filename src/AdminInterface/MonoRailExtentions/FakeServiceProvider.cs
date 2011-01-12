using System;
using System.Collections.Generic;

namespace AdminInterface.MonoRailExtentions
{
	public class FakeServiceProvider : IServiceProvider
	{
		public Dictionary<Type, object> Services = new Dictionary<Type, object>();

		public object GetService(Type serviceType)
		{
			if (!Services.ContainsKey(serviceType))
				return null;
			return Services[serviceType];
		}
	}
}