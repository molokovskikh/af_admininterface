using System;
using System.Linq;
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

			using(new SessionScope())
			{
				if (Administrator.GetByName(Environment.UserName) == null)
					Administrator.CreateLocalAdministrator();
			}
		}
	}
}