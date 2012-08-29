using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Web.Ui.Controllers;
using AdminInterface.MonoRailExtentions;

namespace AdminInterface.Controllers
{
	public class DebugController : AdminInterfaceController
	{
		public void TestLockTimeOut()
		{
			throw new Exception("Lock wait timeout exceeded;");
		}
	}
}