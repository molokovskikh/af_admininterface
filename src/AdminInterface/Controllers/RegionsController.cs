using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))]
	public class RegionsController : SmartDispatcherController
	{
		public void ShowRegions(ulong homeRegionId)
		{
			ShowRegions(homeRegionId, true, false);
		}

		public void ShowRegions(ulong homeRegionId, bool showDefaultRegions, bool showNonDefaultRegions)
		{
			var homeRegion = Region.Find(homeRegionId);
			var allRegions = Region.FindAll();

			if (showDefaultRegions)
				PropertyBag["defaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0);
			if (showNonDefaultRegions)
				PropertyBag["nonDefaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) <= 0);
			PropertyBag["homeRegion"] = Region.Find(homeRegionId);
		}
	}
}
