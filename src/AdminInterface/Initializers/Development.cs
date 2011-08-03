using System;
using System.Linq;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;

namespace AdminInterface.Initializers
{
	public class Development : IEnvironment
	{
		public void Run()
		{
			using(new SessionScope())
			{
				if (Administrator.GetByName(Environment.UserName) == null)
					Administrator.CreateLocalAdministrator();
			}
		}
	}
}