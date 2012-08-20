using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))]
	public class RegionsController : AdminInterfaceController
	{
		public void Index()
		{
			PropertyBag["regions"] = Region.All();
		}

		public void Edit(ulong id)
		{
			var region = Region.Find(id);
			if (IsPost) {
				BindObjectInstance(region, "region");
				if (IsValid(region)) {
					region.Save();
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
			PropertyBag["region"] = region;
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId)
		{
			ShowRegions(clientId, homeRegionId, true, false);
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId, bool showDefaultRegions, bool showNonDefaultRegions)
		{
			var allRegions = Region.FindAll();
			if (homeRegionId.HasValue) {
				var homeRegion = Region.Find(homeRegionId.Value);
				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) <= 0);
				PropertyBag["homeRegion"] = Region.Find(homeRegionId);
			}
			else if (clientId.HasValue) {
				var client = DbSession.Load<Client>(clientId.Value);
				var drugstore = client.Settings;

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
			CancelLayout();
		}

		public void DefaultRegions(ulong homeRegionId, bool singleRegions)
		{
			var homeRegion = Region.Find(homeRegionId);
			var regions = Region.FindAll()
				.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0)
				.OrderBy(region => region.Name)
				.ToArray();
			PropertyBag["singleRegions"] = singleRegions;
			PropertyBag["regions"] = regions;
			PropertyBag["homeRegionId"] = homeRegion.Id;
			CancelLayout();
		}

		public void DefaultRegions(ulong homeRegionId, uint clientId)
		{
			var homeRegion = Region.Find(homeRegionId);
			var client = DbSession.Load<Client>(clientId);
			var drugstore = client.Settings;
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