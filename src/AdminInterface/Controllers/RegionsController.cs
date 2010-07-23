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
		public void ShowRegions(uint? clientId, ulong? homeRegionId)
		{
			ShowRegions(clientId, homeRegionId, true, false);	
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId, bool showDefaultRegions, bool showNonDefaultRegions)
		{
			var allRegions = Region.FindAll();
			if (homeRegionId.HasValue)
			{
				var homeRegion = Region.Find(homeRegionId.Value);
				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) <= 0);
				PropertyBag["homeRegion"] = Region.Find(homeRegionId);
			}
			else if (clientId.HasValue)
			{
				var client = Client.Find(clientId.Value);
				var drugstore = DrugstoreSettings.Find(clientId.Value);

				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region =>
						(region.Id & client.MaskRegion) > 0 ||
						(region.Id & drugstore.OrderRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region =>
						(region.Id & client.MaskRegion) <= 0 ||
						(region.Id & drugstore.OrderRegionMask) <= 0);
				PropertyBag["homeRegion"] = client.HomeRegion;
				PropertyBag["drugstore"] = drugstore;
			}
		}

		public void DefaultRegions(ulong homeRegionId)
		{
			var homeRegion = Region.Find(homeRegionId);
			var regions = Region.FindAll()
				.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0)
				.OrderBy(region => region.Name)
				.ToArray();
			PropertyBag["regions"] = regions;
			PropertyBag["homeRegionId"] = homeRegion.Id;
			CancelLayout();
		}

		public void DefaultRegions(ulong homeRegionId, uint clientId)
		{
			var homeRegion = Region.Find(homeRegionId);
			var client = Client.Find(clientId);
			var drugstore = DrugstoreSettings.Find(clientId);
			var regions = Region.FindAll()
				.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0 ||
					((region.Id & client.MaskRegion) > 0) ||
					((region.Id & drugstore.OrderRegionMask) > 0))
				.OrderBy(region => region.Name)
				.ToArray();
			PropertyBag["regions"] = regions;
			PropertyBag["client"] = client;
			PropertyBag["drugstore"] = drugstore;
			PropertyBag["homeRegionId"] = homeRegion.Id;
			CancelLayout();
		}
	}
}
