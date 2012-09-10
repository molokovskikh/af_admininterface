using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;

namespace AdminInterface.Controllers
{
	public class DebugStatusColorController : MainController
	{
		public void DebugStatusColor(string OrderProcStatus, string PriceProcessorMasterStatus)
		{
			var statuses = new StatusServices(OrderProcStatus, PriceProcessorMasterStatus);
			Index(null, null);
			PropertyBag["ServisesStatus"] = statuses;
		}
	}
}