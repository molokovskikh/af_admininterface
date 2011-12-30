using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[
		Layout("General"),
		Secure,
	]
	public class ManagerReportsController: ARSmartDispatcherController
	{
		public void Main()
		{
		}
	}
}